using UnityEngine;
using UnityEngine.Networking;

public class DancingMoverRecordPlayer : MonoBehaviour
{
	public enum DancingMoverState
	{
		IDLE,
		STAND_BY,
		MOVE,
		RETURN
	}

	public enum MovePhase
	{
		RIGHT,
		UP,
		BOTTOM,
		LEFT
	}

	public DancingMovingMultipiecePlatform dancingPlatform;

	public MovePhase currentMovePhase;

	public DancingMoverState movementState = DancingMoverState.STAND_BY;

	public Animator recordPlayerAnimator;

	public Transform destination;

	public float movementSpeed = 5f;

	public float returningDelaySeconds = 1f;

	private Vector3 localDestinationPosition;

	private bool isMovementPaused;

	private bool destinationReached;

	private const string IDLE_ANIMATION_NAME = "RecordPlayerIdle";

	private const string ACTIVE_ANIMATION_NAME = "RecordPlayerActive";

	private float elapsedTimeBeforeReturning;

	public AnimationCurve movementEaseCurveX;

	public AnimationCurve movementEaseCurveY;

	protected float currentMovementSpeed;

	public float accelerationValue;

	protected float currentMassRatio = 1f;

	private MultipiecePart multipiecePart;

	protected void Start()
	{
		multipiecePart = GetComponent<MultipiecePart>();
		CalculateLocalDestination();
	}

	private void CalculateLocalDestination()
	{
		if (destination != null && base.transform.parent != null)
		{
			localDestinationPosition = base.transform.parent.InverseTransformPoint(destination.position);
		}
		else if (destination != null)
		{
			localDestinationPosition = destination.position;
		}
	}

	protected void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			ProcessServerMovement();
		}
		if (multipiecePart != null)
		{
			multipiecePart.relativeAttachPosition = base.transform.localPosition;
		}
	}

	private void ProcessServerMovement()
	{
		float target = 0f;
		if (!isMovementPaused && !dancingPlatform.PickedUp)
		{
			if (movementState == DancingMoverState.MOVE || movementState == DancingMoverState.RETURN)
			{
				target = CalculateModifiedMovementSpeed(Time.deltaTime);
			}
			if (movementState == DancingMoverState.RETURN && elapsedTimeBeforeReturning < returningDelaySeconds)
			{
				elapsedTimeBeforeReturning += Time.deltaTime;
				return;
			}
			currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, target, CalculateModifiedAccelerationSpeed() * Time.deltaTime);
			ExecuteMovementPhase(currentMovementSpeed);
		}
	}

	private void ExecuteMovementPhase(float maximumDistanceDelta)
	{
		switch (currentMovePhase)
		{
		case MovePhase.RIGHT:
			MoveTowardsTargetHorizontal(localDestinationPosition.x, maximumDistanceDelta, MovePhase.UP);
			break;
		case MovePhase.UP:
			MoveTowardsTargetVertical(localDestinationPosition.y, maximumDistanceDelta, MovePhase.BOTTOM);
			break;
		case MovePhase.BOTTOM:
			MoveTowardsTargetVertical(Vector3.zero.y, maximumDistanceDelta, MovePhase.LEFT);
			break;
		case MovePhase.LEFT:
			MoveTowardsTargetHorizontal(Vector3.zero.x, maximumDistanceDelta, MovePhase.RIGHT);
			CheckReturnToOriginCompletion();
			break;
		}
	}

	private float CalculateModifiedMovementSpeed(float deltaTime)
	{
		return movementSpeed * Mathf.Clamp(Modifiers.GetInstance().PlatformMoveSpeed, 0.1f, 3f) * deltaTime;
	}

	private float CalculateModifiedAccelerationSpeed()
	{
		return accelerationValue * Mathf.Clamp(Modifiers.GetInstance().PlatformMoveSpeed, 0.75f, 3f);
	}

	private void MoveTowardsTargetHorizontal(float targetHorizontalPosition, float maximumDistanceDelta, MovePhase nextPhase)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = targetHorizontalPosition;
		float time = Mathf.Abs(base.transform.localPosition.x - localPosition.x);
		float num = movementEaseCurveX.Evaluate(time);
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, localPosition, maximumDistanceDelta * num / currentMassRatio);
		if (TargetPositionHorizontalReached(targetHorizontalPosition))
		{
			currentMovePhase = nextPhase;
			CheckDestinationReached(nextPhase);
		}
	}

	private void MoveTowardsTargetVertical(float targetVerticalPosition, float maximumDistanceDelta, MovePhase nextPhase)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = targetVerticalPosition;
		float time = Mathf.Abs(base.transform.localPosition.y - localPosition.y);
		float num = movementEaseCurveY.Evaluate(time);
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, localPosition, maximumDistanceDelta * num / currentMassRatio);
		if (TargetPositionVerticalReached(targetVerticalPosition))
		{
			currentMovePhase = nextPhase;
			CheckDestinationReached(nextPhase);
		}
	}

	private void CheckDestinationReached(MovePhase nextPhase)
	{
		if (movementState == DancingMoverState.MOVE && nextPhase == MovePhase.BOTTOM)
		{
			destinationReached = true;
		}
		if (movementState == DancingMoverState.MOVE && nextPhase == MovePhase.RIGHT)
		{
			destinationReached = false;
		}
	}

	private void CheckReturnToOriginCompletion()
	{
		if (movementState == DancingMoverState.RETURN && TargetPositionReached(Vector3.zero))
		{
			AkSoundEngine.PostEvent("SFX_DancingMover_Return_Stop", base.gameObject);
			Deactivate();
		}
	}

	private void PlayRecordPlayerAnimation(string animationName)
	{
		if (recordPlayerAnimator != null)
		{
			recordPlayerAnimator.Play(animationName, 0, 0f);
		}
	}

	public void Activate()
	{
		if (NetworkServer.active && !TargetPositionReached(Vector3.zero))
		{
			ForceMovePhaseToTargetDestination();
		}
		currentMassRatio = dancingPlatform.calculateMassRatio();
		movementState = DancingMoverState.MOVE;
		PlayRecordPlayerAnimation("RecordPlayerActive");
		AkSoundEngine.PostEvent("SFX_DancingMover_Start", base.gameObject);
	}

	public void StandBy()
	{
		if (movementState != DancingMoverState.STAND_BY)
		{
			AkSoundEngine.PostEvent("SFX_DancingMover_Stop", base.gameObject);
		}
		movementState = DancingMoverState.STAND_BY;
		PlayRecordPlayerAnimation("RecordPlayerIdle");
	}

	public void Deactivate()
	{
		if (NetworkServer.active && !TargetPositionReached(Vector3.zero))
		{
			SetReturnToOriginPhase();
		}
		else
		{
			ResetPlatformState();
		}
	}

	private void SetReturnToOriginPhase()
	{
		PlayRecordPlayerAnimation("RecordPlayerIdle");
		AkSoundEngine.PostEvent("SFX_DancingMover_Stop", base.gameObject);
		AkSoundEngine.PostEvent("SFX_DancingMover_Return", base.gameObject);
		movementState = DancingMoverState.RETURN;
		currentMovementSpeed = 0f;
		if (NetworkServer.active)
		{
			elapsedTimeBeforeReturning = 0f;
			ForceMovePhaseToTargetOrigin();
		}
	}

	private void ForceMovePhaseToTargetOrigin()
	{
		switch (currentMovePhase)
		{
		case MovePhase.RIGHT:
			currentMovePhase = MovePhase.LEFT;
			break;
		case MovePhase.UP:
			currentMovePhase = MovePhase.BOTTOM;
			break;
		}
	}

	private void ForceMovePhaseToTargetDestination()
	{
		if (!destinationReached)
		{
			switch (currentMovePhase)
			{
			case MovePhase.BOTTOM:
				currentMovePhase = MovePhase.UP;
				break;
			case MovePhase.LEFT:
				currentMovePhase = MovePhase.RIGHT;
				break;
			}
		}
	}

	public bool TargetPositionReached(Vector3 targetPosition)
	{
		if (TargetPositionHorizontalReached(targetPosition.x))
		{
			return TargetPositionVerticalReached(targetPosition.y);
		}
		return false;
	}

	private bool TargetPositionHorizontalReached(float targetHorizontalPosition)
	{
		return Mathf.Approximately(base.transform.localPosition.x, targetHorizontalPosition);
	}

	private bool TargetPositionVerticalReached(float targetVerticalPosition)
	{
		return Mathf.Approximately(base.transform.localPosition.y, targetVerticalPosition);
	}

	public void Reset()
	{
		ResetPlatformState();
		isMovementPaused = false;
		base.transform.localPosition = Vector3.zero;
		if (multipiecePart != null)
		{
			multipiecePart.relativeAttachPosition = Vector3.zero;
		}
	}

	private void ResetPlatformState()
	{
		PlayRecordPlayerAnimation("RecordPlayerIdle");
		if (movementState != DancingMoverState.STAND_BY)
		{
			AkSoundEngine.PostEvent("SFX_DancingMover_Stop", base.gameObject);
			AkSoundEngine.PostEvent("SFX_DancingMover_Return_Stop", base.gameObject);
		}
		movementState = DancingMoverState.STAND_BY;
		currentMovementSpeed = 0f;
		currentMovePhase = MovePhase.RIGHT;
		destinationReached = false;
		elapsedTimeBeforeReturning = 0f;
	}

	public void Pause()
	{
		isMovementPaused = true;
		SetAnimatorSpeedMultiplier(0f);
	}

	public void Unpause()
	{
		isMovementPaused = false;
		SetAnimatorSpeedMultiplier(1f);
	}

	private void SetAnimatorSpeedMultiplier(float speedMultiplier)
	{
		if (recordPlayerAnimator != null)
		{
			recordPlayerAnimator.speed = speedMultiplier;
		}
	}
}
