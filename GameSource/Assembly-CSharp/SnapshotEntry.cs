using System;
using System.Collections;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SnapshotEntry : MonoBehaviour
{
	public InputField inputField;

	public Text levelNameText;

	public Text codeText;

	public Image backgroundImage;

	public Image faveStar;

	public Image faveStarBack;

	public RawImage thumbnailImage;

	public SpriteRenderer spinnyLoadingThing;

	public Image publicMark;

	public Image hasModsMark;

	public Text reportCount;

	public Text infoLine1;

	public UGCNameTag nameTag1;

	public Text infoLine2;

	public UGCNameTag nameTag2;

	public CanvasGroup canvasGroup;

	public Text tipLevelNameText;

	public Transform tipNameTagLine;

	public UGCNameTag tipNameTag;

	public Transform tipInfoLineContainer;

	public UnityEngine.Object tipTextInfoLinePrefab;

	public bool tipInitialized;

	public UndergroundComputer.FeaturedLevelData featuredLevelData;

	public int indexOnCurrentPage = -1;

	private string internalName;

	private string code;

	private bool local;

	public FeaturedQuickFilter.LevelTypes levelType;

	private IEnumerator fadeCoroutine;

	private string waitingForThumbnailHash;

	public string Code => code;

	public bool Local => local;

	public string SnapshotName
	{
		get
		{
			if (levelNameText != null)
			{
				return levelNameText.text;
			}
			return internalName;
		}
	}

	public string UncensoredName => internalName;

	private void Awake()
	{
		if (inputField != null)
		{
			inputField.enabled = false;
			inputField.textComponent.enabled = false;
		}
		if (thumbnailImage != null)
		{
			thumbnailImage.enabled = false;
		}
		if (spinnyLoadingThing != null)
		{
			spinnyLoadingThing.enabled = false;
		}
	}

	public void SetInfoInternal(string code, string internalName, bool local)
	{
		this.code = code;
		this.internalName = internalName;
		this.local = local;
	}

	public void SetCode(string code, bool local)
	{
		this.code = code;
		this.local = local;
		if (code.NullOrEmpty())
		{
			if (codeText != null)
			{
				if (local)
				{
					codeText.text = ScriptLocalization.Snapshot.LocalSaveIndicator;
				}
				else
				{
					codeText.text = "";
				}
			}
			if (thumbnailImage != null && !featuredLevelData.archived)
			{
				spinnyLoadingThing.enabled = true;
				LevelThumbnailCache.Instance.LoadLocalSaveThumbnail(SnapshotName, GetOnThumbnailFound(featuredLevelData.name));
			}
		}
		else
		{
			if (codeText != null)
			{
				codeText.text = GameSparksQuery.GetFormattedSnapshotCode(code);
			}
			if (thumbnailImage != null && !featuredLevelData.archived)
			{
				spinnyLoadingThing.enabled = true;
				LevelThumbnailCache.Instance.LoadThumbnailFromCloud(code, GetOnThumbnailFound(code + featuredLevelData.name));
			}
		}
		if (reportCount != null)
		{
			if (GameSparksManager.Instance.MainUserIsAdmin && featuredLevelData != null && featuredLevelData.numReports > 0)
			{
				reportCount.text = featuredLevelData.numReports.ToString();
			}
			else
			{
				reportCount.gameObject.SetActive(value: false);
			}
		}
	}

	private UnityAction<Texture2D> GetOnThumbnailFound(string hash)
	{
		waitingForThumbnailHash = hash;
		return delegate(Texture2D tex)
		{
			if (!(hash != waitingForThumbnailHash) && this != null)
			{
				if (thumbnailImage != null)
				{
					if (tex != null)
					{
						LevelThumbnailCache.Instance.AddTextureUser(tex, this);
						thumbnailImage.texture = tex;
						thumbnailImage.enabled = true;
						if (!base.gameObject.activeInHierarchy)
						{
							thumbnailImage.color = Color.white;
							canvasGroup.alpha = 1f;
						}
					}
					else
					{
						thumbnailImage.texture = null;
						thumbnailImage.enabled = false;
					}
				}
				spinnyLoadingThing.enabled = false;
				if (base.gameObject.activeInHierarchy)
				{
					fadeCoroutine = FadeInImage(0.25f);
					fadeCoroutine.MoveNext();
				}
				else
				{
					canvasGroup.alpha = 1f;
				}
			}
		};
	}

	public void MarkFileMissing()
	{
		Color red = Color.red;
		red.a = 0.5f;
		Color color = red;
		color.a = 0.2f;
		levelNameText.color = red;
		codeText.color = red;
		codeText.text = "!!!";
		if (faveStar != null)
		{
			faveStar.color = red;
		}
		if (inputField != null)
		{
			inputField.textComponent.color = red;
		}
		if (backgroundImage != null)
		{
			backgroundImage.color = color;
		}
	}

	private IEnumerator FadeInImage(float time)
	{
		canvasGroup.alpha = 0f;
		for (float timer = 0f; timer < time; timer += Time.unscaledDeltaTime)
		{
			float alpha = timer / time;
			canvasGroup.alpha = alpha;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	private void Update()
	{
		if (fadeCoroutine == null)
		{
			return;
		}
		if (base.gameObject.activeInHierarchy)
		{
			if (!fadeCoroutine.MoveNext())
			{
				fadeCoroutine = null;
			}
		}
		else
		{
			fadeCoroutine = null;
			canvasGroup.alpha = 1f;
		}
	}

	public void Initialize(UndergroundComputer.FeaturedLevelData data, bool showPublishedMark)
	{
		featuredLevelData = data;
		SetInfoInternal(data.code, data.name, data.isLocal);
		levelNameText.text = data.name;
		if (inputField != null)
		{
			inputField.text = data.name;
			inputField.interactable = false;
			inputField.enabled = false;
			inputField.textComponent.enabled = false;
		}
		SetCode(code, local: false);
		bool flag = StatTracker.Instance.GetSaveFileDataForMainUser().IsFavorite(data.name, data.code);
		if (faveStar != null)
		{
			faveStar.enabled = flag;
			faveStarBack.enabled = flag;
		}
		if (publicMark != null)
		{
			publicMark.enabled = showPublishedMark;
		}
		if (hasModsMark != null)
		{
			hasModsMark.enabled = data.hasMods;
		}
		levelType = data.levelType;
		UndergroundComputer undergroundComputer = PickableBuildButton.undergroundComputer;
		if (undergroundComputer != null && undergroundComputer.currentFilter != null)
		{
			PopulateInfoLine(infoLine1, nameTag1, undergroundComputer.currentFilter.infoLine1);
			PopulateInfoLine(infoLine2, nameTag2, undergroundComputer.currentFilter.infoLine2);
		}
		else
		{
			Debug.LogError("Couldn't locate underground computer or underground filter!");
			PopulateInfoLine(infoLine1, nameTag1, FeaturedQuickFilter.InfoLineTypes.None);
			PopulateInfoLine(infoLine2, nameTag2, FeaturedQuickFilter.InfoLineTypes.None);
		}
	}

	private void PopulateInfoLine(Text infoLine, UGCNameTag nameTag, FeaturedQuickFilter.InfoLineTypes infoLineType)
	{
		infoLine.gameObject.SetActive(value: false);
		nameTag.gameObject.SetActive(value: false);
		switch (infoLineType)
		{
		case FeaturedQuickFilter.InfoLineTypes.Author:
			nameTag.gameObject.SetActive(value: true);
			if (featuredLevelData.isLocal)
			{
				nameTag.Clear();
			}
			else
			{
				nameTag.InitializeAsync(featuredLevelData);
			}
			break;
		case FeaturedQuickFilter.InfoLineTypes.TimeSinceCreation:
			if (featuredLevelData.timestamp > 0)
			{
				infoLine.gameObject.SetActive(value: true);
				long num = (UndergroundComputer.lastRefreshTimestamp - featuredLevelData.timestamp) / 1000;
				infoLine.text = UndergroundComputer.TimeToString((int)num);
			}
			break;
		case FeaturedQuickFilter.InfoLineTypes.Difficulty:
			infoLine.gameObject.SetActive(value: true);
			infoLine.text = featuredLevelData.DifficultyString;
			break;
		case FeaturedQuickFilter.InfoLineTypes.LevelType:
			switch (featuredLevelData.levelType)
			{
			case FeaturedQuickFilter.LevelTypes.Challenge:
				infoLine.gameObject.SetActive(value: true);
				infoLine.text = LocalizationManager.GetTranslation("InLobby/ChallengeModeButtonText");
				break;
			case FeaturedQuickFilter.LevelTypes.Versus:
				infoLine.gameObject.SetActive(value: true);
				infoLine.text = LocalizationManager.GetTranslation("InLobby/PartyText");
				break;
			}
			break;
		case FeaturedQuickFilter.InfoLineTypes.PlayCount:
			infoLine.gameObject.SetActive(value: true);
			infoLine.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Played") + " " + featuredLevelData.playCount + " " + ((featuredLevelData.playCount == 1) ? LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTime") : LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTimes"));
			break;
		case FeaturedQuickFilter.InfoLineTypes.Points:
			infoLine.gameObject.SetActive(value: true);
			if (featuredLevelData.rating != 1)
			{
				infoLine.text = string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/LevelPointsSingular"), featuredLevelData.rating);
			}
			else
			{
				infoLine.text = string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/LevelPointsPlural"), featuredLevelData.rating);
			}
			break;
		case FeaturedQuickFilter.InfoLineTypes.None:
			break;
		}
	}

	public void OnShowTip(bool show)
	{
		if (show && !tipInitialized)
		{
			tipInitialized = true;
			tipLevelNameText.text = featuredLevelData.name;
			switch (featuredLevelData.levelType)
			{
			case FeaturedQuickFilter.LevelTypes.Challenge:
				AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Type"), LocalizationManager.GetTranslation("InLobby/ChallengeModeButtonText"));
				break;
			case FeaturedQuickFilter.LevelTypes.Versus:
				AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Type"), LocalizationManager.GetTranslation("InLobby/PartyText"));
				break;
			}
			if (featuredLevelData.isLocal)
			{
				tipNameTag.Clear();
			}
			else
			{
				tipNameTag.InitializeAsync(featuredLevelData);
				PickableBuildButton component = GetComponent<PickableBuildButton>();
				component.AddTipElement(tipNameTag.usernameText);
				component.AddTipElement(tipNameTag.PSNVerifiedIcon);
				component.AddTipElement(tipNameTag.UCHNetIcon);
				if (featuredLevelData.attempts > 0)
				{
					string[] array = SplitAtColon(string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/CompletedBy"), featuredLevelData.CompletionPercentage));
					if (array != null)
					{
						AddTipInfoLine(array[0], array[1]);
					}
					string[] array2 = SplitAtColon(string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Attempts"), featuredLevelData.attempts));
					if (array2 != null)
					{
						AddTipInfoLine(array2[0], array2[1]);
					}
					string[] array3 = SplitAtColon(string.Format(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Successes"), featuredLevelData.successes, featuredLevelData.SuccessFailurePercentage));
					if (array3 != null)
					{
						AddTipInfoLine(array3[0], array3[1]);
					}
				}
				else
				{
					AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/CompletedNoData"), "");
				}
				string value = featuredLevelData.playCount + " " + ((featuredLevelData.playCount == 1) ? LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTime") : LocalizationManager.GetTranslation("UndergroundComputer/Stats/PlayedTimes"));
				AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Played"), value);
				if (!featuredLevelData.isPublished)
				{
					AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Unpublished"), "");
				}
				else if (featuredLevelData.timestamp > 0)
				{
					long num = UndergroundComputer.lastRefreshTimestamp - featuredLevelData.timestamp;
					AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Published"), UndergroundComputer.TimeToString((int)(num / 1000)));
				}
				else
				{
					AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Published"), "???");
				}
				AddTipInfoLine(LocalizationManager.GetTranslation("UndergroundComputer/Stats/RatingHeading"), featuredLevelData.rating.ToString());
			}
		}
		bool flag = show && !featuredLevelData.isLocal;
		if (tipNameTagLine.gameObject.activeSelf != flag)
		{
			tipNameTagLine.gameObject.SetActive(flag);
		}
	}

	private void AddTipInfoLine(string key, string value)
	{
		TipInfoLine tipInfoLine = tipInfoLineContainer.gameObject.AddPrefabAsChild<TipInfoLine>(tipTextInfoLinePrefab);
		tipInfoLine.keyText.text = key;
		tipInfoLine.valueText.text = value;
		PickableBuildButton component = GetComponent<PickableBuildButton>();
		if (component != null)
		{
			component.AddTipElement(tipInfoLine.keyText);
			component.AddTipElement(tipInfoLine.valueText);
		}
	}

	private string[] SplitAtColon(string input)
	{
		int num = input.IndexOf(":", StringComparison.InvariantCulture);
		if (num != -1)
		{
			return new string[2]
			{
				input.Substring(0, num + 1),
				input.Substring(num + 1, input.Length - num - 1)
			};
		}
		num = input.IndexOf("：", StringComparison.InvariantCulture);
		if (num != -1)
		{
			return new string[2]
			{
				input.Substring(0, num + 1),
				input.Substring(num + 1, input.Length - num - 1)
			};
		}
		return null;
	}

	public void ClearTip()
	{
		tipInitialized = false;
		tipInfoLineContainer.DestroyAllChildren();
		PickableButton component = GetComponent<PickableButton>();
		if (component.tipShown)
		{
			component.ShowTip(show: false);
		}
		component.RemoveTipElement(tipNameTag.usernameText);
		component.RemoveTipElement(tipNameTag.PSNVerifiedIcon);
		component.RemoveTipElement(tipNameTag.UCHNetIcon);
		component.ResetTipElements();
		component.ShowTip(show: false);
	}
}
