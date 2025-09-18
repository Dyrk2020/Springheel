using GameEvent;
using UnityEngine;

public class ButtonSlide : MonoBehaviour, IGameEventListener
{
	public float slideDistance;

	public float triggerDistance;

	public float pushDownVelocityTrigger;

	public float resetTime;

	private float timer;

	public AnimationCurve pushupForce;

	public float ForceMultiplier;

	public float velocityFriction;

	public characterCounter characterCounter;

	public float mass;

	public float extraMassFactor;

	public Animator lockoutButtonSign;

	private Rigidbody2D rb;

	public float pushForce;

	public bool TriggeredThisFrame;

	protected bool withAudio;

	public bool TriggerNetwork = true;

	protected Vector3 initialPosition;

	private static TagComparer.Tag solidPlayerMask = (TagComparer.Tag)160;

	public bool Locked { get; protected set; }

	public bool WithAudio
	{
		get
		{
			return withAudio;
		}
		set
		{
			withAudio = value;
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		initialPosition = base.transform.position;
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: true);
	}

	private void FixedUpdate()
	{
		rb.mass = (mass + mass * extraMassFactor * (float)Mathf.Max(characterCounter.overlaps - 1, 0)) * Modifiers.GetInstance().GravityScale;
		timer += Time.fixedDeltaTime;
		if (!Locked)
		{
			if (rb.position.y > initialPosition.y)
			{
				rb.MovePosition(initialPosition);
				rb.position = initialPosition;
				rb.velocity = Vector2.zero;
				return;
			}
			if (rb.position.y < initialPosition.y - slideDistance)
			{
				rb.position = new Vector2(0f, initialPosition.y - slideDistance);
			}
			float num = pushupForce.Evaluate((initialPosition.y - rb.position.y) / slideDistance) * ForceMultiplier * Time.fixedDeltaTime;
			rb.velocity = new Vector2(0f, (rb.velocity.y + num) * velocityFriction);
		}
		else
		{
			rb.MovePosition(initialPosition);
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.x = initialPosition.x;
		base.transform.position = position;
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: false);
	}

	private bool IsSolidPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAllTags(solidPlayerMask);
		}
		return false;
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (Locked || !(timer > resetTime))
		{
			return;
		}
		CollisionTag component = collision.gameObject.GetComponent<CollisionTag>();
		if (IsSolidPlayer(collision.gameObject, component) && base.transform.position.y <= initialPosition.y - triggerDistance && collision.rigidbody.velocity.y < pushDownVelocityTrigger)
		{
			Character component2 = collision.gameObject.GetComponent<Character>();
			if (component2 != null && component2.hasAuthority)
			{
				TriggeredThisFrame = true;
				AkSoundEngine.PostEvent("SFX_" + component2.CharacterSFXNameNoCustom + "_StepOnPartyModeButton", component2.gameObject);
			}
			timer = 0f;
		}
	}

	public void SimulatePress(bool hasAudio = true)
	{
		TriggeredThisFrame = true;
		timer = 0f;
		WithAudio = hasAudio;
	}

	public void Lock()
	{
		Locked = true;
		lockoutButtonSign.SetBool("LockedOut", value: true);
		GameSettings.GetInstance().ModeLocked = true;
	}

	public void Unlock()
	{
		Locked = false;
		lockoutButtonSign.SetBool("LockedOut", value: false);
		GameSettings.GetInstance().ModeLocked = false;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetGameModeLock)
		{
			if (((MsgSetGameModeLock)networkMessageReceivedEvent.ReadMessage).Locked)
			{
				Lock();
				GameSettings.GetInstance().LockPartyButton = true;
			}
			else
			{
				Unlock();
				GameSettings.GetInstance().LockPartyButton = false;
			}
		}
	}
}
