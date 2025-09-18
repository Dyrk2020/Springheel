using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class PartyBox : UIGraphic, IGameEventListener
{
	public enum BlockSelectionMode
	{
		Random,
		ForceStartMostlyPlatforms,
		ForcePlatforms,
		TrueRandom
	}

	private GameObject[] groups;

	public float itemScale = 0.7f;

	private List<PickableBlock> pieces = new List<PickableBlock>();

	private PartyPickCursor[] cursors = new PartyPickCursor[0];

	protected int remainingPickingCursor;

	public Vector2 BoxSize;

	public Vector3 PlacementOffset;

	public float ChallengeRange = 1f / 3f;

	public int MaxLoseStreak;

	public float PlacementRadius;

	public float RadiusRange;

	public float CursorRadius = 0.7f;

	public float CursorAppearDelay;

	public float CursorEnableDelay;

	public PartyPickCursor CursorPrefab;

	public float LastDiff;

	public float LastArea;

	public Animator boxAnimator;

	private bool paused;

	private bool scoreboard;

	public Sprite extraBoxL;

	public Sprite extraBoxR;

	public Sprite regularPartyL;

	public Sprite regularPartyR;

	public Sprite chineseTraditionalL;

	public Sprite chineseTraditionalR;

	public Sprite chineseSimplifiedL;

	public Sprite chineseSimplifiedR;

	public SpriteRenderer boxTopL;

	public SpriteRenderer BoxTopR;

	public GameObject boxFlapL;

	public GameObject boxFlapR;

	protected List<SpriteRenderer> boxFlapSprites = new List<SpriteRenderer>();

	public NetworkSurrogate NetSurrogate;

	public bool HasAuthority;

	protected bool CursorsFound;

	public Camera UICamera;

	public Placeable[] BombPrefab;

	public Placeable CoinPrefab;

	public List<PickableBlock> twitchSelectedItems = new List<PickableBlock>();

	private const float hideDelay = 0.5f;

	private float hideCounter;

	public BlockSelectionMode blockSelectionMode;

	public float Density;

	public float Difficulty;

	private WeightedBlockList baseBlockWeights;

	private WeightedBlockList currentBlockWeights;

	private WeightedBlockList bombPrefabWeights;

	private WeightedBlockList coinPrefabWeights;

	public Placeable gluePrefab;

	private Dictionary<Placeable, List<Placeable>> filteredVariantList;

	public Transform BlockWeightDebugHolder;

	public Transform TopRightHandle;

	public List<PickableBlock> DebugPickables = new List<PickableBlock>();

	public float RandomY = 3f;

	public bool DebugWeightDisplay;

	private bool debugWeightDisplayInitialized;

	private bool partybox_debug;

	public bool IsStillActive
	{
		get
		{
			if (!base.Visible)
			{
				return hideCounter > 0f;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InitializeWeightedBlockList();
		InitializeFilteredVariantList();
		InitializeFilteredBombsAndCoins();
		PlaceableMetadataList metaList = LobbyManager.instance.CurrentGameController.MetaList;
		GameObject[] allBlockPrefabs = metaList.allBlockPrefabs;
		foreach (GameObject gameObject in allBlockPrefabs)
		{
			if (gameObject.GetComponent<Placeable>().PickableBlock != null)
			{
				ClientScene.RegisterPrefab(gameObject.GetComponent<Placeable>().PickableBlock.gameObject);
			}
			else
			{
				Debug.Log("This block doesn't have a pickable, might cause problems:" + gameObject.name);
			}
		}
		allBlockPrefabs = metaList.extraBlocks;
		foreach (GameObject gameObject2 in allBlockPrefabs)
		{
			if (gameObject2.GetComponent<Placeable>().PickableBlock != null)
			{
				ClientScene.RegisterPrefab(gameObject2.GetComponent<Placeable>().PickableBlock.gameObject);
			}
			else
			{
				Debug.Log("This block doesn't have a pickable, might cause problems:" + gameObject2.name);
			}
		}
		boxFlapSprites.AddRange(boxFlapL.GetComponentsInChildren<SpriteRenderer>());
		boxFlapSprites.AddRange(boxFlapR.GetComponentsInChildren<SpriteRenderer>());
	}

	private void Start()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<PickBlockEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<GamePlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<TwitchItemVoteEvent>(this, adding);
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding);
	}

	public List<Placeable> GenerateAdditionalPieces(int forcedBombs, int forcedCoins)
	{
		List<Placeable> list = new List<Placeable>(forcedBombs + forcedCoins);
		for (int i = 0; i < forcedBombs; i++)
		{
			Placeable randomPlaceablePrefab = bombPrefabWeights.GetRandomPlaceablePrefab();
			if (randomPlaceablePrefab != null)
			{
				list.Add(randomPlaceablePrefab);
			}
		}
		for (int j = 0; j < forcedCoins; j++)
		{
			Placeable randomPlaceablePrefab2 = coinPrefabWeights.GetRandomPlaceablePrefab();
			if (randomPlaceablePrefab2 != null)
			{
				list.Add(randomPlaceablePrefab2);
			}
		}
		return list;
	}

	public void ChoosePieces(int amount, List<Placeable> forceBlocks)
	{
		if (partybox_debug)
		{
			Debug.Log("PartyBox: Choose pieces: " + amount + " blocks and " + ((forceBlocks != null && forceBlocks.Count > 0) ? forceBlocks.Count : 0) + " forced block(s).");
		}
		ClearPieces();
		if (twitchSelectedItems.Count > 0)
		{
			foreach (PickableBlock twitchSelectedItem in twitchSelectedItems)
			{
				if (amount > 0)
				{
					PickableBlock pickableBlock = UnityEngine.Object.Instantiate(twitchSelectedItem);
					pickableBlock.NetworkisTwitchItem = true;
					pieces.Add(pickableBlock);
					if (partybox_debug)
					{
						Debug.Log("PartyBox: Added twitch selected item: " + pickableBlock.name);
					}
					amount--;
					continue;
				}
				break;
			}
			twitchSelectedItems.Clear();
		}
		if (forceBlocks != null)
		{
			for (int i = 0; i < forceBlocks.Count; i++)
			{
				if (amount <= 0)
				{
					break;
				}
				Placeable placeable = forceBlocks[i];
				bool flag = true;
				if (placeable.FilterOverride.Length == 0)
				{
					flag = GameSettings.GetInstance().itemFilter[forceBlocks[i]].Enabled;
				}
				else
				{
					for (int j = 0; j != placeable.FilterOverride.Length; j++)
					{
						if (!GameSettings.GetInstance().itemFilter[placeable.FilterOverride[j]].Enabled)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					PickableBlock pickableBlock2 = UnityEngine.Object.Instantiate(forceBlocks[i].PickableBlock);
					pieces.Add(pickableBlock2);
					if (partybox_debug)
					{
						Debug.Log("PartyBox: Added forced block: " + pickableBlock2.name);
					}
					amount--;
				}
			}
		}
		ComputeEffectiveBlockWeights();
		for (int k = 0; k < amount; k++)
		{
			PickableBlock pickableBlock3 = null;
			pickableBlock3 = SelectRandomPiece();
			if (pickableBlock3 != null)
			{
				PickableBlock item = UnityEngine.Object.Instantiate(pickableBlock3);
				pieces.Add(item);
				if (partybox_debug)
				{
					Debug.Log("PartyBox: Added random block: " + pickableBlock3.name);
				}
			}
			else
			{
				Debug.LogError("Could not select a random block.");
			}
		}
		System.Random random = new System.Random();
		int num = 0;
		num = pieces.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(num + 1);
			PickableBlock value = pieces[index];
			pieces[index] = pieces[num];
			pieces[num] = value;
		}
		if (partybox_debug)
		{
			Debug.Log("PartyBox: Positioning " + pieces.Count + " instantiated blocks in party box");
		}
		float num2 = 360f / (float)pieces.Count;
		for (int l = 0; l != pieces.Count; l++)
		{
			pieces[l].transform.parent = base.transform;
			pieces[l].NetworkInPartybox = true;
			pieces[l].ChangeArtLayer("Background 2");
			Transform[] componentsInChildren = pieces[l].GetComponentsInChildren<Transform>();
			for (int m = 0; m < componentsInChildren.Length; m++)
			{
				componentsInChildren[m].gameObject.layer = 5;
			}
			float num3 = UnityEngine.Random.Range((0f - RadiusRange) / 2f, RadiusRange / 2f) + PlacementRadius;
			Vector3 localPosition = PlacementOffset + new Vector3(Mathf.Cos(num2 * (float)l * (MathF.PI / 180f)) * num3, Mathf.Sin(num2 * (float)l * (MathF.PI / 180f)) * num3, 0f);
			pieces[l].NetworkUseStartPosition = true;
			pieces[l].NetworkFindPartyBox = true;
			pieces[l].transform.localPosition = localPosition;
			pieces[l].NetworkStartPosition = pieces[l].transform.localPosition;
			pieces[l].setInitialScale(GameSettings.GetInstance().partyBoxItemScale * pieces[l].PartyBoxScale);
			for (int n = 0; n < pieces[l].PickColliders.Length; n++)
			{
				pieces[l].transform.localPosition = BoundSetter(pieces[l], pieces[l].transform.localPosition, pieces[l].PickColliders[n], BoundingBox);
			}
			pieces[l].Enable();
			NetworkServer.Spawn(pieces[l].gameObject);
			if (partybox_debug)
			{
				Debug.Log("PartyBox: Spawned " + pieces[l].name + " over the network.");
			}
		}
	}

	private void InitializeWeightedBlockList()
	{
		GameObject[] allBlockPrefabs = PlaceableMetadataList.Instance.allBlockPrefabs;
		baseBlockWeights = new WeightedBlockList(allBlockPrefabs.Length);
		for (int i = 0; i < allBlockPrefabs.Length; i++)
		{
			Placeable component = allBlockPrefabs[i].GetComponent<Placeable>();
			if (!component.isSetPiece)
			{
				int itemFilterWeight = BlockGroup.GetItemFilterWeight(component, (int)component.BaseRarity);
				baseBlockWeights.AddWeight(i, component, itemFilterWeight);
			}
		}
	}

	private void InitializeFilteredVariantList()
	{
		filteredVariantList = new Dictionary<Placeable, List<Placeable>>();
		foreach (KeyValuePair<Placeable, List<Placeable>> placeableVariant in PlaceableMetadataList.Instance.PlaceableVariants)
		{
			List<Placeable> list = new List<Placeable>();
			foreach (Placeable item in placeableVariant.Value)
			{
				bool flag = true;
				if (item.FilterOverride.Length != 0)
				{
					Placeable[] filterOverride = item.FilterOverride;
					for (int i = 0; i < filterOverride.Length; i++)
					{
						_ = filterOverride[i];
						if (baseBlockWeights.GetWeightForPlaceable(item) == 0)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					list.Add(item);
				}
			}
			filteredVariantList.Add(placeableVariant.Key, list);
		}
	}

	private void InitializeFilteredBombsAndCoins()
	{
		bombPrefabWeights = new WeightedBlockList(BombPrefab.Length);
		for (int i = 0; i < BombPrefab.Length; i++)
		{
			int weightForPlaceable = baseBlockWeights.GetWeightForPlaceable(BombPrefab[i]);
			bombPrefabWeights.AddWeight(i, BombPrefab[i], weightForPlaceable);
		}
		coinPrefabWeights = new WeightedBlockList(1);
		coinPrefabWeights.AddWeight(0, CoinPrefab, baseBlockWeights.GetWeightForPlaceable(CoinPrefab));
	}

	private void InitializeWeightBlockDebugView()
	{
		if (!DebugWeightDisplay || !Application.isEditor)
		{
			return;
		}
		GameObject gameObject = new GameObject("weightObjectHolderDebugger");
		GameObject gameObject2 = new GameObject("TopRight");
		gameObject.transform.parent = null;
		gameObject.transform.position = new Vector3(10000f, 10000f, 0f);
		gameObject2.transform.position = new Vector3(10016f, 10009f, 0f);
		gameObject2.transform.parent = gameObject.transform;
		BlockWeightDebugHolder = gameObject.transform;
		TopRightHandle = gameObject2.transform;
		GameObject[] allBlockPrefabs = PlaceableMetadataList.Instance.allBlockPrefabs;
		for (int i = 0; i < allBlockPrefabs.Length; i++)
		{
			PickableBlock pickableBlock = allBlockPrefabs[i].GetComponent<Placeable>().PickableBlock;
			if (!pickableBlock.placeablePrefab.isSetPiece)
			{
				float x = UnityEngine.Random.Range(0f, TopRightHandle.transform.localPosition.x);
				GameObject obj = UnityEngine.Object.Instantiate(pickableBlock.gameObject, BlockWeightDebugHolder.position + new Vector3(x, 0f, 0f), Quaternion.identity, BlockWeightDebugHolder);
				PickableBlock component = obj.GetComponent<PickableBlock>();
				obj.transform.parent = BlockWeightDebugHolder;
				obj.transform.localScale = Vector3.one * component.BlockProbabilityScale;
				DebugPickables.Add(component);
			}
		}
		debugWeightDisplayInitialized = true;
	}

	private void DisplayComputedWeigthsDebug()
	{
		foreach (PickableBlock debugPickable in DebugPickables)
		{
			float num = (float)currentBlockWeights.GetWeightForPlaceable(debugPickable.placeablePrefab) / (float)currentBlockWeights.GetMaxWeight();
			float y = Mathf.Lerp(0f, TopRightHandle.transform.localPosition.y, num) + UnityEngine.Random.Range(0f, RandomY);
			debugPickable.transform.localPosition = new Vector3(debugPickable.transform.localPosition.x, y, 0f);
			float num2 = debugPickable.BlockProbabilityScale * num;
			if (num2 < 0.05f)
			{
				debugPickable.transform.localScale = Vector3.one * 0.05f;
			}
			else
			{
				debugPickable.transform.localScale = Vector3.one * num2;
			}
			if (currentBlockWeights.GetWeightForPlaceable(debugPickable.placeablePrefab) == 0)
			{
				debugPickable.Enable(enable: false);
			}
			else
			{
				debugPickable.Enable(enable: true);
			}
		}
	}

	public void ComputeEffectiveBlockWeights()
	{
		currentBlockWeights = baseBlockWeights.Clone();
		switch (blockSelectionMode)
		{
		case BlockSelectionMode.Random:
			if (Difficulty < 0.5f)
			{
				float num = 1f - Difficulty * 2f;
				float multiplier = 1f + num * 0.5f;
				float multiplier2 = 1f - num * 0.5f;
				currentBlockWeights.ApplySkew(Placeable.Tag.StaticBlock, multiplier);
				currentBlockWeights.ApplySkew((Placeable.Tag)132, multiplier2);
			}
			else
			{
				float num2 = (Difficulty - 0.5f) * 2f;
				float num3 = 1f + num2;
				float multiplier3 = 1f - num2 * 0.5f;
				currentBlockWeights.ApplySkew(Placeable.Tag.Hazard, num3 * 1.5f);
				currentBlockWeights.ApplySkew((Placeable.Tag)130, num3);
				currentBlockWeights.ApplySkew(Placeable.Tag.StaticBlock, multiplier3);
			}
			if (Density < 0.5f)
			{
				float num4 = 1f - Density * 2f;
				float num5 = 1f + num4 * 0.5f;
				float num6 = 1f - num4 * 0.5f;
				currentBlockWeights.ApplySkew((Placeable.Tag)514, num6);
				currentBlockWeights.ApplySkew(Placeable.Tag.Bomb, num5);
				currentBlockWeights.ApplyAreaSkewAbove(4f, num6 * 0.5f);
				currentBlockWeights.ApplyAreaSkewBelow(4f, num5 * 0.5f);
			}
			else
			{
				float num7 = (Difficulty - 0.5f) * 2f;
				float multiplier4 = 1f + num7 * 0.5f;
				float multiplier5 = 1f - num7 * 0.5f;
				currentBlockWeights.ApplySkew((Placeable.Tag)642, multiplier4);
				currentBlockWeights.ApplySkew(Placeable.Tag.Bomb, multiplier5);
				currentBlockWeights.ApplyAreaSkewAbove(4f, multiplier4);
				currentBlockWeights.ApplyAreaSkewBelow(4f, multiplier5);
			}
			break;
		case BlockSelectionMode.ForcePlatforms:
			currentBlockWeights.ApplySkewNot((Placeable.Tag)545, 0.01f);
			break;
		case BlockSelectionMode.ForceStartMostlyPlatforms:
			currentBlockWeights.ApplySkew(Placeable.Tag.StaticBlock, 1.5f);
			currentBlockWeights.ApplySkewNot((Placeable.Tag)5, 0.8f);
			currentBlockWeights.ApplySkew(Placeable.Tag.Hazard, 0.1f);
			currentBlockWeights.ApplySkew(Placeable.Tag.Bomb, 0.001f);
			break;
		}
		currentBlockWeights.RecomputeTotalWeights();
		if (currentBlockWeights.isEmpty)
		{
			currentBlockWeights = baseBlockWeights.Clone();
			Debug.Log("Block Weight Computation result in an empty set of blocks.  Using base blocks.");
		}
		if (DebugWeightDisplay && Application.isEditor)
		{
			if (!debugWeightDisplayInitialized)
			{
				InitializeWeightBlockDebugView();
			}
			DisplayComputedWeigthsDebug();
		}
	}

	public PickableBlock SelectRandomPiece()
	{
		Placeable placeable = currentBlockWeights.GetRandomPlaceablePrefab();
		float num = (float)GameSettings.GetInstance().itemFilter[gluePrefab].Frequency / 9f;
		if (placeable != null)
		{
			num += placeable.UseGluedVariantModifier;
			List<Placeable> value = null;
			if (filteredVariantList.TryGetValue(placeable, out value) && UnityEngine.Random.Range(0f, 1f) < num * 0.9f)
			{
				int index = UnityEngine.Random.Range(1, value.Count);
				placeable = value[index];
			}
			return placeable.PickableBlock;
		}
		Debug.LogError("Could not get random block.");
		return null;
	}

	public Vector3 BoundSetter(PickableBlock block, Vector3 centerPos, Collider2D collider, Bounds outsideBounds)
	{
		Vector3 result = centerPos;
		Bounds bounds = collider.bounds;
		float num = GameSettings.GetInstance().partyBoxItemScale * block.PartyBoxScale;
		Vector2 offset = collider.offset;
		if (centerPos.x + bounds.extents.x * num + offset.x > outsideBounds.extents.x)
		{
			result.x = outsideBounds.extents.x - bounds.extents.x * num - offset.x;
		}
		if (centerPos.x - bounds.extents.x * num + offset.x < 0f - outsideBounds.extents.x)
		{
			result.x = 0f - outsideBounds.extents.x + bounds.extents.x * num - offset.x;
		}
		if (centerPos.y + bounds.extents.y * num + offset.y > outsideBounds.extents.y)
		{
			result.y = outsideBounds.extents.y - bounds.extents.y * num - offset.y;
		}
		if (centerPos.y - bounds.extents.y * num + offset.y < 0f - outsideBounds.extents.y)
		{
			result.y = 0f - outsideBounds.extents.y + bounds.extents.y * num - offset.y;
		}
		return result;
	}

	public void SetPlayerCount(int players)
	{
		if (players < 1 || players > 4)
		{
			Debug.LogError("PartyBox.SetPlayerCount: invalid number of players: " + players);
		}
		else
		{
			cursors = new PartyPickCursor[players];
		}
	}

	public PartyPickCursor AddPlayer(int playerNumber, Character.Animals animal)
	{
		if (partybox_debug)
		{
			Debug.Log("Spawning party cursor " + playerNumber);
		}
		if (playerNumber > 4 || playerNumber < 1)
		{
			return null;
		}
		PartyPickCursor partyPickCursor = UnityEngine.Object.Instantiate(CursorPrefab);
		partyPickCursor.name = animal.ToString() + " party cursor";
		AkSoundEngine.SetSwitch("Character", animal.ToString(), base.gameObject);
		GamePlayer gamePlayer = LobbyManager.instance.PlayerTracker.GetGamePlayer(playerNumber);
		partyPickCursor.NetworkPlayerAnimal = animal;
		partyPickCursor.NetworkFindControllerOnSpawn = true;
		partyPickCursor.NetworklocalNumber = gamePlayer.localNumber;
		partyPickCursor.LocalPlayer = gamePlayer.LocalPlayer;
		partyPickCursor.NetworknetworkNumber = gamePlayer.networkNumber;
		partyPickCursor.AssociatedGamePlayer = gamePlayer;
		partyPickCursor.SetLayer(5, isServer: true);
		partyPickCursor.UseCamera = UICamera;
		partyPickCursor.Disable();
		Debug.Log("[Net] " + partyPickCursor?.ToString() + " - " + LobbyManager.instance?.ToString() + " - " + gamePlayer?.ToString() + " - " + playerNumber);
		NetworkServer.SpawnWithClientAuthority(partyPickCursor.gameObject, gamePlayer.gameObject);
		if (cursors.Length >= playerNumber)
		{
			cursors[playerNumber - 1] = partyPickCursor;
		}
		return partyPickCursor;
	}

	protected void ClearPieces()
	{
		if (partybox_debug)
		{
			Debug.Log("PartyBox: Cleared Pieces");
		}
		foreach (PickableBlock piece in pieces)
		{
			UnityEngine.Object.Destroy(piece.gameObject);
		}
		pieces.Clear();
	}

	public void ShowBox(bool extraBox = false)
	{
		if (extraBox)
		{
			boxTopL.sprite = extraBoxL;
			BoxTopR.sprite = extraBoxR;
		}
		else
		{
			boxTopL.sprite = regularPartyL;
			BoxTopR.sprite = regularPartyR;
		}
		boxAnimator.SetBool("BoxOpen", value: true);
		if (!CursorsFound)
		{
			cursors = UnityEngine.Object.FindObjectsOfType<PartyPickCursor>();
			CursorsFound = true;
		}
		base.Visible = true;
		remainingPickingCursor = cursors.Length;
		if (HasAuthority)
		{
			MsgPartyBoxOpen msgPartyBoxOpen = new MsgPartyBoxOpen();
			msgPartyBoxOpen.IsOpen = true;
			msgPartyBoxOpen.isExtraBox = extraBox;
			NetworkServer.SendToAll(NetMsgTypes.PartyBoxOpen, msgPartyBoxOpen);
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		if (partybox_debug)
		{
			Debug.Log("Hiding Party Box...");
		}
		base.Visible = false;
		boxAnimator.SetBool("BoxOpen", value: false);
		hideCounter = 0.5f;
	}

	public void CenterCursors()
	{
		if (cursors.Length == 0)
		{
			return;
		}
		float num = (float)(360 / cursors.Length) * (MathF.PI / 180f);
		for (int i = 0; i != cursors.Length; i++)
		{
			if (cursors[i] != null)
			{
				cursors[i].transform.localPosition = new Vector3(Mathf.Cos(num * (float)i) * CursorRadius, Mathf.Sin(num * (float)i) * CursorRadius, 0f);
				cursors[i].transform.localRotation = default(Quaternion);
			}
		}
	}

	public void Pause()
	{
	}

	public void Unpause()
	{
		if (!scoreboard)
		{
			_ = paused;
		}
	}

	public void FinishHide()
	{
		if (remainingPickingCursor == 0)
		{
			ClearPieces();
		}
	}

	private void shake()
	{
		AkSoundEngine.PostEvent("UI_InGame_ShakePartyBox", base.gameObject);
	}

	private void openFlaps()
	{
		AkSoundEngine.PostEvent("UI_InGame_OpenPartyBox", base.gameObject);
		GameEventManager.SendEvent(new PartyBoxEvent(opened: true));
		PartyPickCursor[] array = cursors;
		foreach (PartyPickCursor partyPickCursor in array)
		{
			if (partyPickCursor != null)
			{
				partyPickCursor.transform.localScale = new Vector3(2f, 2f, 2f);
				partyPickCursor.transform.rotation = Quaternion.identity;
				if (partyPickCursor.hasAuthority)
				{
					partyPickCursor.Enable();
				}
			}
		}
	}

	private void closeFlaps()
	{
		AkSoundEngine.PostEvent("UI_InGame_ClosePartyBox", base.gameObject);
		GameEventManager.SendEvent(new PartyBoxEvent(opened: false));
	}

	private void ToPlayMode()
	{
		Hide();
	}

	private void ToPlaceMode()
	{
		remainingPickingCursor = cursors.Length;
	}

	public void boxFlapOrder(bool topBoxFlaps)
	{
		foreach (SpriteRenderer boxFlapSprite in boxFlapSprites)
		{
			if (topBoxFlaps)
			{
				boxFlapSprite.sortingLayerName = "Background 1";
			}
			else
			{
				boxFlapSprite.sortingLayerName = "Background 2";
			}
		}
		if (topBoxFlaps)
		{
			boxTopL.sortingOrder = 6;
			BoxTopR.sortingOrder = 6;
		}
		else
		{
			boxTopL.sortingOrder = 4;
			BoxTopR.sortingOrder = 4;
		}
	}

	public void boxFlapFront()
	{
		boxFlapOrder(topBoxFlaps: true);
	}

	public void boxFlapBack()
	{
		boxFlapOrder(topBoxFlaps: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent) && GameSettings.GetInstance().partyBoxMode == PartyBoxMode.Standard)
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				ToPlaceMode();
			}
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				ToPlayMode();
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				paused = true;
				Pause();
			}
			else
			{
				paused = false;
				Unpause();
			}
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
				Pause();
			}
			else
			{
				scoreboard = false;
				Unpause();
			}
		}
		if (type == typeof(PickBlockEvent) && HasAuthority && GameSettings.GetInstance().partyBoxMode == PartyBoxMode.Standard)
		{
			remainingPickingCursor--;
			if (remainingPickingCursor <= 0 && HasAuthority)
			{
				MsgPartyBoxOpen msgPartyBoxOpen = new MsgPartyBoxOpen();
				msgPartyBoxOpen.IsOpen = false;
				NetworkServer.SendToAll(NetMsgTypes.PartyBoxOpen, msgPartyBoxOpen);
				Hide();
			}
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PiecePicked)
			{
				OnPiecePicked(networkMessageReceivedEvent.ReadMessage as MsgPiecePicked);
			}
			else if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PartyBoxOpen)
			{
				OnBoxOpen(networkMessageReceivedEvent.ReadMessage as MsgPartyBoxOpen);
			}
		}
		if (type == typeof(GamePlayerRemovedEvent))
		{
			GamePlayerRemovedEvent gamePlayerRemovedEvent = e as GamePlayerRemovedEvent;
			if (cursors.Length != 0)
			{
				PartyPickCursor[] array = new PartyPickCursor[cursors.Length - 1];
				int num = 0;
				for (int i = 0; i != cursors.Length; i++)
				{
					PartyPickCursor partyPickCursor = cursors[i];
					if (partyPickCursor.networkNumber == gamePlayerRemovedEvent.PlayerNetworkNumber)
					{
						if (partyPickCursor.Picking || partyPickCursor.Pick == null)
						{
							remainingPickingCursor--;
						}
						if (partyPickCursor.Enabled)
						{
							partyPickCursor.Disable();
						}
						UnityEngine.Object.Destroy(partyPickCursor.gameObject);
					}
					else if (num == array.Length)
					{
						Debug.Log("No cursor with network number " + gamePlayerRemovedEvent.PlayerNetworkNumber + "?");
					}
					else
					{
						array[num] = partyPickCursor;
						num++;
					}
				}
				if (num < array.Length)
				{
					Debug.Log("Multiple cursors with network number " + gamePlayerRemovedEvent.PlayerNetworkNumber + "?");
				}
				cursors = array;
				if (remainingPickingCursor <= 0 && HasAuthority)
				{
					MsgPartyBoxOpen msgPartyBoxOpen2 = new MsgPartyBoxOpen();
					msgPartyBoxOpen2.IsOpen = false;
					NetworkServer.SendToAll(NetMsgTypes.PartyBoxOpen, msgPartyBoxOpen2);
					Hide();
				}
			}
		}
		if (type == typeof(TwitchItemVoteEvent))
		{
			TwitchItemVoteEvent twitchItemVoteEvent = e as TwitchItemVoteEvent;
			PickableBlock pickableByIndex = LobbyManager.instance.CurrentGameController.MetaList.GetPickableByIndex(twitchItemVoteEvent.pickableID);
			if (partybox_debug)
			{
				Debug.Log("(Twitch) Setting twitch-selected item: " + pickableByIndex.name);
			}
			twitchSelectedItems.Add(pickableByIndex);
		}
		if (!(type == typeof(ControllerConnectionEvent)))
		{
			return;
		}
		ControllerConnectionEvent controllerConnectionEvent = e as ControllerConnectionEvent;
		if (!controllerConnectionEvent.Connected)
		{
			return;
		}
		PartyPickCursor[] array2 = cursors;
		foreach (PartyPickCursor partyPickCursor2 in array2)
		{
			if (partyPickCursor2.LocalPlayer == controllerConnectionEvent.Player)
			{
				controllerConnectionEvent.Player.UseController.AddReceiver(partyPickCursor2);
			}
		}
	}

	public void OnPiecePicked(MsgPiecePicked pickMsg)
	{
		GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(pickMsg.PickableNetID));
		if (gameObject != null)
		{
			PickableBlock component = gameObject.GetComponent<PickableBlock>();
			if (component != null)
			{
				component.Disable();
			}
			GameEventManager.SendEvent(new PickBlockEvent(pickMsg.PlayerNumber, component));
		}
		else
		{
			GameEventManager.SendEvent(new PickBlockEvent(pickMsg.PlayerNumber, null));
		}
	}

	public void OnBoxOpen(MsgPartyBoxOpen boxOpenMsg)
	{
		if (!HasAuthority)
		{
			if (boxOpenMsg.IsOpen)
			{
				ShowBox(boxOpenMsg.isExtraBox);
			}
			else
			{
				Hide();
			}
		}
	}

	public override void Update()
	{
		if (hideCounter > 0f)
		{
			hideCounter -= Time.unscaledDeltaTime;
		}
	}
}
