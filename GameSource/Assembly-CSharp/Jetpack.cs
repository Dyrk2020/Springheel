using UnityEngine;
using UnityEngine.Networking;

public class Jetpack : ActiveBlock
{
	protected Character carrier;

	protected Vector3 startPosition;

	protected Transform parentPiece;

	protected bool pickupRequested;

	protected Character pickupRequester;

	protected Animator animator;

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

	public Character Carrier => carrier;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
	}

	protected void Update()
	{
		if (!disabled && base.Active)
		{
			if (carrier == null && pickupRequested)
			{
				_ = pickupRequester;
			}
			if (carrier != null && (carrier.Dead || carrier.Dying))
			{
				carrier = null;
			}
		}
	}

	protected override void Activate()
	{
		base.Activate();
		if (carrier != null && carrier.hasAuthority)
		{
			carrier.SetJetpackPickedUp(pickedUp: false);
		}
		carrier = null;
		pickupRequested = false;
		pickupRequester = null;
	}

	public override void Disable()
	{
		base.Disable();
		carrier = null;
		pickupRequested = false;
		pickupRequester = null;
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (carrier == null && base.Active && !pickupRequested)
		{
			Character componentInParent = c.gameObject.GetComponentInParent<Character>();
			if (!(componentInParent == null) && !componentInParent.Dead && !componentInParent.Dying && !componentInParent.HasJetpack && (!componentInParent.CrouchingDown || (!(c == componentInParent.headCollider) && !componentInParent.hazardHeadcollider && !(c == componentInParent.coinGrabber))) && componentInParent.hasAuthority && NetSurrogate != null)
			{
				componentInParent.TryPickUpJetpack(NetSurrogate.netId);
				pickupRequested = true;
				pickupRequester = componentInParent;
			}
		}
	}

	protected virtual void Pickup(Character chr)
	{
		if (carrier == null)
		{
			carrier = chr;
			pickupRequested = false;
			pickupRequester = null;
			AkSoundEngine.PostEvent("SFX_Char_Jetpack_PickUp", base.gameObject);
		}
		else
		{
			Debug.LogError("Attempted to pick up jetpack with a carrier.");
		}
		if (!AlwaysRespawn || !IsSaveable)
		{
			base.MarkedForDestruction = true;
		}
		SwitchColliderTo(ColliderModeEnum.NoColliders);
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = false;
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
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
	}

	public static Jetpack GetJetpackFromSurrogateID(NetworkInstanceId id)
	{
		GameObject gameObject = ClientScene.FindLocalObject(id);
		if (gameObject != null)
		{
			Transform parent = gameObject.transform.parent;
			if (parent != null)
			{
				Jetpack component = parent.GetComponent<Jetpack>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	public void SetCarrier(Character chr)
	{
		chr.SetJetpackPickedUp(pickedUp: true);
		Pickup(chr);
	}
}
