using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackhole : ActiveBlock
{
	public float PullStrength;

	public AnimationCurve gravityCurve;

	public float shrinkTime;

	public float deathDistance;

	public float rotateSpinSpeed;

	public Vector3 KillOffset;

	public float cutoffDistanceSqr;

	public string DeathCauseName = "Blackhole";

	public GameObject blackholeBurstPrefab;

	public blackholeBurst currentBlackholeBurst;

	public bool blackholeBurstFilled;

	public float blackholeBurstTimer;

	public float blackholeBurstTime;

	protected List<Character> currentlySucking = new List<Character>();

	protected Animator animator;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
	}

	public override void Enable()
	{
		base.Enable();
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingOrder = 100;
		}
		currentlySucking.Clear();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (disabled || !base.Active)
		{
			return;
		}
		blackholeBurstTimer += Time.deltaTime;
		foreach (GamePlayer item in LobbyManager.instance.CurrentGameController.CurrentPlayerQueue)
		{
			if (!(item != null))
			{
				continue;
			}
			Character characterInstance = item.CharacterInstance;
			if (characterInstance == null || characterInstance.InBlackHole)
			{
				if (currentlySucking.Contains(characterInstance))
				{
					AkSoundEngine.PostEvent("SFX_Pieces_Black_Hole_UnSuck", base.gameObject);
					currentlySucking.Remove(characterInstance);
				}
				continue;
			}
			Vector3 vector = characterInstance.transform.position - base.transform.position;
			if (vector.sqrMagnitude < cutoffDistanceSqr)
			{
				float num = GameSettings.GetInstance().blackholePull * GameSettings.GetInstance().blackholeRange.Evaluate(vector.magnitude);
				if (blackholeBurstTimer > blackholeBurstTime && !characterInstance.Dying)
				{
					currentBlackholeBurst = Object.Instantiate(blackholeBurstPrefab, base.transform.position, base.transform.rotation).GetComponent<blackholeBurst>();
					currentBlackholeBurst.target = characterInstance;
					currentBlackholeBurst.parentBlackhole = this;
					currentBlackholeBurst.stopTracking = cutoffDistanceSqr;
					blackholeBurstFilled = true;
					AkSoundEngine.PostEvent("SFX_Pieces_Black_Hole_Suck", base.gameObject);
					if (!currentlySucking.Contains(characterInstance))
					{
						currentlySucking.Add(characterInstance);
					}
				}
				if ((double)Mathf.Abs(num) > 0.1)
				{
					characterInstance.ApplyPhysicsModifier(new PhysicsModifier(PhysicsModifier.ModType.Blackhole, num, vector.normalized, base.gameObject));
					Debug.DrawLine(characterInstance.transform.position, characterInstance.transform.position + vector.normalized * num);
				}
				if (!characterInstance.InBlackHole && (vector + KillOffset).magnitude < deathDistance)
				{
					if (characterInstance.hasAuthority && !characterInstance.Dead && !characterInstance.Dying)
					{
						characterInstance.KillCharacter(DeathCauseName, deathFreezeOn: false, placedByPlayerNumber);
					}
					StartCoroutine(suckInCharacter(characterInstance));
				}
			}
			else if (currentlySucking.Contains(characterInstance))
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Black_Hole_UnSuck", base.gameObject);
				currentlySucking.Remove(characterInstance);
			}
		}
		if (blackholeBurstTimer > blackholeBurstTime)
		{
			blackholeBurstTimer = 0f;
		}
	}

	public IEnumerator suckInCharacter(Character chr)
	{
		if (!chr.InBlackHole && !chr.Success)
		{
			chr.InBlackHole = true;
			AkSoundEngine.PostEvent("SFX_Pieces_Black_Hole", base.gameObject);
			float shrinkTimer = 0.01f;
			Vector3 initialScale = chr.transform.localScale;
			Vector3 targetScale = new Vector3(0.001f, 0.001f, 0.001f);
			GameObject tempTrackingObject = new GameObject("Character Suck In Target");
			tempTrackingObject.transform.SetParent(base.transform);
			tempTrackingObject.transform.position = chr.transform.position;
			tempTrackingObject.transform.rotation = chr.transform.rotation;
			Vector3 initialPosition = tempTrackingObject.transform.localPosition;
			Quaternion initialRotation = chr.transform.rotation;
			do
			{
				shrinkTimer += Time.deltaTime;
				Vector3 localScale = Vector3.Lerp(initialScale, targetScale, shrinkTimer / shrinkTime);
				chr.transform.localScale = localScale;
				tempTrackingObject.transform.localPosition = Vector3.Lerp(initialPosition, Vector3.zero, shrinkTimer / shrinkTime);
				chr.transform.position = tempTrackingObject.transform.position;
				chr.transform.Rotate(new Vector3(0f, 0f, rotateSpinSpeed * Time.deltaTime));
				yield return null;
			}
			while (shrinkTimer < shrinkTime && chr != null && chr.Dying && base.Active);
			Object.Destroy(tempTrackingObject);
			while (base.Active && chr != null && chr.Dying && chr.InBlackHole)
			{
				chr.transform.position = base.transform.position;
				yield return null;
			}
			if (chr != null)
			{
				chr.RefreshScale();
				chr.transform.rotation = initialRotation;
				chr.InBlackHole = false;
				currentlySucking.Remove(chr);
			}
		}
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
}
