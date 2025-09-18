using GameEvent;
using UnityEngine;

public class Bomb : Placeable
{
	public BombKillzone Killzone;

	public bool bombArmed = true;

	protected bool bombExploded;

	public SpriteRenderer BombSprite;

	public Animator BombAnimator;

	public string ExplodeSoundString;

	public string FuseSoundString;

	protected override void Awake()
	{
		base.Awake();
		bombArmed = false;
	}

	protected void Update()
	{
		AnimatorStateInfo currentAnimatorStateInfo = BombAnimator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("Explode") && !bombExploded)
		{
			onExplode();
		}
		if (!disabled && currentAnimatorStateInfo.IsName("Done"))
		{
			placed = true;
		}
	}

	protected virtual void onExplode()
	{
		bombExploded = true;
		GameObject[] array = Killzone.InBlastZone.ToArray();
		foreach (GameObject gameObject in array)
		{
			if (gameObject == null)
			{
				continue;
			}
			Placeable componentInParent = gameObject.GetComponentInParent<Placeable>();
			if ((bool)componentInParent && !componentInParent.inDestructible && componentInParent.InteractableInCurrentMode)
			{
				if (componentInParent.damageable)
				{
					componentInParent.AddBombDamage(999999);
					continue;
				}
				GameEventManager.SendEvent(new DestroyPieceEvent(componentInParent, placedByPlayerNumber));
				Killzone.InBlastZone.Remove(gameObject);
				float num = Mathf.Lerp(0f, 0.6f, (componentInParent.transform.position - base.transform.position).magnitude / GameSettings.GetInstance().bombExplodeDistanceDelay);
				componentInParent.DestroySelfIn(destroyChildren: false, useSmoke: true, num + Random.Range(0f, 0.2f));
			}
		}
	}

	public override void Disable()
	{
		base.Disable();
		BombAnimator.SetBool("Placed", value: false);
		Killzone.Reset();
		Killzone.GetComponent<Collider2D>().enabled = false;
	}

	public override void Enable()
	{
		base.Enable();
		Killzone.GetComponent<Collider2D>().enabled = true;
		bombArmed = true;
		bombExploded = false;
	}

	public override void EnablePlacement(bool showGuides = true)
	{
		base.EnablePlacement(showGuides);
		Killzone.GetComponent<Collider2D>().enabled = true;
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (!(item == null))
			{
				item.Disable();
			}
		}
		bombArmed = true;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		Place(playerNumber);
	}

	public override void Place(int playerNumber)
	{
		placedByPlayerNumber = playerNumber;
		placed = false;
		GameObject[] array = Killzone.InBlastZone.ToArray();
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				Placeable componentInParent = gameObject.GetComponentInParent<Placeable>();
				if ((bool)componentInParent && !componentInParent.inDestructible)
				{
					componentInParent.Protected = true;
				}
			}
		}
		if (bombArmed)
		{
			BombAnimator.SetBool("Placed", value: true);
			AkSoundEngine.PostEvent(FuseSoundString, base.gameObject);
		}
	}

	private void FinishExplode()
	{
		BombSprite.enabled = false;
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		placed = true;
		bombArmed = false;
		DestroySelf();
		GameEventManager.SendEvent(new PiecePlacedEvent(placedByPlayerNumber, this));
	}

	private void ExplodeSound()
	{
		LobbyManager.instance.GetCurrentZoomCamera().shakeCamera();
		AkSoundEngine.PostEvent(ExplodeSoundString, base.gameObject);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Killzone.Reset();
	}
}
