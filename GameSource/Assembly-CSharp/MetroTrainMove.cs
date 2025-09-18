using GameEvent;
using UnityEngine;

public class MetroTrainMove : ActiveBlock
{
	public Animator MetroTrainAnimator;

	public string StartSoundFXString;

	public string StopSoundFXString;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	private Collider2D[] colliders;

	public Vector2 Velocity => velocity;

	protected override void Awake()
	{
		base.Awake();
		colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
	}

	protected override void Start()
	{
		base.Start();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
		GameEventManager.ChangeListener<PlayerSucceedEvent>(this, adding: true);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		GameEventManager.ChangeListener<PlayerSucceedEvent>(this, adding: false);
	}

	protected override void Activate()
	{
		base.Activate();
		BoxCollider2D[] componentsInChildren = GetComponentsInChildren<BoxCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: true);
		}
		MetroTrainAnimator.SetBool("Moving", value: true);
		AkSoundEngine.PostEvent(StartSoundFXString, base.gameObject);
	}

	protected override YieldInstruction TimeToWaitForResetReactivation()
	{
		return new WaitForSeconds(3f);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			Vector3 position = MetroTrainAnimator.transform.position;
			velocity = ((Vector2)position - lastPosition) / deltaTime;
			lastPosition = position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		velocity = Vector3.zero;
		MetroTrainAnimator.SetBool("Moving", value: false);
		AkSoundEngine.PostEvent(StopSoundFXString, base.gameObject);
		BoxCollider2D[] componentsInChildren = GetComponentsInChildren<BoxCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
	}

	public override void Pause()
	{
		base.Pause();
		MetroTrainAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		MetroTrainAnimator.speed = 1f;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
		}
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
		}
		return pms;
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e is PlayerSucceedEvent playerSucceedEvent)
		{
			playerSucceedEvent.Character.TemporaryIgnoreCollision(colliders);
		}
	}
}
