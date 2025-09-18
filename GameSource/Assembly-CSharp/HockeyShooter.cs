using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HockeyShooter : ActiveBlock
{
	[Header("Hockey Shooter")]
	public puckStreak puckStreak;

	public puckSprite puckSprite;

	public float initialDelay;

	public float interval;

	private float modInterval;

	public float Angle;

	public float lastShot;

	public string SoundEventName;

	private Animator anim;

	public bool shooting;

	public Transform RayStart;

	public Transform RayEnd;

	public Vector3 RayStartDebug;

	public Vector3 RayEndDebug;

	public float pushCharacterStrength;

	protected bool addImpulseSwitch;

	protected Vector2 impulseToAdd;

	protected Character impulseTarget;

	public float VerticalPushExtra;

	public LayerMask puckInteractsLayers;

	public Vector2 boxcastSize = new Vector2(0.25f, 0.25f);

	private Teleporter lastTraversedTeleporter;

	private AnimalCannon lastShotCannon;

	private HashSet<AnimalCannon> visitedCannons = new HashSet<AnimalCannon>();

	public GameObject impactProjectileTrigger;

	private IEnumerator impactProjectileTriggerAnimator;

	public string puckSpriteSortingLayerName;

	public int puckSpriteSortingOrder;

	private BoxCollider2D screenBoundsCollider;

	private float wrapMinX;

	private float wrapMaxX;

	private float wrapMinY;

	private float wrapMaxY;

	private float wrapWidth;

	private float wrapHeight;

	private Vector3 initialLaunchPos;

	private new RaycastHit2D[] raycastResultCache = new RaycastHit2D[10];

	private static int puckSpriteSortingLayerID = -1;

	private int PuckSpriteSortingLayerID
	{
		get
		{
			if (puckSpriteSortingLayerID == -1)
			{
				puckSpriteSortingLayerID = SortingLayer.NameToID(puckSpriteSortingLayerName);
			}
			return puckSpriteSortingLayerID;
		}
	}

	protected override void Start()
	{
		base.Start();
		anim = GetComponentInChildren<Animator>();
		UpdateInitialDelay();
		lastShot = 0f - initialDelay;
		ScreenWrapping screenWrapping = Object.FindObjectOfType<ScreenWrapping>();
		if (screenWrapping != null)
		{
			screenBoundsCollider = screenWrapping.GetComponent<BoxCollider2D>();
			if (screenBoundsCollider != null)
			{
				Bounds bounds = screenBoundsCollider.bounds;
				wrapMinX = bounds.min.x;
				wrapMaxX = bounds.max.x;
				wrapMinY = bounds.min.y;
				wrapMaxY = bounds.max.y;
				wrapWidth = bounds.size.x;
				wrapHeight = bounds.size.y;
			}
		}
	}

	private void UpdateInitialDelay()
	{
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
		{
			initialDelay = 0.5f;
		}
		else
		{
			initialDelay = 0.5f - LobbyManager.instance.GetAveragePingToServer();
		}
	}

	protected void Update()
	{
		UpdateInitialDelay();
		if (!paused && base.Active)
		{
			lastShot += Time.deltaTime;
			if (anim != null && lastShot >= modInterval && !shooting)
			{
				anim.SetTrigger("Shoot");
				shooting = true;
				lastShot = 0f;
			}
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (addImpulseSwitch && !impulseTarget.DeathFrozen)
		{
			addImpulseSwitch = false;
			impulseTarget.AddImpulse(impulseToAdd, 0.1f);
		}
		if (impactProjectileTriggerAnimator != null && !impactProjectileTriggerAnimator.MoveNext())
		{
			impactProjectileTriggerAnimator = null;
		}
	}

	public override void Enable()
	{
		base.Enable();
		lastShot = 0f - initialDelay;
	}

	public override void Disable()
	{
		base.Disable();
		shooting = false;
	}

	protected override void Activate()
	{
		base.Activate();
		float rateOfFire = Modifiers.GetInstance().RateOfFire;
		modInterval = interval / rateOfFire;
		if (anim != null && rateOfFire > 1f)
		{
			anim.speed = rateOfFire;
		}
	}

	public override void Reset()
	{
		base.Reset();
		lastShot = 0f - initialDelay;
		if (shooting)
		{
			shooting = false;
			anim.SetTrigger("Reset");
			anim.ResetTrigger("Shoot");
		}
		HideProjectileTrigger();
		impactProjectileTriggerAnimator = null;
	}

	public void ShootProjectile()
	{
		shooting = false;
		if (!disabled && isActive)
		{
			lastTraversedTeleporter = null;
			lastShotCannon = null;
			visitedCannons.Clear();
			initialLaunchPos = RayStart.position;
			ThrowPuck(RayStart.position, RayEnd.position - RayStart.position);
		}
	}

	private void ThrowPuck(Vector3 rayStart, Vector3 rayDir, int recursions = 0)
	{
		if (recursions > 100)
		{
			Debug.LogWarning("Puck went through too many teleporters and disintegrated into thin air!");
			return;
		}
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = false;
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(rayStart, boxcastSize, 0f, rayDir, 1000f, puckInteractsLayers);
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		Vector3 rayVectorToHit = ((raycastHit2D.collider != null) ? ((Vector3)raycastHit2D.point - rayStart) : (rayDir.normalized * 1000f));
		if (recursions > 0)
		{
			float num = Vector3.Dot(initialLaunchPos - rayStart, rayDir.normalized);
			if (num > 0f && num < rayVectorToHit.magnitude)
			{
				Vector3 vector = rayStart + rayDir.normalized * num;
				if (Vector3.SqrMagnitude(vector - initialLaunchPos) < 0.25f)
				{
					MakeStreak(rayStart, vector, Vector3.zero, showPuck: true);
					AkSoundEngine.PostEvent("SFX_Pieces_Hockey_Puck_Shooter_Hit", base.gameObject);
					return;
				}
			}
		}
		if (doRedirect(rayStart, rayVectorToHit, rayDir.normalized, recursions))
		{
			return;
		}
		if (raycastHit2D.collider != null)
		{
			GameObject gameObject = MakeStreak(rayStart, raycastHit2D.point, raycastHit2D.normal, showPuck: true);
			ShowProjectileTrigger(raycastHit2D.point);
			Character componentInParent = raycastHit2D.collider.gameObject.GetComponentInParent<Character>();
			if (componentInParent != null && componentInParent.hasAuthority)
			{
				componentInParent.KillCharacter(Name, deathFreezeOn: true, placedByPlayerNumber);
				Vector2 vector2 = rayDir.normalized;
				impulseToAdd = vector2 * pushCharacterStrength + Vector2.up * VerticalPushExtra;
				addImpulseSwitch = true;
				impulseTarget = componentInParent;
			}
			if (componentInParent == null && gameObject != null)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Hockey_Puck_Shooter_Hit", gameObject);
			}
			UnLockBox componentInParent2 = raycastHit2D.collider.gameObject.GetComponentInParent<UnLockBox>();
			if (componentInParent2 != null)
			{
				componentInParent2.Pop();
			}
		}
		else
		{
			MakeStreak(rayStart, rayStart + rayDir.normalized * 100f, Vector3.zero, showPuck: false);
		}
	}

	private bool doRedirect(Vector3 rayStart, Vector3 rayVectorToHit, Vector3 normalizedDir, int recursions)
	{
		float sqrMagnitude = rayVectorToHit.sqrMagnitude;
		Vector3 vector = Vector3.zero;
		bool flag = false;
		float num = float.MaxValue;
		if (screenBoundsCollider != null)
		{
			float num2 = ((normalizedDir.x < 0f) ? ((wrapMinX - rayStart.x) / normalizedDir.x) : float.MaxValue);
			float num3 = ((normalizedDir.x > 0f) ? ((wrapMaxX - rayStart.x) / normalizedDir.x) : float.MaxValue);
			float num4 = ((normalizedDir.y < 0f) ? ((wrapMinY - rayStart.y) / normalizedDir.y) : float.MaxValue);
			float num5 = ((normalizedDir.y > 0f) ? ((wrapMaxY - rayStart.y) / normalizedDir.y) : float.MaxValue);
			float num6 = float.MaxValue;
			if (num2 > 0f && num2 < num6)
			{
				num6 = num2;
			}
			if (num3 > 0f && num3 < num6)
			{
				num6 = num3;
			}
			if (num4 > 0f && num4 < num6)
			{
				num6 = num4;
			}
			if (num5 > 0f && num5 < num6)
			{
				num6 = num5;
			}
			if (num6 != float.MaxValue && num6 * num6 < sqrMagnitude - 0.01f)
			{
				num = num6 * num6;
				flag = true;
				vector = rayStart + normalizedDir * num6;
			}
		}
		Vector3 hitPoint;
		Teleporter teleporter = FindTeleporter(rayStart, rayVectorToHit, out hitPoint);
		Vector3 hitPoint2;
		AnimalCannon animalCannon = FindAnimalCannon(rayStart, rayVectorToHit, out hitPoint2);
		float num7 = ((teleporter != null) ? (hitPoint - rayStart).sqrMagnitude : float.MaxValue);
		float num8 = ((animalCannon != null) ? (hitPoint2 - rayStart).sqrMagnitude : float.MaxValue);
		if (animalCannon != null && visitedCannons.Contains(animalCannon))
		{
			if (num8 < num && num8 < num7)
			{
				MakeStreak(rayStart, hitPoint2, Vector3.zero, showPuck: true);
				ShowProjectileTrigger(hitPoint2);
				AkSoundEngine.PostEvent("SFX_Pieces_Hockey_Puck_Shooter_Hit", base.gameObject);
				return true;
			}
			animalCannon = null;
			num8 = float.MaxValue;
		}
		if (!flag && teleporter == null && animalCannon == null)
		{
			return false;
		}
		if (flag && num < num7 && num < num8)
		{
			MakeStreak(rayStart, vector, Vector3.zero, showPuck: false);
			lastTraversedTeleporter = null;
			lastShotCannon = null;
			Vector3 rayStart2 = vector;
			if (rayStart2.x <= wrapMinX + 0.1f)
			{
				rayStart2.x += wrapWidth;
			}
			else if (rayStart2.x >= wrapMaxX - 0.1f)
			{
				rayStart2.x -= wrapWidth;
			}
			if (rayStart2.y <= wrapMinY + 0.1f)
			{
				rayStart2.y += wrapHeight;
			}
			else if (rayStart2.y >= wrapMaxY - 0.1f)
			{
				rayStart2.y -= wrapHeight;
			}
			ThrowPuck(rayStart2, normalizedDir, recursions + 1);
			return true;
		}
		if (!(teleporter == null) && (animalCannon == null || num7 < num8))
		{
			MakeStreak(rayStart, hitPoint, Vector3.zero, showPuck: false);
			TeleportPuck(normalizedDir, hitPoint, teleporter, recursions);
			return true;
		}
		MakeStreak(rayStart, hitPoint2, Vector3.zero, showPuck: false);
		ShootPuck(normalizedDir, hitPoint2, animalCannon, recursions);
		return true;
	}

	private void TeleportPuck(Vector3 rayDir, Vector3 teleporterHitPoint, Teleporter foundTeleporter, int recursions)
	{
		foundTeleporter.SpawnProjectileSendParticle(teleporterHitPoint);
		lastTraversedTeleporter = foundTeleporter.Destination;
		lastShotCannon = null;
		Vector3 position = foundTeleporter.transform.InverseTransformPoint(teleporterHitPoint);
		Vector3 vector = foundTeleporter.Destination.transform.TransformPoint(position);
		Vector3 vector2 = Quaternion.Inverse(foundTeleporter.transform.rotation) * rayDir;
		Vector3 rayDir2 = foundTeleporter.Destination.transform.rotation * vector2;
		foundTeleporter.Destination.SpawnProjectileSendParticle(vector);
		ThrowPuck(vector, rayDir2, recursions + 1);
	}

	private void ShootPuck(Vector3 rayDir, Vector3 cannonHitPoint, AnimalCannon foundCannon, int recursions)
	{
		lastShotCannon = foundCannon;
		lastTraversedTeleporter = null;
		visitedCannons.Add(foundCannon);
		float num = 0f;
		Vector3 localScale = foundCannon.transform.localScale;
		if (localScale.y < 0f)
		{
			num = ((!(localScale.x < 0f)) ? 270f : 180f);
		}
		else if (localScale.x < 0f)
		{
			num = 90f;
		}
		Vector3 rayDir2 = Quaternion.AngleAxis(foundCannon.transform.eulerAngles.z + num + foundCannon.boostAngle, Vector3.forward) * Vector3.right;
		ThrowPuck(foundCannon.LaunchTarget.position, rayDir2, recursions + 1);
	}

	private Teleporter FindTeleporter(Vector3 p0, Vector2 dir, out Vector3 hitPoint)
	{
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		int num = Physics2D.BoxCastNonAlloc(p0, boxcastSize, 0f, dir, raycastResultCache, dir.magnitude + 0.01f, puckInteractsLayers);
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		for (int i = 0; i < num; i++)
		{
			RaycastHit2D raycastHit2D = raycastResultCache[i];
			Teleporter componentInParent = raycastHit2D.collider.GetComponentInParent<Teleporter>();
			if (componentInParent != null && componentInParent != lastTraversedTeleporter && componentInParent.CanTeleport)
			{
				hitPoint = raycastHit2D.point;
				return componentInParent;
			}
		}
		hitPoint = Vector3.zero;
		return null;
	}

	private AnimalCannon FindAnimalCannon(Vector3 p0, Vector2 dir, out Vector3 hitPoint)
	{
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		int num = Physics2D.BoxCastNonAlloc(p0, boxcastSize, 0f, dir, raycastResultCache, dir.magnitude + 0.01f, puckInteractsLayers);
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		for (int i = 0; i < num; i++)
		{
			RaycastHit2D raycastHit2D = raycastResultCache[i];
			AnimalCannon componentInParent = raycastHit2D.collider.GetComponentInParent<AnimalCannon>();
			if (componentInParent != null && componentInParent != lastShotCannon && (bool)componentInParent)
			{
				hitPoint = raycastHit2D.point;
				return componentInParent;
			}
		}
		hitPoint = Vector3.zero;
		return null;
	}

	private GameObject MakeStreak(Vector3 streakStartPosition, Vector3 puckHitPosition, Vector3 puckHitNormal, bool showPuck)
	{
		puckStreak puckStreak2 = Object.Instantiate(puckStreak, Vector3.zero, base.transform.rotation);
		if (!showPuck)
		{
			puckStreak2.startPuck(streakStartPosition, puckHitPosition, PuckSpriteSortingLayerID, puckSpriteSortingOrder - 1);
			return null;
		}
		puckSprite puckSprite2 = Object.Instantiate(puckSprite, puckHitPosition, base.transform.rotation);
		puckSprite2.normalOfCollision = puckHitNormal;
		puckSprite2.SetSortingLayer(PuckSpriteSortingLayerID, puckSpriteSortingOrder);
		puckStreak2.puckSprite = puckSprite2;
		puckStreak2.startPuck(streakStartPosition, puckHitPosition, PuckSpriteSortingLayerID, puckSpriteSortingOrder - 1);
		return puckSprite2.gameObject;
	}

	public void warningSound()
	{
		AkSoundEngine.PostEvent("SFX_Pieces_Hockey_Puck_Shooter_Warning", base.gameObject);
	}

	private void shootingSound()
	{
		AkSoundEngine.PostEvent("SFX_Pieces_Hockey_Puck_Shooter", base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		if (anim != null)
		{
			anim.speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (!paused && !scoreboard && anim != null)
		{
			float rateOfFire = Modifiers.GetInstance().RateOfFire;
			anim.speed = rateOfFire;
		}
	}

	private void ShowProjectileTrigger(Vector3 impactPosition)
	{
		impactProjectileTrigger.gameObject.SetActive(value: true);
		impactProjectileTrigger.transform.position = impactPosition;
		impactProjectileTriggerAnimator = HideProjectileTriggerAfter(3);
	}

	private void HideProjectileTrigger()
	{
		impactProjectileTrigger.transform.localPosition = Vector3.zero;
		impactProjectileTrigger.gameObject.SetActive(value: false);
	}

	private IEnumerator HideProjectileTriggerAfter(int frames)
	{
		int i = 0;
		while (i < frames)
		{
			yield return null;
			int num = i + 1;
			i = num;
		}
		HideProjectileTrigger();
	}
}
