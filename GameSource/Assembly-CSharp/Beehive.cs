using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Beehive : ActiveBlock
{
	private enum BeeState
	{
		INACTIVE,
		SWARM,
		POSTSWARM,
		DISAPPEAR,
		WALKONHIVE
	}

	public Character followedCharacter;

	public BeeSwarm bees;

	public Collider2D beeHazard;

	public Transform indicatorBees;

	public Animator animator;

	public float birthTime = 1f;

	public float beeSpeed;

	public float returnBeeSpeed = 2f;

	private float attackDeadBodyTimer;

	public float attackDeadBodyTime = 2f;

	private IEnumerator beeDeathAnim;

	private IEnumerator beeBirthAnim;

	private bool beesRequested;

	private bool beesMoving;

	private bool beesUnleashed;

	public float followBeeAccel = 1f;

	public AnimationCurve followBeeAccelerationDistanceModifier = new AnimationCurve();

	public AnimationCurve followBeeDistanceToClumpRandomNoiseMod = new AnimationCurve();

	public AnimationCurve followBeeCurve = new AnimationCurve();

	public float velocityDampeningFriction = 0.97f;

	public Transform followBees;

	private Transform[] beeTransforms;

	private Vector3[] lastBeeForce;

	private Vector3[] followBeesVelocity;

	private float[] randomHeading;

	private float[] randomMagnitude;

	private BeeState[] beeState;

	private Vector3[] beeHistory;

	private int beeHistoryMaxSize = 100;

	private int beeHistoryCurrentIndex;

	public float noiseTemporalSpeedHeading = 0.5f;

	public float noiseTemporalSpeedMagnitude = 3f;

	public Transform beeSpawnPoint;

	public float randomOffsetFluctuationRate = 1f;

	public float randomHeadingSpeed;

	public AnimationCurve headingSpeedThroughSwarm;

	public float targetModifierSpeed = 0.5f;

	public float postSwarmRandomHeadingSpeed = 1f;

	public float postSwarmRandomFriction = 0.6f;

	public AnimationCurve disappearBeeAccelerationIndexModifier = new AnimationCurve();

	public AnimationCurve disappearBeeDistanceToClumpRandomNoiseMod = new AnimationCurve();

	public AnimationCurve disappearBeeSpringSpeed = new AnimationCurve();

	public float returnRandomHeadingSpeed;

	public float returnTargetModifierSpeed;

	public int indicatorBeeNum = 3;

	public Transform beeWalkPoint;

	public float beeWalkSpeed = 0.5f;

	public float beeWalkRandomSpeed = 0.5f;

	public float beeWalkFriction = 0.7f;

	public float beeWalktemporalNoiseSpeed = 1f;

	public float beeWalkMinimumRandomMagnitude = 0.2f;

	public AnimationCurve walkingBeeAccelerationDistanceModifier = new AnimationCurve();

	public int randomRateOfReturnToHive = 50;

	public int randomRateOfPostSwarm = 300;

	private Vector3 targetPosition;

	protected override void Start()
	{
		base.Start();
		List<SpriteRenderer> list = new List<SpriteRenderer>();
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer item in artSprites)
		{
			list.Add(item);
		}
		bees.gameObject.SetActive(value: false);
		indicatorBees.gameObject.SetActive(value: false);
		list.AddRange(indicatorBees.GetComponentsInChildren<SpriteRenderer>());
		ArtSprites = list.ToArray();
		beeTransforms = new Transform[followBees.childCount];
		followBeesVelocity = new Vector3[followBees.childCount];
		randomHeading = new float[followBees.childCount];
		randomMagnitude = new float[followBees.childCount];
		lastBeeForce = new Vector3[followBees.childCount];
		beeState = new BeeState[followBees.childCount];
		for (int j = 0; j < followBees.childCount; j++)
		{
			followBeesVelocity[j] = new Vector3(UnityEngine.Random.Range(0f, 0.1f), UnityEngine.Random.Range(0f, 0.1f), 0f);
			randomHeading[j] = UnityEngine.Random.Range(0, 360);
			randomMagnitude[j] = UnityEngine.Random.Range(-0.1f, 0.1f);
			beeState[j] = BeeState.INACTIVE;
			beeTransforms[j] = followBees.GetChild(j);
		}
		beeHistory = new Vector3[beeHistoryMaxSize];
		for (int k = 0; k < followBees.childCount; k++)
		{
			followBees.GetChild(k).position = beeSpawnPoint.position;
			followBees.GetChild(k).gameObject.SetActive(value: false);
		}
		bees.gameObject.transform.parent = null;
		Reset();
	}

	public void SetFollowedCharacter(Character character)
	{
		if (!beesUnleashed)
		{
			followedCharacter = character;
			bees.transform.position = beeSpawnPoint.position;
			followBees.transform.parent = null;
			attackDeadBodyTimer = 0f;
			animator.SetTrigger("Trigger");
			AnimateBeeBirth();
		}
	}

	public override void Enable()
	{
		base.Enable();
		SpriteRenderer[] componentsInChildren = followBees.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer obj in componentsInChildren)
		{
			obj.sortingLayerName = "Effects";
			obj.enabled = true;
		}
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		AkSoundEngine.PostEvent("SFX_Pieces_Beehive_Idle_Start", base.gameObject);
		doPlace();
	}

	public override void Place(int playerNumber)
	{
		base.Place(playerNumber);
		doPlace();
	}

	private void doPlace()
	{
		SpriteRenderer[] componentsInChildren = followBees.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer obj in componentsInChildren)
		{
			obj.sortingLayerName = "Effects";
			obj.enabled = true;
		}
		bees.KillType = KillType;
		bees.placedByPlayerNumber = placedByPlayerNumber;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (paused)
		{
			return;
		}
		float num = beeSpeed;
		if (followedCharacter != null)
		{
			targetPosition = followedCharacter.transform.position;
		}
		if (beesMoving)
		{
			if (attackDeadBodyTimer > 0f)
			{
				attackDeadBodyTimer += Time.deltaTime;
				if (attackDeadBodyTimer > attackDeadBodyTime)
				{
					AnimateBeeDeath();
				}
			}
			else if (((followedCharacter.Dead || followedCharacter.Dying) && (!followedCharacter.isGhost || followedCharacter.isZombie)) || followedCharacter.Success || followedCharacter.InBlackHole)
			{
				attackDeadBodyTimer += Time.deltaTime;
				followedCharacter = null;
			}
		}
		else
		{
			targetPosition = beeSpawnPoint.position;
			num = returnBeeSpeed;
		}
		Vector3 vector = targetPosition - bees.transform.position;
		float magnitude = vector.magnitude;
		if (magnitude > 0.001f)
		{
			Vector3 vector2 = vector / magnitude * num * Time.deltaTime;
			if (magnitude < vector2.magnitude)
			{
				vector2 = vector;
			}
			bees.transform.position += vector2;
		}
		beeHistoryCurrentIndex++;
		if (beeHistoryCurrentIndex >= beeHistoryMaxSize)
		{
			beeHistoryCurrentIndex = 0;
		}
		if (beesUnleashed && (followedCharacter == null || followedCharacter.Dying))
		{
			bees.Disappear();
		}
		beeHistory[beeHistoryCurrentIndex] = bees.transform.position;
		int childCount = followBees.childCount;
		for (int i = 0; i < childCount; i++)
		{
			if (beesUnleashed)
			{
				if (followedCharacter != null)
				{
					if (beeState[i] != BeeState.POSTSWARM)
					{
						beeState[i] = BeeState.SWARM;
					}
					if (followedCharacter.Dying && beeState[i] == BeeState.SWARM && UnityEngine.Random.Range(0, randomRateOfPostSwarm + 1) > randomRateOfPostSwarm - 2)
					{
						beeState[i] = BeeState.POSTSWARM;
					}
				}
				else
				{
					if (beeState[i] == BeeState.SWARM && UnityEngine.Random.Range(0, randomRateOfReturnToHive + 1) > randomRateOfReturnToHive - 2)
					{
						beeState[i] = BeeState.POSTSWARM;
					}
					if (beeState[i] == BeeState.POSTSWARM && UnityEngine.Random.Range(0, randomRateOfReturnToHive + 1) > randomRateOfReturnToHive - 2)
					{
						beeState[i] = BeeState.DISAPPEAR;
					}
				}
			}
			else if (i < childCount - indicatorBeeNum)
			{
				beeState[i] = BeeState.INACTIVE;
			}
			else
			{
				beeState[i] = BeeState.WALKONHIVE;
			}
			_ = Vector3.zero;
			Vector3 zero = Vector3.zero;
			vector = Vector3.zero;
			magnitude = 0f;
			Transform transform = beeTransforms[i];
			Vector3 vector3 = Vector3.zero;
			float num2 = 1f;
			float angle = 0f;
			float num3 = 1f;
			switch (beeState[i])
			{
			case BeeState.SWARM:
			{
				randomHeading[i] += (Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedHeading + (float)(i * 23), i) - 0.5f) * randomOffsetFluctuationRate;
				randomMagnitude[i] = Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedMagnitude + (float)(i * 95), i);
				float num4 = randomHeadingSpeed * headingSpeedThroughSwarm.Evaluate((float)i / (float)childCount);
				Vector3 vector5 = new Vector2(Mathf.Cos(randomHeading[i]), Mathf.Sin(randomHeading[i])) * num4 * randomMagnitude[i] * followBeeDistanceToClumpRandomNoiseMod.Evaluate((bees.transform.position - transform.position).magnitude);
				zero = (BeePastPosition(Mathf.FloorToInt(followBeeCurve.Evaluate(i))) - transform.position) * targetModifierSpeed;
				vector = vector5 + zero;
				magnitude = vector.magnitude;
				if (magnitude > 0.001f)
				{
					vector3 = vector / magnitude * followBeeAccelerationDistanceModifier.Evaluate(magnitude) * followBeeAccel * Time.deltaTime;
					num2 = velocityDampeningFriction;
					angle = Mathf.Atan2(followBeesVelocity[i].y, followBeesVelocity[i].x) * 57.29578f;
				}
				break;
			}
			case BeeState.POSTSWARM:
			{
				randomHeading[i] += (Mathf.PerlinNoise((float)ID + Time.time * (noiseTemporalSpeedHeading + (float)i * 0.01f) + (float)(i * 23), i) - 0.5f) * randomOffsetFluctuationRate * MathF.PI * 2f;
				randomMagnitude[i] = Mathf.PerlinNoise((float)ID + Time.time * (noiseTemporalSpeedMagnitude + (float)i * 0.01f) + (float)(i * 95), i);
				Vector3 vector7 = new Vector2(Mathf.Cos(randomHeading[i]), Mathf.Sin(randomHeading[i])) * postSwarmRandomHeadingSpeed * randomMagnitude[i] * followBeeDistanceToClumpRandomNoiseMod.Evaluate((bees.transform.position - transform.position).magnitude);
				zero = Vector3.zero;
				vector = vector7 + zero;
				magnitude = vector.magnitude;
				if (magnitude > 0.001f)
				{
					vector3 = vector / magnitude * followBeeAccelerationDistanceModifier.Evaluate(magnitude) * followBeeAccel * Time.deltaTime;
					num2 = postSwarmRandomFriction;
					angle = randomHeading[i];
					num3 = 0.9f;
				}
				break;
			}
			case BeeState.DISAPPEAR:
			{
				if (!transform.gameObject.activeSelf)
				{
					continue;
				}
				randomHeading[i] += (Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedHeading + (float)(i * 23), i) - 0.5f) * randomOffsetFluctuationRate * MathF.PI * 2f;
				randomMagnitude[i] = Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedMagnitude + (float)(i * 95), i);
				Vector3 vector6 = new Vector2(Mathf.Cos(randomHeading[i]), Mathf.Sin(randomHeading[i])) * returnRandomHeadingSpeed * randomMagnitude[i] * disappearBeeDistanceToClumpRandomNoiseMod.Evaluate((beeSpawnPoint.transform.position - transform.position).magnitude);
				_ = beeSpawnPoint.transform.position - transform.position;
				zero = Vector3.zero;
				vector = vector6 + zero;
				magnitude = vector.magnitude;
				if (magnitude > 0.001f)
				{
					vector3 = vector * disappearBeeAccelerationIndexModifier.Evaluate(i) * followBeeAccel * Time.deltaTime;
					num2 = velocityDampeningFriction;
					angle = randomHeading[i];
					if (transform.transform.localScale.x < 0.03f)
					{
						transform.gameObject.SetActive(value: false);
						num3 = 1f;
					}
					else
					{
						num3 = transform.transform.localScale.x * 0.8f;
					}
				}
				break;
			}
			case BeeState.WALKONHIVE:
			{
				randomHeading[i] += (Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedHeading + (float)(i * 23), i) - 0.5f) * beeWalktemporalNoiseSpeed * MathF.PI * 2f;
				randomMagnitude[i] = Mathf.Max(beeWalkMinimumRandomMagnitude, Mathf.PerlinNoise((float)ID + Time.time * noiseTemporalSpeedMagnitude + (float)(i * 95), i));
				Vector3 vector4 = new Vector2(Mathf.Cos(randomHeading[i]), Mathf.Sin(randomHeading[i])) * beeWalkRandomSpeed * randomMagnitude[i];
				vector = beeWalkPoint.position - transform.position;
				zero = vector * walkingBeeAccelerationDistanceModifier.Evaluate(vector.magnitude) * beeWalkSpeed;
				vector = vector4 + zero;
				magnitude = vector.magnitude;
				if (magnitude > 0.001f)
				{
					vector3 = vector * Time.deltaTime;
					angle = Mathf.Atan2(followBeesVelocity[i].y, followBeesVelocity[i].x) * 57.29578f;
					num2 = beeWalkFriction;
				}
				break;
			}
			}
			if (magnitude > 0.001f)
			{
				followBeesVelocity[i] += vector3;
				lastBeeForce[i] = vector3;
				followBeesVelocity[i] *= num2;
				transform.transform.position += followBeesVelocity[i];
				transform.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
				transform.transform.localScale = Vector3.one * num3;
			}
		}
		if (beeDeathAnim != null && !beeDeathAnim.MoveNext())
		{
			beeDeathAnim = null;
		}
		if (beeBirthAnim != null && !beeBirthAnim.MoveNext())
		{
			beeBirthAnim = null;
		}
	}

	private void AnimateBeeDeath()
	{
		beeDeathAnim = BeeDeathAnimation();
		beeDeathAnim.MoveNext();
	}

	private IEnumerator BeeDeathAnimation()
	{
		beeHazard.gameObject.SetActive(value: false);
		for (float t = 0f; t < attackDeadBodyTime; t += Time.deltaTime)
		{
			yield return null;
		}
		beesMoving = false;
		float returnTimer = 0f;
		int activebees;
		do
		{
			returnTimer += Time.deltaTime;
			activebees = 0;
			for (int i = 0; i < followBees.childCount - indicatorBeeNum; i++)
			{
				Transform child = followBees.GetChild(i);
				if (child.gameObject.activeInHierarchy)
				{
					if ((child.position - beeSpawnPoint.position).magnitude < 0.1f)
					{
						child.gameObject.SetActive(value: false);
					}
					else
					{
						activebees++;
					}
				}
			}
			yield return null;
		}
		while (returnTimer < 8f && activebees > 0);
	}

	private void AnimateBeeBirth()
	{
		beeBirthAnim = BeeBirthAnimation();
		beeBirthAnim.MoveNext();
	}

	private IEnumerator BeeBirthAnimation()
	{
		beesUnleashed = true;
		beesMoving = false;
		indicatorBees.gameObject.SetActive(value: false);
		bees.gameObject.SetActive(value: true);
		beeHazard.gameObject.SetActive(value: false);
		float t = 0f;
		float waitTime = 0f;
		for (int i = 0; i < followBees.childCount; i++)
		{
			while (waitTime < 0.01667f)
			{
				waitTime += Time.deltaTime;
				if (followedCharacter != null && (followedCharacter.Dead || followedCharacter.Dying || followedCharacter.LocallyDead) && !followedCharacter.isZombie && !followedCharacter.isGhost)
				{
					beesMoving = true;
					yield break;
				}
				yield return null;
			}
			t += waitTime;
			waitTime = 0f;
			if (t > birthTime && !beesMoving)
			{
				beesMoving = true;
				beeHazard.gameObject.SetActive(value: true);
			}
			Transform child = followBees.GetChild(i);
			if (i < followBees.childCount - indicatorBeeNum)
			{
				child.position = beeSpawnPoint.position;
			}
			if (attackDeadBodyTimer <= 0.01f)
			{
				child.gameObject.SetActive(value: true);
				continue;
			}
			break;
		}
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (followedCharacter == null && !beesRequested)
		{
			Character componentInParent = c.gameObject.GetComponentInParent<Character>();
			if (!(componentInParent == null) && ((!componentInParent.Dead && !componentInParent.Dying && !componentInParent.LocallyDead) || componentInParent.isZombie || componentInParent.isGhost) && !componentInParent.Success && (!componentInParent.CrouchingDown || (!(c == componentInParent.headCollider) && !(c == componentInParent.hazardHeadcollider) && !(c == componentInParent.coinGrabber))) && componentInParent.hasAuthority)
			{
				componentInParent.CallCmdRequestBees(NetSurrogate.netId);
				beesRequested = true;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		followBees.transform.parent = base.transform;
		followBees.transform.localPosition = Vector3.zero;
		for (int i = 0; i < followBees.childCount; i++)
		{
			Transform child = followBees.GetChild(i);
			child.position = beeSpawnPoint.position;
			child.GetComponentInChildren<SpriteRenderer>().enabled = true;
			if (i < followBees.childCount - indicatorBeeNum)
			{
				child.gameObject.SetActive(value: false);
			}
			else
			{
				child.gameObject.SetActive(value: true);
			}
		}
		bees.transform.position = beeSpawnPoint.position;
		bees.gameObject.SetActive(value: false);
		beeHazard.gameObject.SetActive(value: false);
		beeDeathAnim = null;
		beeBirthAnim = null;
		beesRequested = false;
		beesMoving = false;
		beesUnleashed = false;
		followedCharacter = null;
		AkSoundEngine.PostEvent("SFX_Pieces_Beehive_Idle_Reset", base.gameObject);
	}

	public static Beehive GetBeehiveFromSurrogateID(NetworkInstanceId id)
	{
		GameObject gameObject = ClientScene.FindLocalObject(id);
		if (gameObject != null)
		{
			Transform parent = gameObject.transform.parent;
			if (parent != null)
			{
				Beehive component = parent.GetComponent<Beehive>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	private Vector3 BeePastPosition(int framesInPast)
	{
		if (framesInPast < 0)
		{
			framesInPast = 0;
		}
		if (framesInPast >= beeHistoryMaxSize)
		{
			framesInPast = beeHistoryMaxSize - 1;
		}
		int num = beeHistoryCurrentIndex - framesInPast;
		if (num < 0)
		{
			num = beeHistoryMaxSize + beeHistoryCurrentIndex - framesInPast;
		}
		return beeHistory[num];
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AkSoundEngine.PostEvent("SFX_Pieces_Beehive_Idle_Reset", base.gameObject);
		UnityEngine.Object.Destroy(followBees.gameObject);
		if (bees != null)
		{
			UnityEngine.Object.Destroy(bees.gameObject);
		}
	}
}
