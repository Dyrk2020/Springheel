using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : ActiveBlock
{
	public Teleporter Destination;

	public bool ready;

	public bool triggeredLocally;

	public bool forceOff;

	public SpriteRenderer grid;

	public Animator portalAnimator;

	public Transform TeleportPosition;

	public float TeleportTargetRange;

	public Object sendParticlePrefab;

	public float CoolDownTime;

	private float coolDownTimer;

	public Color[] colorOptions;

	private static Color lastColorUsed;

	public List<GameObject> insideGameObjects = new List<GameObject>();

	private bool recentlyReset;

	private Vector3 lastPosition;

	private Vector3 lastWorldOffset;

	public Teleporter startupLinkedTeleporter;

	public bool preventAutoConnectOnSpawn;

	public static List<Teleporter> AllTeleporters = new List<Teleporter>(128);

	private float teleportTimer;

	private float teleportTime;

	private Character ChrInTeleport;

	private bool triggersThisFrame;

	private bool isTemporarilyDisabled;

	public float TimeToTeleport
	{
		get
		{
			if (Destination != null)
			{
				return GameSettings.GetInstance().teleporterTimeCurve.Evaluate(Mathf.Abs((Destination.transform.position - base.transform.position).magnitude));
			}
			return 1f;
		}
	}

	public bool Blocked => insideGameObjects.Count > 0;

	public bool IsTemporarilyDisabled
	{
		get
		{
			return isTemporarilyDisabled;
		}
		set
		{
			if (isTemporarilyDisabled != value)
			{
				isTemporarilyDisabled = value;
				if (isTemporarilyDisabled)
				{
					ready = false;
				}
			}
		}
	}

	public bool CanTeleport
	{
		get
		{
			if (Destination != null && placed && ready && !IsTemporarilyDisabled)
			{
				return !Destination.IsTemporarilyDisabled;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AllTeleporters.Add(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AllTeleporters.Remove(this);
	}

	protected override void Start()
	{
		base.Start();
		lastPosition = base.transform.position;
		lastWorldOffset = Vector3.zero;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		lastWorldOffset = base.transform.position - lastPosition;
		lastPosition = base.transform.position;
		if (!triggersThisFrame)
		{
			insideGameObjects.Clear();
		}
		else
		{
			insideGameObjects.RemoveAll((GameObject x) => !x.activeInHierarchy);
		}
		triggersThisFrame = false;
		ActTeleport(Time.fixedDeltaTime);
	}

	private bool IsPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Player);
		}
		return false;
	}

	private bool IsBody(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Body);
		}
		return false;
	}

	private bool IsBlocker(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null && !collisionTag.ContainsAnyTag((TagComparer.Tag)65568))
		{
			return collisionTag.ContainsAnyTag((TagComparer.Tag)532608);
		}
		return false;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (recentlyReset)
		{
			return;
		}
		if (insideGameObjects.Contains(c.gameObject) || IsBlocker(c.gameObject, c.GetComponent<CollisionTag>()))
		{
			triggersThisFrame = true;
		}
		if (!CanTeleport)
		{
			return;
		}
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (IsPlayer(c.gameObject, component) && IsBody(c.gameObject, component))
		{
			Character component2 = c.transform.parent.GetComponent<Character>();
			if (component2 == null)
			{
				component2 = c.transform.parent.parent.GetComponent<Character>();
			}
			if (component2 != null && !component2.InCannon && component2.hasAuthority && !component2.Success && (!(component2.Dead | component2.Dying) || !component2.OnGround || component2.isGhost))
			{
				component2.Freeze(hide: true, freezeAnimator: true, disableColliders: true);
				component2.PositionCharacter(Destination.TeleportPosition.position);
				ChrInTeleport = component2;
				teleportTimer = 0f;
				teleportTime = TimeToTeleport;
				component2.ForceAllowSuicideFor(teleportTime);
				StartCoroutine(TeleportingAnimation());
				NetSurrogate.TriggerVal = true;
				triggeredLocally = true;
				ready = false;
				Destination.ready = false;
				coolDownTimer = CoolDownTime + TimeToTeleport;
				Destination.coolDownTimer = Destination.CoolDownTime + Destination.TimeToTeleport;
			}
			return;
		}
		Projectile component3 = c.transform.GetComponent<Projectile>();
		if (!(component3 != null) || !(component3.lastExitedTeleporter != this))
		{
			return;
		}
		if (component3.movingPlatform)
		{
			component3.StartUnstableCrash();
			return;
		}
		Rigidbody2D component4 = component3.GetComponent<Rigidbody2D>();
		Vector3 direction = base.transform.InverseTransformDirection(new Vector3(component4.velocity.x, component4.velocity.y, 0f));
		Vector3 position = component3.transform.TransformPoint(component3.GetComponent<BoxCollider2D>().offset);
		Vector3 vector = base.transform.InverseTransformPoint(position);
		Vector3 vector2 = ActiveCollidersNew[0].GetComponent<BoxCollider2D>().offset.ToVector3();
		Vector3 vector3 = vector2 + base.transform.InverseTransformDirection(lastWorldOffset);
		bool flag = false;
		for (int i = 0; i < 2; i++)
		{
			float num = vector2[i];
			float num2 = vector[i];
			float num3 = vector3[i];
			float num4 = num2 - direction[i] * Time.deltaTime;
			float num5 = ((num < num3) ? num : num3);
			float num6 = ((num < num3) ? num3 : num);
			float num7 = ((num2 < num4) ? num2 : num4);
			float num8 = ((num2 < num4) ? num4 : num2);
			if ((num5 >= num7 && num5 <= num8) || (num7 >= num5 && num7 <= num6))
			{
				flag = true;
			}
		}
		if (flag)
		{
			SpawnProjectileSendParticle(position);
			Vector3 self = base.transform.InverseTransformPoint(component3.transform.position);
			Vector3 vector4 = ActiveCollidersNew[0].GetComponent<BoxCollider2D>().size / 2f;
			Vector3 min = vector2 - vector4;
			Vector3 max = vector2 + vector4;
			self = self.Clamp(min, max);
			component3.transform.position = Destination.transform.position + Destination.transform.rotation * self;
			component3.lastExitedTeleporter = Destination;
			component3.transform.rotation = Quaternion.Inverse(base.transform.rotation) * component3.transform.rotation;
			component3.transform.rotation = Destination.transform.rotation * component3.transform.rotation;
			direction = Destination.transform.TransformDirection(direction);
			component4.velocity = direction;
			Destination.SpawnProjectileSendParticle(component3.transform.TransformPoint(component3.GetComponent<BoxCollider2D>().offset));
		}
	}

	public void SpawnProjectileSendParticle(Vector3 position)
	{
		GameObject obj = (GameObject)Object.Instantiate(sendParticlePrefab);
		obj.GetComponent<TeleporterProjectileSend>().SetColor(grid.color);
		obj.transform.position = position;
		obj.transform.SetParent(base.transform, worldPositionStays: true);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (recentlyReset)
		{
			return;
		}
		if (Destination != null)
		{
			CollisionTag component = c.GetComponent<CollisionTag>();
			Placeable componentInParent = c.GetComponentInParent<Placeable>();
			if ((!(componentInParent != null) || !componentInParent.PickedUp) && (ParentPiece == null || c.gameObject != ParentPiece.gameObject) && ((IsBlocker(c.gameObject, component) && c.gameObject.layer != 22) || c.gameObject.layer == 8))
			{
				if (!Blocked)
				{
					portalAnimator.SetBool("On", value: false);
					Destination.portalAnimator.SetBool("On", value: false);
				}
				NetSurrogate.TriggerVal = true;
				triggeredLocally = true;
				ready = false;
				Destination.ready = false;
				if (!insideGameObjects.Contains(c.gameObject))
				{
					insideGameObjects.Add(c.gameObject);
				}
			}
		}
		OnTriggerStay2D(c);
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		if (!recentlyReset)
		{
			CollisionTag component = c.GetComponent<CollisionTag>();
			if ((ParentPiece == null || c.gameObject != ParentPiece.gameObject) && ((IsBlocker(c.gameObject, component) && c.gameObject.layer != 22) || c.gameObject.layer == 8))
			{
				insideGameObjects.Remove(c.gameObject);
			}
		}
	}

	protected void ActTeleport(float deltaTime)
	{
		recentlyReset = false;
		grid.sortingLayerName = "Effects";
		TeleportPosition.localPosition = new Vector3(0f, (0f - Mathf.Abs(base.transform.eulerAngles.z / 180f - 1f) + 1f) * TeleportTargetRange, 0f);
		if (coolDownTimer > 0f && !paused)
		{
			coolDownTimer -= deltaTime;
		}
		if (NetSurrogate != null && NetSurrogate.TriggerVal && !triggeredLocally)
		{
			StartCoroutine(TeleportingAnimation());
			ready = false;
			Destination.ready = false;
			coolDownTimer = CoolDownTime + TimeToTeleport;
			Destination.coolDownTimer = coolDownTimer;
		}
		if (!ready && Destination != null && !Destination.Blocked && coolDownTimer <= 0f && !Blocked && !IsTemporarilyDisabled && !Destination.IsTemporarilyDisabled)
		{
			portalAnimator.SetBool("On", value: true);
			Destination.portalAnimator.SetBool("On", value: true);
			coolDownTimer = 0f;
		}
		if (Destination == null || IsTemporarilyDisabled || (Destination != null && Destination.IsTemporarilyDisabled))
		{
			portalAnimator.SetBool("On", value: false);
			if (Destination == null)
			{
				CancelTeleport();
			}
		}
		CharacterTeleportUpdate();
	}

	public void setReadyTrue()
	{
		ready = true;
		triggeredLocally = false;
		portalAnimator.ResetTrigger("Receive");
		portalAnimator.ResetTrigger("Receiving");
		portalAnimator.ResetTrigger("Send");
	}

	public void activateSound()
	{
		AkSoundEngine.PostEvent("SFX_Pieces_Teleporter_Activate", base.gameObject);
	}

	public override void Reset()
	{
		base.Reset();
		CancelTeleport();
		if ((bool)Destination && !Destination.Blocked && !Blocked)
		{
			ready = true;
			triggeredLocally = false;
			portalAnimator.SetBool("On", value: true);
		}
		coolDownTimer = 0f;
		recentlyReset = true;
		insideGameObjects.Clear();
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		if (preventAutoConnectOnSpawn)
		{
			preventAutoConnectOnSpawn = false;
		}
		else
		{
			StartCoroutine(waitForSurrogateConnection());
		}
	}

	private IEnumerator waitForSurrogateConnection()
	{
		while (NetSurrogate == null || NetSurrogate.netId.IsEmpty())
		{
			yield return null;
		}
		ConnectToAnotherTeleporter();
	}

	public void ConnectToAnotherTeleporter(int ignoreMe = 0)
	{
		if (NetSurrogate == null || NetSurrogate.hasAuthority)
		{
			if (!(Destination == null))
			{
				return;
			}
			List<Teleporter> list = new List<Teleporter>();
			if (startupLinkedTeleporter != null)
			{
				list.Add(startupLinkedTeleporter);
				startupLinkedTeleporter = null;
			}
			else
			{
				foreach (Teleporter allTeleporter in AllTeleporters)
				{
					if (allTeleporter != null && allTeleporter.Destination == null && allTeleporter != this && allTeleporter.placed && allTeleporter.ID != ignoreMe)
					{
						list.Add(allTeleporter);
					}
				}
			}
			using List<Teleporter>.Enumerator enumerator = list.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Teleporter current2 = enumerator.Current;
				Color color;
				do
				{
					color = colorOptions[Random.Range(0, colorOptions.Length - 1)];
				}
				while (color == lastColorUsed);
				lastColorUsed = color;
				SetGridColor(color);
				current2.SetGridColor(color);
				current2.Destination = this;
				current2.ready = false;
				Destination = current2;
				portalAnimator.SetBool("On", value: true);
				current2.portalAnimator.SetBool("On", value: true);
				ready = false;
				if (NetSurrogate != null && NetSurrogate.hasAuthority)
				{
					NetSurrogate.IntVal = current2.ID;
					NetSurrogate.StringVal = color.ToString();
				}
			}
			return;
		}
		if (NetSurrogate != null)
		{
			StartCoroutine(waitForNewDestination(ignoreMe));
		}
	}

	private IEnumerator waitForNewDestination(int ignoreMe = 0)
	{
		if (!(NetSurrogate != null))
		{
			yield break;
		}
		while (NetSurrogate.IntVal == 0 || NetSurrogate.IntVal == ignoreMe)
		{
			yield return null;
		}
		foreach (Teleporter allTeleporter in AllTeleporters)
		{
			if (allTeleporter != null && allTeleporter.ID == NetSurrogate.IntVal)
			{
				allTeleporter.Destination = this;
				allTeleporter.ready = false;
				Destination = allTeleporter;
				ready = false;
				portalAnimator.SetBool("On", value: true);
				allTeleporter.portalAnimator.SetBool("On", value: true);
				string[] array = NetSurrogate.StringVal.Substring(5, 26).Split(',');
				Color gridColor = (lastColorUsed = new Color(Parsing.ParseFloat_InvariantCulture(array[0]), Parsing.ParseFloat_InvariantCulture(array[1]), Parsing.ParseFloat_InvariantCulture(array[2]), Parsing.ParseFloat_InvariantCulture(array[3])));
				SetGridColor(gridColor);
				allTeleporter.SetGridColor(gridColor);
			}
		}
	}

	public override void DestroySelf(bool destroyChildren = false, bool useSmoke = true, bool sendNetworkSignal = true)
	{
		CancelTeleport();
		bool num = base.MarkedForDestruction;
		base.DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
		if (!num && Destination != null)
		{
			Destination.Destination = null;
			Destination.portalAnimator.SetBool("On", value: false);
			Destination.ConnectToAnotherTeleporter(ID);
		}
	}

	private void CharacterTeleportUpdate()
	{
		if (!ChrInTeleport || paused)
		{
			return;
		}
		teleportTimer += Time.deltaTime;
		if (teleportTimer >= teleportTime)
		{
			ChrInTeleport.PositionCharacter(Destination.TeleportPosition.position);
			SaveFileData saveFileDataFromNetworkNumber = StatTracker.Instance.GetSaveFileDataFromNetworkNumber(ChrInTeleport.networkNumber, fallback: true);
			if (saveFileDataFromNetworkNumber != null)
			{
				saveFileDataFromNetworkNumber.IncrementStat("TimesTeleported");
				AchievementChecker.Instance.Space_Time_Cadet_AchievementCheck(saveFileDataFromNetworkNumber);
			}
			CancelTeleport();
		}
	}

	public IEnumerator TeleportingAnimation()
	{
		float timer = 0f;
		portalAnimator.SetTrigger("Send");
		portalAnimator.SetBool("On", value: false);
		AkSoundEngine.PostEvent("SFX_Pieces_Teleporter_Send", base.gameObject);
		if (Destination != null)
		{
			Destination.portalAnimator.SetTrigger("Receiving");
		}
		do
		{
			if (!paused)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
		while (timer <= TimeToTeleport && ChrInTeleport != null);
		if (Destination != null)
		{
			Destination.portalAnimator.SetTrigger("Receive");
		}
		yield return null;
		if (Destination != null)
		{
			Destination.portalAnimator.SetBool("On", value: false);
		}
		AkSoundEngine.PostEvent("SFX_Pieces_Teleporter_Arrive", base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		portalAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		portalAnimator.speed = 1f;
	}

	private void SetGridColor(Color c)
	{
		grid.color = c;
		for (int i = 0; i < ArtSprites.Length; i++)
		{
			if (ArtSprites[i] == grid)
			{
				initialColors[i] = c;
				break;
			}
		}
	}

	public void CancelTeleport()
	{
		if (ChrInTeleport != null)
		{
			ChrInTeleport.Unfreeze();
			ChrInTeleport = null;
		}
	}

	public static void OnCharacterRespawned(Character chr)
	{
		foreach (Teleporter allTeleporter in AllTeleporters)
		{
			if (allTeleporter != null && allTeleporter.ChrInTeleport != null && allTeleporter.ChrInTeleport == chr)
			{
				allTeleporter.CancelTeleport();
			}
		}
	}
}
