using UnityEngine;

public class Stopwatch : ActiveBlock
{
	public GameObject pickupEffect;

	protected Vector3 startPosition;

	protected Animator animator;

	public AnimationCurve TimeSlowCurve;

	public float SlowRampTime;

	public float SlowDuration;

	public float SlowSpeed = 0.5f;

	private bool triggered;

	protected bool AlwaysRespawn
	{
		get
		{
			if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
			{
				return GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE;
			}
			return true;
		}
	}

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
	}

	private void Update()
	{
		if (disabled)
		{
			return;
		}
		if (triggered)
		{
			if (!Timekeeper.HasSource(this) && !AlwaysRespawn)
			{
				if (ParentPiece != null)
				{
					ParentPiece.DetachPiece(this, removeFromGroup: false);
				}
				SwitchColliderTo(ColliderModeEnum.NoColliders);
				base.MarkedForDestruction = true;
			}
		}
		else if (NetSurrogate != null && NetSurrogate.BoolVal)
		{
			triggerSlowdown();
		}
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		startPosition = base.transform.position;
	}

	public override void Pause()
	{
		base.Pause();
		animator.speed = 0f;
		Timekeeper.PauseSlowMoSource(this);
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
		Timekeeper.UnpauseSlowMoSource(this);
	}

	private void triggerSlowdown()
	{
		if (triggered)
		{
			return;
		}
		triggered = true;
		AkSoundEngine.PostEvent("SFX_Pieces_Stop_Watch_Picked_Up", base.gameObject);
		if (!NetSurrogate.BoolVal)
		{
			NetSurrogate.BoolVal = true;
		}
		if ((bool)pickupEffect)
		{
			Object.Instantiate(pickupEffect, base.transform.position, base.transform.rotation);
		}
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = false;
		}
		Timekeeper.AddSlowMoSource(this, SlowSpeed * Modifiers.GetInstance().GameSpeed, SlowDuration, SlowRampTime, TimeSlowCurve);
		if (!AlwaysRespawn)
		{
			if (ParentPiece != null)
			{
				ParentPiece.DetachPiece(this, removeFromGroup: false);
			}
			base.MarkedForDestruction = true;
		}
	}

	protected override void Activate()
	{
		Timekeeper.RemoveSlowMoSource(this);
	}

	public override void Disable()
	{
		base.Disable();
		Timekeeper.RemoveSlowMoSource(this);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (base.Active)
		{
			Character componentInParent = c.gameObject.GetComponentInParent<Character>();
			if (!(componentInParent == null) && !componentInParent.Dead && !componentInParent.Dying)
			{
				triggerSlowdown();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (AlwaysRespawn)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].enabled = true;
			}
			triggered = false;
			if (NetSurrogate.BoolVal)
			{
				NetSurrogate.BoolVal = false;
			}
			Timekeeper.RemoveSlowMoSource(this);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Timekeeper.RemoveSlowMoSource(this);
	}
}
