using System;
using GameEvent;
using UnityEngine;

public class FallingIce : MonoBehaviour, IGameEventListener
{
	public Vector2 Direction;

	public Sprite[] Images;

	public float Speed;

	public float rotateSpeed;

	public Bounds killBoundary;

	public float Lifespan;

	public bool UseGravity;

	public float GravityScale = 1f;

	private bool collided;

	public string HitSoundEvent;

	public string HitSoundEventPlayer;

	protected Vector3 pauseVel = Vector3.zero;

	public GameObject collidedWithObject;

	public GameObject debris;

	public SpriteRenderer spriteRenderer;

	private float timeAlive;

	private bool scoreboard;

	public int placedByPlayerNumber;

	public float NotDeadlyTime = 0.5f;

	private static TagComparer.Tag solidPlayerMask = (TagComparer.Tag)160;

	private static TagComparer.Tag triggerCheckMask = TagComparer.Tag.Solid;

	private static TagComparer.Tag triggerIgnoreMask = (TagComparer.Tag)65568;

	public bool Paused { get; protected set; }

	protected void Start()
	{
		GetComponent<Rigidbody2D>().velocity = base.transform.up * Speed;
		killBoundary.center = base.transform.position;
		pauseVel = base.transform.up * Speed;
		ChangeListener(adding: true);
		GetComponent<Collider2D>().enabled = false;
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	private void FixedUpdate()
	{
		if (Paused || scoreboard)
		{
			return;
		}
		timeAlive += Time.fixedDeltaTime;
		if (timeAlive > NotDeadlyTime)
		{
			GetComponent<Collider2D>().enabled = true;
		}
		if (collided || !killBoundary.Contains(base.transform.position) || timeAlive > Lifespan)
		{
			if ((bool)debris)
			{
				UnityEngine.Object.Instantiate(debris, base.transform.position, base.transform.rotation).transform.localScale = base.transform.localScale;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (UseGravity)
		{
			GetComponent<Rigidbody2D>().gravityScale = GravityScale;
		}
	}

	private void OnCollisionEnter2D(Collision2D c)
	{
		if (GameState.GetInstance().Paused || !(c.gameObject.GetComponent<ProjectileLauncher>() == null))
		{
			return;
		}
		collided = true;
		CollisionTag component = c.gameObject.GetComponent<CollisionTag>();
		if (IsSolidPlayer(c.gameObject, component))
		{
			if (HitSoundEventPlayer != "")
			{
				AkSoundEngine.PostEvent(HitSoundEventPlayer, base.gameObject);
			}
		}
		else if (HitSoundEvent != "")
		{
			AkSoundEngine.PostEvent(HitSoundEvent, base.gameObject);
		}
	}

	private bool IsSolidPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAllTags(solidPlayerMask);
		}
		return false;
	}

	private bool CollidesWithTrigger(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			if (collisionTag.ContainsAnyTag(triggerCheckMask))
			{
				return !collisionTag.ContainsAnyTag(triggerIgnoreMask);
			}
			return false;
		}
		return false;
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (GameState.GetInstance().Paused || !CollidesWithTrigger(c.gameObject, component))
		{
			return;
		}
		collidedWithObject = c.gameObject;
		collided = true;
		if (IsSolidPlayer(c.gameObject, component))
		{
			if (HitSoundEventPlayer != "")
			{
				AkSoundEngine.PostEvent(HitSoundEventPlayer, base.gameObject);
			}
		}
		else if (HitSoundEvent != "")
		{
			AkSoundEngine.PostEvent(HitSoundEvent, base.gameObject);
		}
	}

	private void OnDestroy()
	{
		GetComponent<Collider2D>().enabled = false;
		spriteRenderer.enabled = false;
		ChangeListener(adding: false);
	}

	public void Pause()
	{
		pauseVel = GetComponent<Rigidbody2D>().velocity;
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
		if (UseGravity)
		{
			GetComponent<Rigidbody2D>().gravityScale = 0f;
		}
	}

	public void Unpause()
	{
		if (!scoreboard && !Paused)
		{
			GetComponent<Rigidbody2D>().velocity = pauseVel;
			if (UseGravity)
			{
				GetComponent<Rigidbody2D>().gravityScale = GravityScale;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Paused = true;
				Pause();
			}
			else
			{
				Paused = false;
				Unpause();
			}
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
			}
			else
			{
				scoreboard = false;
			}
		}
	}
}
