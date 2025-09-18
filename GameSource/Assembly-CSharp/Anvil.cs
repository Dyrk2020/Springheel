using System;
using UnityEngine;

public class Anvil : MonoBehaviour
{
	public Thwomp thwomp;

	[SerializeField]
	private Collider2D solidCollider;

	[SerializeField]
	private Collider2D floorDetectorCollider;

	public float fallSpeed = 1f;

	public float fallMaxDistance = 10f;

	public float returnSpeed = 10f;

	public float launchStrength = 25f;

	private Animator animator;

	private Transform anvilTransform;

	private Vector3 startLocalPosition;

	private float elapsedFallingDistance;

	private static readonly RaycastHit2D[] floorDetectionResults = new RaycastHit2D[8];

	private static readonly RaycastHit2D[] playerDetectionResults = new RaycastHit2D[8];

	private static readonly Collider2D[] overlapResults = new Collider2D[8];

	public Collider2D crumbleBlockTrigger;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		anvilTransform = base.transform;
		if (base.transform.parent == null)
		{
			throw new InvalidOperationException("Anvil requires a parent Transform to calculate local coordinates.");
		}
		startLocalPosition = base.transform.localPosition;
		crumbleBlockTrigger.enabled = false;
	}

	public void UpdateColliders(ThwompState state)
	{
		floorDetectorCollider.enabled = state == ThwompState.FALL;
		crumbleBlockTrigger.enabled = state == ThwompState.FALL;
	}

	public Collider2D DetectFloor()
	{
		float distance = ModifiedFallSpeed(Time.deltaTime);
		int num = floorDetectorCollider.Cast(-base.transform.up, floorDetectionResults, distance);
		for (int i = 0; i < num; i++)
		{
			Collider2D collider = floorDetectionResults[i].collider;
			CollisionTag component = collider.GetComponent<CollisionTag>();
			if (component != null && component.ContainsAnyTag(TagComparer.Tag.Solid) && !component.ContainsAnyTag(TagComparer.Tag.Player))
			{
				return collider;
			}
		}
		return null;
	}

	public void StartFall()
	{
		PlayFallingAnimation();
		elapsedFallingDistance = 0f;
	}

	public void FallStep(float deltaTime)
	{
		float num = ModifiedFallSpeed(deltaTime);
		elapsedFallingDistance += num;
		MoveDown(num);
	}

	public void FallPassthroughStep()
	{
		float distance = ModifiedFallSpeed(Time.deltaTime);
		MoveDown(distance);
	}

	public bool HasExceededMaxFallDistance()
	{
		return elapsedFallingDistance >= fallMaxDistance;
	}

	public void Ground(Collider2D floorCollider)
	{
		AlignWithPlatform(floorCollider);
		PlayHitAnimation();
	}

	public void WaitInMidAir()
	{
		PlayHitAnimation();
	}

	public void ReturnStep(float deltaTime)
	{
		float maxDistance = ModifiedReturnSpeed(deltaTime);
		MoveTowardsStartPosition(maxDistance);
	}

	public void Pause()
	{
		animator.speed = 0f;
	}

	public void Unpause()
	{
		animator.speed = 1f;
	}

	public void Reset()
	{
		ResetToStartPosition();
		PlayIdleAnimation();
		elapsedFallingDistance = 0f;
	}

	public void ResetToStartPosition()
	{
		SetLocalPositionTo(startLocalPosition);
	}

	public bool HasReachedStartPosition()
	{
		return Vector3.Distance(base.transform.localPosition, startLocalPosition) < 0.01f;
	}

	private void AlignWithPlatform(Collider2D groundCollider)
	{
		if (!(groundCollider == null))
		{
			ColliderDistance2D colliderDistance2D = Physics2D.Distance(solidCollider, groundCollider);
			if (colliderDistance2D.isValid)
			{
				Vector2 vector = colliderDistance2D.pointB - colliderDistance2D.pointA;
				Vector3 position = (Vector2)anvilTransform.position + vector;
				Vector3 localPositionTo = base.transform.parent.InverseTransformPoint(position);
				SetLocalPositionTo(localPositionTo);
			}
		}
	}

	private void MoveDown(float distance)
	{
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, base.transform.localPosition + Vector3.down * fallMaxDistance, distance);
	}

	private void MoveTowardsStartPosition(float maxDistance)
	{
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = Vector3.MoveTowards(localPosition, startLocalPosition, maxDistance);
	}

	private void MovePhysicsToLocalPosition(Vector3 targetLocalPosition)
	{
		targetLocalPosition.x = startLocalPosition.x;
		targetLocalPosition.z = startLocalPosition.z;
		Vector2 vector = base.transform.parent.TransformPoint(targetLocalPosition);
		anvilTransform.position = vector;
	}

	private void SetLocalPositionTo(Vector3 targetLocalPosition)
	{
		targetLocalPosition.x = startLocalPosition.x;
		targetLocalPosition.z = startLocalPosition.z;
		base.transform.localPosition = targetLocalPosition;
		Vector2 physicsPositionFromWorld = base.transform.position;
		SetPhysicsPositionFromWorld(physicsPositionFromWorld);
	}

	private void SetPhysicsPositionFromWorld(Vector2 targetWorldPosition)
	{
		anvilTransform.position = targetWorldPosition;
		base.transform.position = new Vector3(targetWorldPosition.x, targetWorldPosition.y, base.transform.position.z);
	}

	private float ModifiedFallSpeed(float deltaTime)
	{
		return fallSpeed * Mathf.Clamp(Modifiers.GetInstance().PlatformMoveSpeed, 0f, 2.5f) * deltaTime;
	}

	private float ModifiedReturnSpeed(float deltaTime)
	{
		return returnSpeed * Modifiers.GetInstance().PlatformMoveSpeed * deltaTime;
	}

	private void PlayFallingAnimation()
	{
		animator.Play("AnvilDropperAnvilFalling", 0, 0f);
	}

	private void PlayHitAnimation()
	{
		animator.SetTrigger("AnvilHit");
	}

	private void PlayIdleAnimation()
	{
		animator.Play("AnvilDropperAnvilIdle", 0, 0f);
	}

	public void PlayAnticipationAnimation()
	{
		animator.SetTrigger("Anticipation");
	}
}
