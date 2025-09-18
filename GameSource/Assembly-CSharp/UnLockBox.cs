using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class UnLockBox : ActiveBlock
{
	protected bool popped;

	protected bool landed;

	protected Animator animator;

	protected Rigidbody2D rb;

	public float gravitySpeed = 1f;

	protected ZoomCamera zoomCamera;

	public float centerInCameraTime = 2f;

	public float RemoveFromSceneTime = 10f;

	protected bool moving;

	public AnimationCurve x;

	public AnimationCurve y;

	public float belowCenterY;

	public float sizeScaleMod = 1f;

	public UnLockInfo currentUnLockInfo;

	protected Vector2 PrePauseVelocity;

	public Collider2D Box;

	public Text upperText;

	public Text upperTextShadow;

	public Text lowerText;

	public Text lowerTextShadow;

	public Text SteamNameText;

	public Text SteamNameTextShadow;

	public Image unlockImage;

	protected float poppedTimer = 7f;

	public static TagComparer.Tag poppingObjectMask = (TagComparer.Tag)288;

	public static TagComparer.Tag stoppingObjectMask = (TagComparer.Tag)384;

	public void SetupUnlockTextAndImage(UnLockInfo unlockSign)
	{
		currentUnLockInfo = unlockSign;
		upperText.text = unlockSign.UpperString;
		upperTextShadow.text = unlockSign.UpperString;
		lowerText.text = unlockSign.LowerString;
		lowerTextShadow.text = unlockSign.LowerString;
		SteamNameText.text = unlockSign.SteamNameString;
		SteamNameTextShadow.text = unlockSign.SteamNameString;
		unlockImage.sprite = unlockSign.itemSprite;
	}

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
		rb = GetComponent<Rigidbody2D>();
		zoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		placed = true;
		SetupUnlockTextAndImage(currentUnLockInfo);
	}

	protected void Update()
	{
		if (popped && !landed)
		{
			poppedTimer -= Time.deltaTime;
			if (poppedTimer < 0f)
			{
				moving = true;
				landed = true;
				GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.SCOREBOARDDELAY));
				StartCoroutine(ToCenterScreen());
			}
		}
	}

	private bool IsDeathPit(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Deathpit);
		}
		return false;
	}

	private bool CanPopBox(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(poppingObjectMask);
		}
		return false;
	}

	private bool CanStopBox(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(stoppingObjectMask);
		}
		return false;
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (!(c.GetComponent<FallingIce>() != null))
		{
			CollisionTag component = c.GetComponent<CollisionTag>();
			if (popped && !landed && !moving && IsDeathPit(c.gameObject, component))
			{
				moving = true;
				GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.SCOREBOARDDELAY));
				StartCoroutine(ToCenterScreen());
			}
			if (!popped && base.Active && CanPopBox(c.gameObject, component))
			{
				Pop();
			}
		}
	}

	public override void Tint()
	{
	}

	public void OnCollisionEnter2D(Collision2D c)
	{
		if (popped && !landed)
		{
			CollisionTag component = c.gameObject.GetComponent<CollisionTag>();
			if (CanStopBox(c.gameObject, component))
			{
				landed = true;
				AkSoundEngine.PostEvent("SFX_UnlockBox_HitGround", base.gameObject);
				animator.SetTrigger("Open");
				rb.isKinematic = true;
				rb.gravityScale = 0f;
				GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.SCOREBOARDDELAY));
			}
			if (!moving && IsDeathPit(c.gameObject, component))
			{
				moving = true;
				GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.SCOREBOARDDELAY));
				StartCoroutine(ToCenterScreen());
			}
		}
	}

	private IEnumerator ToCenterScreen()
	{
		rb.isKinematic = true;
		rb.gravityScale = 0f;
		Vector3 startPosition = base.transform.position;
		float timer = 0f;
		while (timer < centerInCameraTime)
		{
			base.transform.localScale = Vector3.one * zoomCamera.useCamera.fieldOfView * sizeScaleMod;
			float num = Mathf.Lerp(startPosition.x, zoomCamera.transform.position.x, x.Evaluate(timer / centerInCameraTime));
			float num2 = Mathf.Lerp(startPosition.y, zoomCamera.transform.position.y + belowCenterY * base.transform.localScale.x, y.Evaluate(timer / centerInCameraTime));
			base.transform.position = new Vector3(num, num2, base.transform.position.z);
			if (!paused || scoreboard)
			{
				timer += Time.unscaledDeltaTime;
			}
			yield return null;
		}
		base.transform.parent = zoomCamera.transform;
		animator.SetTrigger("OpenCenter");
		while (timer < RemoveFromSceneTime)
		{
			base.transform.localScale = Vector3.one * zoomCamera.useCamera.fieldOfView * sizeScaleMod;
			base.transform.position = new Vector3(zoomCamera.transform.position.x, zoomCamera.transform.position.y + belowCenterY * base.transform.localScale.x, startPosition.z);
			if (!paused)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
		base.transform.parent = null;
		if ((bool)zoomCamera)
		{
			zoomCamera.RemoveTarget(base.transform);
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (!scoreboard)
		{
			animator.speed = 0f;
		}
		if (popped && !moving)
		{
			PrePauseVelocity = rb.velocity;
			rb.isKinematic = true;
			rb.velocity = Vector2.zero;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
		if (popped && !moving)
		{
			rb.velocity = PrePauseVelocity;
			rb.isKinematic = false;
		}
	}

	protected override void ToPlaceMode(bool enableSelection)
	{
		base.ToPlaceMode(enableSelection);
		if (popped)
		{
			if ((bool)zoomCamera)
			{
				zoomCamera.RemoveTarget(base.transform);
			}
			Object.Destroy(base.gameObject);
		}
	}

	public override void ToPlayMode()
	{
		base.ToPlayMode();
	}

	public void removeCameraTarget()
	{
		if ((bool)zoomCamera)
		{
			zoomCamera.RemoveTarget(base.transform);
		}
		Object.Destroy(base.gameObject, 1f);
	}

	public void Pop()
	{
		animator.SetTrigger("Pop");
		rb.isKinematic = false;
		rb.gravityScale = gravitySpeed;
		popped = true;
		foreach (CheckColliding item in ActiveCollidersNew)
		{
			item.Disable();
		}
		foreach (CheckColliding item2 in PlacementCollidersNew)
		{
			item2.Disable();
		}
		Box.enabled = true;
		if ((bool)zoomCamera)
		{
			zoomCamera.AddTarget(base.transform);
		}
		if (!currentUnLockInfo.IsLocal)
		{
			return;
		}
		SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(currentUnLockInfo.forPlayerLocalNumber);
		if (saveFileDataForLocalPlayer != null)
		{
			switch (currentUnLockInfo.unlockType)
			{
			case UnLockInfo.UnlockType.Character:
				saveFileDataForLocalPlayer.GetStat<StatBoolArray>("CharactersUnlocked").Set((int)currentUnLockInfo.AssociatedCharacter, value: true);
				saveFileDataForLocalPlayer.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked").Set(0);
				AchievementChecker.Instance.Character_Unlocked_AchievementCheck(saveFileDataForLocalPlayer);
				break;
			case UnLockInfo.UnlockType.Outfit:
				saveFileDataForLocalPlayer.GetStat<StatCountArray>("OutfitsUnlocked").OrValue((int)currentUnLockInfo.AssociatedCharacter, currentUnLockInfo.OutfitMaskNumber);
				AchievementChecker.Instance.Outfits_Unlocked_AchievementCheck(saveFileDataForLocalPlayer);
				break;
			case UnLockInfo.UnlockType.Level:
				saveFileDataForLocalPlayer.GetStat<StatBoolArray>("LevelsUnlocked").Set((int)currentUnLockInfo.AssociatedLevel, value: true);
				AchievementChecker.Instance.Levels_Unlocked_AchievementCheck(saveFileDataForLocalPlayer);
				saveFileDataForLocalPlayer.GetStat<StatCount>("GamesSinceLastLevelUnlocked").Set(-1);
				break;
			}
			Player player = PlayerManager.GetInstance().GetPlayer(currentUnLockInfo.forPlayerLocalNumber);
			if (player != null && player.AssociatedLobbyPlayer != null)
			{
				GameState.GetInstance().guaranteedUnlocks[player.AssociatedLobbyPlayer] = false;
			}
			currentUnLockInfo.SendUnlockedAnalytic();
			StatTracker.Instance.SaveGameForAllUsers();
		}
	}
}
