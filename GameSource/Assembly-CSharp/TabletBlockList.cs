using System;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletBlockList : MonoBehaviour, IGameEventListener
{
	[Serializable]
	public class ColumnInfo
	{
		public bool visible;

		public TabletBlock[] tabletBlocks;
	}

	public UnityEngine.Object blockProbabilityControlPrefab;

	public UnityEngine.Object blockPagePrefab;

	public int gridWidth = 15;

	public int gridHeight = 4;

	public int gridSpacing = 25;

	public int gridSquareW = 300;

	public int gridSquareH = 300;

	public RectTransform buttonOverlay;

	public RectTransform buttonOverlayContainer;

	public RectTransform blockSettingsDialog;

	public RectMask2D buttonOverlayMask;

	public TabletButton prevButton;

	public TabletButton nextButton;

	public TabletTextLabel pageLabel;

	public TabletButton hideDisabledButton;

	public TabletTextLabel hideDisabledButtonText;

	public float scrollSpeed = 1000f;

	public TabletBlock[] tabletBlocks;

	public TabletBlock[] tabletBlocksByIndex;

	private float pageWidth;

	private int currentPage;

	private bool scrolling;

	public bool showingAdvancedProbabilities;

	public bool hidingDisabledBlocks;

	public List<ColumnInfo> columnInfo;

	private bool forceRefreshColumns;

	public Color[] probabilityBarColors;

	public Sprite[] fillSprites;

	public int CurrentPage => currentPage;

	public int NumPages
	{
		get
		{
			int num = (hidingDisabledBlocks ? CountDisplayedBlocks() : tabletBlocks.Length);
			if (num == 0)
			{
				return 0;
			}
			return Mathf.CeilToInt((float)Mathf.CeilToInt((float)num / (float)gridHeight) / 4f);
		}
	}

	private void Awake()
	{
		ChangeListener(onOff: true);
		if (columnInfo == null)
		{
			Debug.LogWarning("Generating column info for tablet block list - should not happen at runtime");
			GenerateColumnInfo();
		}
	}

	public void GenerateColumnInfo()
	{
		int num = Mathf.CeilToInt((float)(hidingDisabledBlocks ? CountDisplayedBlocks() : tabletBlocks.Length) / (float)gridHeight);
		this.columnInfo = new List<ColumnInfo>(num);
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			ColumnInfo columnInfo = new ColumnInfo
			{
				visible = true,
				tabletBlocks = new TabletBlock[gridHeight]
			};
			for (int j = 0; j < gridHeight; j++)
			{
				num2 = GetNextDisplayedBlock(num2);
				if (num2 == -1)
				{
					break;
				}
				columnInfo.tabletBlocks[j] = tabletBlocks[num2];
			}
			if (columnInfo.tabletBlocks[0] != null)
			{
				this.columnInfo.Add(columnInfo);
			}
		}
		forceRefreshColumns = true;
	}

	private int CountDisplayedBlocks()
	{
		int num = 0;
		TabletBlock[] array = tabletBlocks;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].displayedInList)
			{
				num++;
			}
		}
		return num;
	}

	private int GetNextDisplayedBlock(int curIdx)
	{
		int i = curIdx + 1;
		if (i >= tabletBlocks.Length || i < 0)
		{
			return -1;
		}
		for (; i < tabletBlocks.Length && !tabletBlocks[i].displayedInList; i++)
		{
		}
		if (i >= tabletBlocks.Length)
		{
			return -1;
		}
		return i;
	}

	public void Start()
	{
	}

	public void Initialize(bool isDisabled)
	{
		pageWidth = 4 * (gridSquareW + gridSpacing);
		buttonOverlay.localPosition = Vector3.zero;
		TabletBlock[] array = tabletBlocks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initialize();
		}
		tabletBlocksByIndex = new TabletBlock[GameSettings.GetInstance().DefaultRuleset.Blocks.Length];
		for (int j = 0; j < tabletBlocks.Length; j++)
		{
			TabletBlock tabletBlock = tabletBlocks[j];
			tabletBlock.disabled = isDisabled;
			int blockSerializeIndex = tabletBlock.pickableBlockPrefab.blockSerializeIndex;
			if (blockSerializeIndex >= 0 && blockSerializeIndex < tabletBlocksByIndex.Length)
			{
				tabletBlocksByIndex[blockSerializeIndex] = tabletBlock;
			}
		}
		RefreshPageNumber();
	}

	private void OnDestroy()
	{
		ChangeListener(onOff: false);
	}

	private void ChangeListener(bool onOff)
	{
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, onOff);
	}

	public void PassRuleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetBlockFrequency)
		{
			MsgSetBlockFrequency msgSetBlockFrequency = (MsgSetBlockFrequency)networkMessageReceivedEvent.ReadMessage;
			if (msgSetBlockFrequency.blockIndex < tabletBlocksByIndex.Length)
			{
				tabletBlocksByIndex[msgSetBlockFrequency.blockIndex].SetProbability(msgSetBlockFrequency.frequency, sendNetwork: false);
				GameSettings.GetInstance().SetBlockFrequency(msgSetBlockFrequency.blockIndex, msgSetBlockFrequency.frequency);
				if (showingAdvancedProbabilities)
				{
					RefreshAdvancedProbabilities();
				}
				if (hidingDisabledBlocks)
				{
					RefreshHiddenBlocks();
				}
			}
			else
			{
				Debug.LogError("Received SetBlockFrequency for non-existent block index");
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetAllBlockFrequencies)
		{
			MsgSetAllBlockFrequencies msgSetAllBlockFrequencies = (MsgSetAllBlockFrequencies)networkMessageReceivedEvent.ReadMessage;
			TabletBlock[] array = tabletBlocks;
			foreach (TabletBlock tabletBlock in array)
			{
				if (msgSetAllBlockFrequencies.frequency == -1)
				{
					int stepValueFromRarity = GetStepValueFromRarity(tabletBlock.pickableBlockPrefab.placeablePrefab.BaseRarity);
					tabletBlock.SetProbability(stepValueFromRarity, sendNetwork: false);
				}
				else
				{
					tabletBlock.SetProbability(msgSetAllBlockFrequencies.frequency, sendNetwork: false);
				}
			}
			int num = GameSettings.GetInstance().DefaultRuleset.Blocks.Length;
			for (int j = 0; j < num; j++)
			{
				if (msgSetAllBlockFrequencies.frequency == -1)
				{
					int stepValueFromRarity2 = GetStepValueFromRarity(PlaceableMetadataList.Instance.allBlockPrefabs[j].GetComponent<Placeable>().BaseRarity);
					GameSettings.GetInstance().SetBlockFrequency(j, stepValueFromRarity2);
				}
				else
				{
					GameSettings.GetInstance().SetBlockFrequency(j, msgSetAllBlockFrequencies.frequency);
				}
			}
			if (showingAdvancedProbabilities)
			{
				RefreshAdvancedProbabilities();
			}
			if (hidingDisabledBlocks)
			{
				RefreshHiddenBlocks();
			}
		}
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.SendAllBlockFrequencies)
		{
			return;
		}
		MsgSendAllBlockFrequencies msgSendAllBlockFrequencies = (MsgSendAllBlockFrequencies)networkMessageReceivedEvent.ReadMessage;
		for (int k = 0; k < msgSendAllBlockFrequencies.frequencies.Length; k++)
		{
			TabletBlock tabletBlock2 = tabletBlocksByIndex[k];
			if (tabletBlock2 != null)
			{
				tabletBlock2.SetProbability(msgSendAllBlockFrequencies.frequencies[k], sendNetwork: false);
			}
			GameSettings.GetInstance().SetBlockFrequency(k, msgSendAllBlockFrequencies.frequencies[k]);
		}
		if (showingAdvancedProbabilities)
		{
			RefreshAdvancedProbabilities();
		}
		if (hidingDisabledBlocks)
		{
			RefreshHiddenBlocks();
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(LanguageChangeEvent))
		{
			UpdateHideDisabledButton();
		}
	}

	public void LateUpdate()
	{
		if (scrolling)
		{
			float num = (float)(-currentPage) * pageWidth;
			RectTransform rectTransform = (RectTransform)base.transform.GetChild(0);
			float x = rectTransform.anchoredPosition.x;
			if (Mathf.Abs(x - num) < 2f)
			{
				x = num;
			}
			else
			{
				float num2 = scrollSpeed * Time.deltaTime;
				if (num < x)
				{
					num2 = 0f - num2;
				}
				x += num2;
				if ((num2 > 0f && x > num) || (num2 < 0f && x < num))
				{
					x = num;
				}
			}
			rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
			if (x == num)
			{
				scrolling = false;
			}
			RefreshPageNumber();
		}
		if (base.gameObject.activeInHierarchy)
		{
			RectTransform component = base.transform.GetChild(0).GetComponent<RectTransform>();
			buttonOverlayContainer.position = component.position;
			buttonOverlayMask.rectTransform.position = blockSettingsDialog.position;
			UpdateColumnVisibility();
		}
	}

	public void OnClickNextPage(PickCursor pickCursor)
	{
		currentPage = Mathf.Min(currentPage + 1, NumPages - 1);
		scrolling = true;
		RefreshPageNumber();
		AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Page_Next", base.gameObject);
	}

	public void OnClickPreviousPage(PickCursor pickCursor)
	{
		currentPage = Mathf.Max(currentPage - 1, 0);
		scrolling = true;
		RefreshPageNumber();
		AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Page_Previous", base.gameObject);
	}

	private void RefreshPageNumber()
	{
		int numPages = NumPages;
		if (numPages == 0)
		{
			pageLabel.text = "";
			nextButton.gameObject.SetActive(value: false);
			prevButton.gameObject.SetActive(value: false);
			return;
		}
		if (scrolling)
		{
			int num = Mathf.RoundToInt(Mathf.Abs(((RectTransform)base.transform.GetChild(0)).anchoredPosition.x / pageWidth) - 0.5f);
			pageLabel.text = num + 1 + "/" + numPages;
		}
		else
		{
			pageLabel.text = currentPage + 1 + "/" + numPages;
		}
		nextButton.gameObject.SetActive(value: true);
		prevButton.gameObject.SetActive(value: true);
		bool flag = currentPage == 0 || numPages == 0;
		if (flag != prevButton.Disabled)
		{
			prevButton.SetDisabled(flag);
		}
		bool flag2 = currentPage == numPages - 1 || numPages == 0;
		if (flag2 != nextButton.Disabled)
		{
			nextButton.SetDisabled(flag2);
		}
	}

	public void OnClickFilterAll(PickCursor pickCursor)
	{
		if (!GameSettings.GetInstance().DefaultRuleset.IsCurrentlyApplied(checkRules: false, checkPoints: false, checkBlocks: true, checkMods: false))
		{
			MsgSetAllBlockFrequencies msgSetAllBlockFrequencies = new MsgSetAllBlockFrequencies();
			msgSetAllBlockFrequencies.frequency = -1;
			LobbyManager.instance.client.Send(NetMsgTypes.SetAllBlockFrequencies, msgSetAllBlockFrequencies);
			GetComponentInParent<TabletRulesScreen>().MarkRulesDirty();
		}
	}

	public void OnClickFilterNone(PickCursor pickCursor)
	{
		MsgSetAllBlockFrequencies msgSetAllBlockFrequencies = new MsgSetAllBlockFrequencies();
		msgSetAllBlockFrequencies.frequency = 0;
		LobbyManager.instance.client.Send(NetMsgTypes.SetAllBlockFrequencies, msgSetAllBlockFrequencies);
		GetComponentInParent<TabletRulesScreen>().MarkRulesDirty();
	}

	public void OnClickRandomize(PickCursor pickCursor)
	{
		GetComponentInParent<TabletRulesScreen>().MarkRulesDirty();
		MsgSendAllBlockFrequencies msgSendAllBlockFrequencies = new MsgSendAllBlockFrequencies();
		msgSendAllBlockFrequencies.frequencies = GetAllBlockFrequencies();
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			int num = UnityEngine.Random.Range(0, 10);
			tabletBlock.SetProbability(num, sendNetwork: false);
			msgSendAllBlockFrequencies.frequencies[tabletBlock.pickableBlockPrefab.blockSerializeIndex] = num;
		}
		NetworkServer.SendToAll(NetMsgTypes.SendAllBlockFrequencies, msgSendAllBlockFrequencies);
		if (hidingDisabledBlocks)
		{
			RefreshHiddenBlocks();
		}
	}

	public void OnClickEverythingPlus(PickCursor pickCursor)
	{
		GetComponentInParent<TabletRulesScreen>().MarkRulesDirty();
		MsgSendAllBlockFrequencies msgSendAllBlockFrequencies = new MsgSendAllBlockFrequencies();
		msgSendAllBlockFrequencies.frequencies = GetAllBlockFrequencies();
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			int num = Mathf.Min(9, tabletBlock.currentProbStep + 1);
			tabletBlock.SetProbability(num, sendNetwork: false);
			msgSendAllBlockFrequencies.frequencies[tabletBlock.pickableBlockPrefab.blockSerializeIndex] = num;
		}
		NetworkServer.SendToAll(NetMsgTypes.SendAllBlockFrequencies, msgSendAllBlockFrequencies);
		if (hidingDisabledBlocks)
		{
			RefreshHiddenBlocks();
		}
	}

	public void OnClickEverythingMinus(PickCursor pickCursor)
	{
		GetComponentInParent<TabletRulesScreen>().MarkRulesDirty();
		MsgSendAllBlockFrequencies msgSendAllBlockFrequencies = new MsgSendAllBlockFrequencies();
		msgSendAllBlockFrequencies.frequencies = GetAllBlockFrequencies();
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			int num = Mathf.Max(0, tabletBlock.currentProbStep - 1);
			tabletBlock.SetProbability(num, sendNetwork: false);
			msgSendAllBlockFrequencies.frequencies[tabletBlock.pickableBlockPrefab.blockSerializeIndex] = num;
		}
		NetworkServer.SendToAll(NetMsgTypes.SendAllBlockFrequencies, msgSendAllBlockFrequencies);
		if (hidingDisabledBlocks)
		{
			RefreshHiddenBlocks();
		}
	}

	public void OnItemFilterRefreshed()
	{
		TabletBlock[] array = tabletBlocks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnItemFilterRefreshed();
		}
		if (hidingDisabledBlocks)
		{
			RefreshHiddenBlocks();
		}
	}

	public void ShowAdvancedProbabilities(bool show)
	{
		showingAdvancedProbabilities = show;
		if (show)
		{
			RefreshAdvancedProbabilities();
		}
		TabletBlock[] array = tabletBlocks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].advancedPercentText.gameObject.SetActive(show);
		}
	}

	public void RefreshAdvancedProbabilities()
	{
		Dictionary<TabletBlock, int> dictionary = new Dictionary<TabletBlock, int>();
		int num = 0;
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			int weightFromStepValue = GetWeightFromStepValue(tabletBlock.currentProbStep);
			dictionary.Add(tabletBlock, weightFromStepValue);
			num += weightFromStepValue;
		}
		if (num != 0)
		{
			foreach (KeyValuePair<TabletBlock, int> item in dictionary)
			{
				if (item.Value != 0)
				{
					float num2 = (float)item.Value / (float)num * 100f;
					item.Key.advancedPercentText.text = num2.ToString("F1") + "%";
				}
				else
				{
					item.Key.advancedPercentText.text = "";
				}
			}
			return;
		}
		foreach (KeyValuePair<TabletBlock, int> item2 in dictionary)
		{
			item2.Key.advancedPercentText.text = "";
		}
	}

	private void UpdateColumnVisibility()
	{
		float num = 0f - ((RectTransform)base.transform.GetChild(0)).anchoredPosition.x + (float)gridSpacing;
		float x = ((RectTransform)base.transform).sizeDelta.x;
		float num2 = num + x;
		float num3 = gridSpacing + gridSquareW;
		int num4 = Mathf.FloorToInt(num / num3);
		int num5 = Mathf.FloorToInt(num2 / num3);
		num4--;
		num5++;
		num4 = Mathf.Clamp(num4, 0, columnInfo.Count);
		num5 = Mathf.Clamp(num5, 0, columnInfo.Count);
		for (int i = 0; i < columnInfo.Count; i++)
		{
			if (i >= num4 && i <= num5)
			{
				if (!forceRefreshColumns && columnInfo[i].visible)
				{
					continue;
				}
				columnInfo[i].visible = true;
				TabletBlock[] array = columnInfo[i].tabletBlocks;
				foreach (TabletBlock tabletBlock in array)
				{
					if (tabletBlock != null)
					{
						tabletBlock.gameObject.SetActive(value: true);
						tabletBlock.overlays.gameObject.SetActive(value: true);
					}
				}
			}
			else
			{
				if (!forceRefreshColumns && !columnInfo[i].visible)
				{
					continue;
				}
				columnInfo[i].visible = false;
				TabletBlock[] array = columnInfo[i].tabletBlocks;
				foreach (TabletBlock tabletBlock2 in array)
				{
					if (tabletBlock2 != null)
					{
						tabletBlock2.gameObject.SetActive(value: false);
						tabletBlock2.overlays.gameObject.SetActive(value: false);
					}
				}
			}
		}
		forceRefreshColumns = false;
	}

	public void HideDisabledBlocks()
	{
		hidingDisabledBlocks = true;
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			if (tabletBlock.currentProbStep == 0)
			{
				tabletBlock.displayedInList = false;
				tabletBlock.gameObject.SetActive(value: false);
				tabletBlock.overlays.gameObject.SetActive(value: false);
			}
		}
	}

	public void RevealAllBlocks()
	{
		hidingDisabledBlocks = false;
		TabletBlock[] array = tabletBlocks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].displayedInList = true;
		}
	}

	public void OnClickToggleHideDisabled(PickCursor pickCursor)
	{
		if (hidingDisabledBlocks)
		{
			RevealAllBlocks();
		}
		else
		{
			HideDisabledBlocks();
		}
		UpdateHideDisabledButton();
		ReorderList();
	}

	private void UpdateHideDisabledButton()
	{
		if (hidingDisabledBlocks)
		{
			hideDisabledButtonText.text = LocalizationManager.GetTranslation("RuleBook/Presets/ShowDisabled");
			hideDisabledButton.clickSound = "UI_UPad_PointsSettings_Disabled_Show";
		}
		else
		{
			hideDisabledButtonText.text = LocalizationManager.GetTranslation("RuleBook/Presets/HideDisabled");
			hideDisabledButton.clickSound = "UI_UPad_PointsSettings_Disabled_Hide";
		}
	}

	public void ReorderList()
	{
		int num = gridSquareW + gridSpacing;
		int num2 = gridSquareH + gridSpacing;
		int num3 = 0;
		TabletBlock[] array = tabletBlocks;
		foreach (TabletBlock tabletBlock in array)
		{
			if (tabletBlock.displayedInList)
			{
				int num4 = num3 / gridHeight * num + gridSpacing;
				int num5 = num3 % gridHeight * -num2 - gridSpacing;
				tabletBlock.transform.localPosition = new Vector3(num4, num5);
				tabletBlock.overlays.localPosition = new Vector3(num4, num5);
				num3++;
			}
		}
		GenerateColumnInfo();
		UpdateColumnVisibility();
		currentPage = Mathf.Clamp(currentPage, 0, NumPages - 1);
		scrolling = true;
		RefreshPageNumber();
	}

	private void RefreshHiddenBlocks()
	{
		RevealAllBlocks();
		HideDisabledBlocks();
		ReorderList();
	}

	public static int GetWeightFromStepValue(int probStepValue)
	{
		return probStepValue switch
		{
			0 => 0, 
			1 => 100, 
			2 => 200, 
			3 => 500, 
			4 => 700, 
			6 => 1500, 
			7 => 2000, 
			8 => 3000, 
			9 => 5000, 
			_ => 1000, 
		};
	}

	public static int GetStepValueFromRarity(Placeable.Rarity rarity)
	{
		return rarity switch
		{
			Placeable.Rarity.NeverPick => 0, 
			Placeable.Rarity.Rare => 2, 
			Placeable.Rarity.Common => 7, 
			_ => 5, 
		};
	}

	private int[] GetAllBlockFrequencies()
	{
		int[] array = new int[tabletBlocksByIndex.Length];
		for (int i = 0; i < tabletBlocksByIndex.Length; i++)
		{
			if (tabletBlocksByIndex[i] != null)
			{
				array[i] = tabletBlocksByIndex[i].currentProbStep;
			}
		}
		return array;
	}
}
