using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class Coin : ActiveBlock
{
	public delegate Vector3 TargetPositionModifier(Vector3 currentPosition, Vector3 targetPosition);

	public Transform coinHolder;

	public float returnToHolderTime;

	public float coinFollowDistance;

	public AnimationCurve coinFollowSpeed;

	protected Character carrier;

	protected Vector3 startPosition;

	protected float randomExtraDistance;

	protected bool atSpawnPoint = true;

	protected bool awarded;

	protected bool coinMoving;

	protected Transform parentPiece;

	protected bool pickupRequested;

	protected Character pickupRequester;

	protected Animator animator;

	public GameObject coinEffect;

	protected int lastSurrogateInt;

	public static TargetPositionModifier ModifyTargetPosition;

	public static List<Coin> AllCoins = new List<Coin>();

	private bool skipMove;

	public string CoinPickUpSFX;

	public bool ForceAlwaysRespawn;

	protected bool AlwaysRespawn
	{
		get
		{
			if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY && GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE)
			{
				return ForceAlwaysRespawn;
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

	private void OnEnable()
	{
		AllCoins.Add(this);
	}

	private void OnDisable()
	{
		AllCoins.Remove(this);
	}

	protected void Update()
	{
		if (disabled)
		{
			return;
		}
		if (base.Active)
		{
			Character character = carrier;
			if (character == null && pickupRequested)
			{
				character = pickupRequester;
			}
			if (character != null)
			{
				Vector3 position = base.transform.position;
				Vector3 position2 = character.transform.position;
				Vector3 vector = ((!(position.x > position2.x)) ? (position2 + new Vector3(-1f, 0f, 0f) * (coinFollowDistance + randomExtraDistance)) : (position2 + new Vector3(1f, 0f, 0f) * (coinFollowDistance + randomExtraDistance)));
				if (ModifyTargetPosition != null)
				{
					vector = ModifyTargetPosition(position, vector);
				}
				float sqrMagnitude = (vector - position).sqrMagnitude;
				base.transform.position = Vector3.MoveTowards(position, vector, coinFollowSpeed.Evaluate(sqrMagnitude) * Time.deltaTime);
			}
			if (!(carrier != null))
			{
				return;
			}
			bool flag = GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY;
			if (carrier.Dying)
			{
				carrier.CoinsCollected = 0;
				if (AlwaysRespawn && flag)
				{
					if (carrier.hasAuthority)
					{
						carrier.CallCmdDroppedCoin(NetSurrogate.netId, base.transform.position, returnToCoinSpawn: true);
					}
				}
				else if (carrier.hasAuthority)
				{
					carrier.CallCmdDroppedCoin(NetSurrogate.netId, base.transform.position, returnToCoinSpawn: false);
				}
				else
				{
					carrier = null;
				}
			}
			else if (carrier.Success && !awarded)
			{
				if (AlwaysRespawn && flag)
				{
					StartCoroutine(returnCoinToSpawn());
				}
				else if (carrier.hasAuthority)
				{
					carrier.CallCmdFinishedWithCoin(NetSurrogate.netId);
				}
				DoAwardAnimation();
				if (AlwaysRespawn)
				{
					base.MarkedForDestruction = false;
				}
			}
		}
		else if (!atSpawnPoint && !coinMoving)
		{
			StartCoroutine(returnCoinToSpawn());
		}
	}

	public void Drop(Vector2 coinPosition, bool returnToRespawn = false)
	{
		if (carrier != null)
		{
			SaveFileData saveFileDataFromNetworkNumber = StatTracker.Instance.GetSaveFileDataFromNetworkNumber(carrier.networkNumber, fallback: true);
			if (saveFileDataFromNetworkNumber != null)
			{
				saveFileDataFromNetworkNumber.IncrementStat("CoinsLost");
				AchievementChecker.Instance.Droppin_Bills_AchievementCheck(saveFileDataFromNetworkNumber);
			}
			if (saveFileDataFromNetworkNumber != null)
			{
				AchievementChecker.Instance.Droppin_Bills_AchievementCheck(saveFileDataFromNetworkNumber);
			}
		}
		carrier = null;
		SwitchColliderTo(ColliderModeEnum.RunPhase);
		base.transform.position = coinPosition;
		if (returnToRespawn)
		{
			StartCoroutine(returnCoinToSpawn());
		}
	}

	public void DoAwardAnimation()
	{
		carrier = null;
		awarded = true;
		coinHolder.transform.SetParent(base.transform);
		AkSoundEngine.PostEvent("SFX_Pieces_Coin_ReachGoal", base.gameObject);
		if ((bool)coinEffect)
		{
			Object.Instantiate(coinEffect, base.transform.position, base.transform.rotation);
		}
		if (!AlwaysRespawn)
		{
			if (ParentPiece != null)
			{
				ParentPiece.DetachPiece(this, removeFromGroup: false);
			}
			base.MarkedForDestruction = true;
		}
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = false;
		}
	}

	protected override void Activate()
	{
		base.Activate();
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
		if (carrier == null && !coinMoving && base.Active && !pickupRequested)
		{
			Character componentInParent = c.gameObject.GetComponentInParent<Character>();
			if (!(componentInParent == null) && !componentInParent.Dead && !componentInParent.Dying && (!componentInParent.LocallyDead || componentInParent.isZombie) && (!componentInParent.isZombie || !componentInParent.zombieLocallyDead) && (!componentInParent.CrouchingDown || (!(c == componentInParent.headCollider) && !(c == componentInParent.hazardHeadcollider) && !(c == componentInParent.coinGrabber))) && componentInParent.hasAuthority)
			{
				componentInParent.CallCmdRequestGrabCoin(NetSurrogate.netId);
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
			if (!atSpawnPoint)
			{
				if (carrier != null)
				{
					StatTracker.Instance.GetSaveFileDataFromNetworkNumber(carrier.networkNumber, fallback: true)?.IncrementStat("CoinsStolen");
				}
			}
			else
			{
				atSpawnPoint = false;
			}
			AkSoundEngine.PostEvent(CoinPickUpSFX, base.gameObject);
			base.transform.rotation = Quaternion.identity;
			SwitchColliderTo(ColliderModeEnum.NoColliders);
			if ((bool)ParentPiece)
			{
				base.transform.parent = null;
			}
		}
		else
		{
			Debug.LogError("Attempted to pick up coin with a carrier.");
		}
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		startPosition = base.transform.position;
		atSpawnPoint = true;
		randomExtraDistance = Random.Range(-1f, 1f);
	}

	protected virtual IEnumerator returnCoinToSpawn()
	{
		SwitchColliderTo(ColliderModeEnum.NoColliders);
		coinMoving = true;
		Vector3 start = coinHolder.transform.position;
		Vector3 target = startPosition;
		float t = 0f;
		while (t < returnToHolderTime)
		{
			if (skipMove)
			{
				t = returnToHolderTime;
			}
			t += Time.deltaTime;
			float t2 = Mathf.SmoothStep(0f, 1f, t / returnToHolderTime);
			base.transform.position = Vector3.Lerp(start, target, t2);
			yield return null;
		}
		base.transform.position = target;
		skipMove = false;
		coinMoving = false;
		atSpawnPoint = true;
		awarded = false;
		if ((bool)ParentPiece)
		{
			base.transform.parent = ParentPiece.transform;
		}
		SwitchColliderTo(ColliderModeEnum.RunPhase);
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = true;
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

	public static Coin GetCoinFromSurrogateID(NetworkInstanceId id)
	{
		GameObject gameObject = ClientScene.FindLocalObject(id);
		if (gameObject != null)
		{
			Transform parent = gameObject.transform.parent;
			if (parent != null)
			{
				Coin component = parent.GetComponent<Coin>();
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
		chr.CoinsCollected++;
		Pickup(chr);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(LevelResetEvent) && coinMoving && !skipMove)
		{
			skipMove = true;
		}
	}

	public override void EnablePlaced()
	{
		base.EnablePlaced();
		if (AlwaysRespawn && coinMoving && !skipMove)
		{
			skipMove = true;
		}
	}
}
