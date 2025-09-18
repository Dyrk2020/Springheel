using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletBlock : MonoBehaviour
{
	public PickableBlock pickableBlockPrefab;

	public RectTransform overlays;

	public Transform spriteHolder;

	public RectTransform clickAreaRect;

	public Image backgroundImage;

	public TabletButton resetButton;

	public TabletTextLabel advancedPercentText;

	public CanvasGroup overlayButtonCanvasGroup;

	public Image fill;

	public Image fillCurrent;

	public float fillAlpha = 0.36f;

	public Color negativeColor;

	public Color defaultColor;

	public Animator animator;

	public SpriteRenderer crossOut;

	public bool playOnce;

	private bool animating;

	private bool hovered;

	public SpriteRenderer[] ArtSprites;

	private Color[] initialColors;

	private bool wasHovered;

	private const float scaleTime = 0.1f;

	private float scaleAlpha;

	private int initialStepValue = 5;

	public int currentProbStep = 5;

	public string clickSound;

	public string hoverSound;

	private IEnumerator anim;

	public static int buttonSections = 10;

	public bool displayedInList = true;

	public TabletBlockList tabletBlockList;

	public bool disabled;

	public RectTransform defaultLine;

	public Outline notDefaultOutline;

	public float animateTime;

	private float animateTimer;

	private bool shouldSendFrequencyMessage;

	private float lastFrequencyMessage;

	private const float frequencyMessageDelay = 0.3f;

	public void Initialize()
	{
		advancedPercentText.gameObject.SetActive(value: false);
		overlayButtonCanvasGroup.alpha = 0f;
		crossOut.gameObject.SetActive(value: false);
		resetButton.gameObject.SetActive(value: false);
		pickableBlockPrefab.EnsureSerializeIndex();
		Placeable.Rarity baseRarity = pickableBlockPrefab.placeablePrefab.BaseRarity;
		initialStepValue = TabletBlockList.GetStepValueFromRarity(baseRarity);
		defaultLine.anchoredPosition = new Vector2(0f, ((float)initialStepValue + 1f) / 10f * 300f);
		currentProbStep = GameSettings.GetInstance().GetBlockFrequency(pickableBlockPrefab.blockSerializeIndex);
		notDefaultOutline.enabled = currentProbStep != initialStepValue;
		UpdateCrossoutAndResetAndFill();
		if (pickableBlockPrefab.noneDefaultColors)
		{
			initialColors = new Color[ArtSprites.Length];
			for (int i = 0; i < ArtSprites.Length; i++)
			{
				initialColors[i] = ArtSprites[i].color;
			}
		}
	}

	public void OnItemFilterRefreshed()
	{
		int blockFrequency = GameSettings.GetInstance().GetBlockFrequency(pickableBlockPrefab.blockSerializeIndex);
		if (currentProbStep != blockFrequency)
		{
			currentProbStep = blockFrequency;
			UpdateCrossoutAndResetAndFill();
		}
	}

	public void setFillColours(Color color)
	{
		fill.color = new Color(color.r, color.g, color.b, fillAlpha);
		fillCurrent.color = new Color(color.r, color.g, color.b, fillAlpha);
	}

	public void InitializeSprites(PickableBlock pickableBlockPrefab, Material unlitSpriteMat)
	{
		this.pickableBlockPrefab = pickableBlockPrefab;
		Tablet componentInParent = GetComponentInParent<Tablet>();
		for (int num = spriteHolder.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(spriteHolder.GetChild(num).gameObject);
		}
		PickableBlock pickableBlock = Object.Instantiate(pickableBlockPrefab);
		GameObject gameObject = pickableBlock.gameObject;
		if (pickableBlock.twitchLogo != null)
		{
			Object.DestroyImmediate(pickableBlock.twitchLogo.gameObject);
		}
		Collider2D[] pickColliders = pickableBlock.PickColliders;
		foreach (Collider2D collider2D in pickColliders)
		{
			if (collider2D != null)
			{
				Object.DestroyImmediate(collider2D.gameObject);
			}
		}
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.SetParent(spriteHolder, worldPositionStays: false);
		new SortOrder(gameObject).setSortOrder(componentInParent.GetComponentInChildren<Canvas>().sortingOrder + 100);
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = componentInParent.PickableBlockSpriteMaterial;
		}
		ParticleSystemRenderer[] componentsInChildren2 = GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer obj in componentsInChildren2)
		{
			obj.sharedMaterial = pickableBlockPrefab.TabletParticleMaterial;
			obj.sortingOrder = componentInParent.GetComponentInChildren<Canvas>().sortingOrder + 100;
		}
		animator = pickableBlock.animator;
		crossOut = pickableBlock.crossOut;
		playOnce = pickableBlock.playOnce;
		ArtSprites = pickableBlock.ArtSprites;
		Object.DestroyImmediate(pickableBlock);
		Object.DestroyImmediate(gameObject.GetComponent<NetworkIdentity>());
		spriteHolder.localScale = new Vector3(100f, 100f, 100f) * pickableBlockPrefab.BlockProbabilityScale;
		spriteHolder.localPosition = pickableBlockPrefab.BlockProbabilityOffset * 100f;
	}

	public void OnCursorOver()
	{
		hovered = true;
	}

	public void OnCursorOut()
	{
		hovered = false;
		if ((bool)animator && animator.isInitialized)
		{
			animator.SetBool("Keep Active", value: false);
			animating = false;
			animateTimer = 0f;
		}
	}

	public void OnClickPickableBlock(PickCursor pickCursor)
	{
		if (LobbyManager.instance.IsHost)
		{
			if (!clickSound.NullOrEmpty())
			{
				AkSoundEngine.PostEvent(clickSound, base.gameObject);
			}
			if (currentProbStep != currentProbStep)
			{
				OnProbStepChanged(sendNetwork: true);
			}
		}
	}

	public void ApplyScheme(TabletColorScheme colorScheme)
	{
		TabletStyledObject[] componentsInChildren = GetComponentsInChildren<TabletStyledObject>();
		foreach (TabletStyledObject obj in componentsInChildren)
		{
			obj.colorScheme = colorScheme;
			obj.ResetStyles();
		}
	}

	public void OnClickReset(PickCursor pickCursor)
	{
		if (LobbyManager.instance.IsHost)
		{
			int num = currentProbStep;
			Placeable.Rarity baseRarity = pickableBlockPrefab.placeablePrefab.BaseRarity;
			currentProbStep = TabletBlockList.GetStepValueFromRarity(baseRarity);
			if (currentProbStep != num)
			{
				UpdateCrossoutAndResetAndFill();
				OnProbStepChanged(sendNetwork: true);
			}
		}
	}

	private void OnProbStepChanged(bool sendNetwork)
	{
		GameSettings.GetInstance().SetBlockFrequency(pickableBlockPrefab.blockSerializeIndex, currentProbStep);
		if (sendNetwork)
		{
			shouldSendFrequencyMessage = true;
			TabletRulesScreen componentInParent = GetComponentInParent<TabletRulesScreen>();
			componentInParent.MarkRulesDirty();
			componentInParent.RefreshAdvancedProbabilities();
		}
	}

	private void SendFrequencyMessage()
	{
		MsgSetBlockFrequency msgSetBlockFrequency = new MsgSetBlockFrequency();
		msgSetBlockFrequency.blockIndex = pickableBlockPrefab.blockSerializeIndex;
		msgSetBlockFrequency.frequency = currentProbStep;
		LobbyManager.instance.client.Send(NetMsgTypes.SetBlockFrequency, msgSetBlockFrequency);
	}

	public void SetProbability(int probStep, bool sendNetwork = true)
	{
		if (currentProbStep != probStep)
		{
			currentProbStep = probStep;
			UpdateCrossoutAndResetAndFill();
			OnProbStepChanged(sendNetwork);
		}
	}

	public void UpdateCrossoutAndResetAndFill()
	{
		notDefaultOutline.enabled = currentProbStep != initialStepValue;
		if (currentProbStep == 0)
		{
			crossOut.gameObject.SetActive(value: true);
			SetCurrentFillAmount(1 / buttonSections);
			SetFillAmount(9);
			setFillColours(negativeColor);
		}
		else
		{
			crossOut.gameObject.SetActive(value: false);
			SetCurrentFillAmount(currentProbStep);
			SetFillAmount(currentProbStep);
			setFillColours(tabletBlockList.probabilityBarColors[currentProbStep]);
		}
		if (currentProbStep == initialStepValue)
		{
			if (resetButton.gameObject.activeSelf)
			{
				resetButton.SetDisabled(disabled: true);
				resetButton.gameObject.SetActive(value: false);
			}
		}
		else if (!resetButton.gameObject.activeSelf && LobbyManager.instance != null && LobbyManager.instance.IsHost)
		{
			resetButton.SetDisabled(disabled: false);
			resetButton.gameObject.SetActive(value: true);
		}
	}

	public void IndicateBlockOff()
	{
		SetFillAmount(9);
		SetCurrentFillAmount(0);
		setFillColours(negativeColor);
		crossOut.gameObject.SetActive(value: true);
	}

	public void IndicateBlockOn(int targetProb)
	{
		setFillColours(tabletBlockList.probabilityBarColors[targetProb]);
		crossOut.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		if (shouldSendFrequencyMessage)
		{
			lastFrequencyMessage += Time.unscaledDeltaTime;
			if (lastFrequencyMessage > 0.3f)
			{
				lastFrequencyMessage = 0f;
				shouldSendFrequencyMessage = false;
				SendFrequencyMessage();
			}
		}
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
		bool flag = false;
		if (hovered)
		{
			animateTimer += Time.deltaTime;
			if (animateTimer > animateTime && (bool)animator && animator.isInitialized)
			{
				if (!animating)
				{
					animator.SetTrigger("Active");
					animating = true;
				}
				if (!playOnce)
				{
					animator.SetBool("Keep Active", value: true);
				}
			}
		}
		bool flag2 = hovered || resetButton.HasTrackedCursors;
		if (flag2 != wasHovered)
		{
			wasHovered = flag2;
			animateTimer = 0f;
			if (flag2 && !hoverSound.NullOrEmpty())
			{
				AkSoundEngine.PostEvent(hoverSound, base.gameObject);
			}
		}
		if (flag2 && scaleAlpha != 1f)
		{
			scaleAlpha += Time.deltaTime / 0.1f;
			if (scaleAlpha >= 1f)
			{
				scaleAlpha = 1f;
			}
			flag = true;
		}
		if (!flag2 && scaleAlpha != 0f)
		{
			scaleAlpha -= Time.deltaTime / 0.1f;
			if (scaleAlpha <= 0f)
			{
				scaleAlpha = 0f;
			}
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		overlayButtonCanvasGroup.alpha = scaleAlpha;
		spriteHolder.localScale = Vector3.one * pickableBlockPrefab.BlockProbabilityScale * (100f + 10f * scaleAlpha);
		GameSettings instance = GameSettings.GetInstance();
		Color color = Color.Lerp(instance.neutralColor, instance.highlightColor, scaleAlpha);
		if (pickableBlockPrefab.noneDefaultColors)
		{
			for (int i = 0; i < ArtSprites.Length; i++)
			{
				color += initialColors[i] - instance.neutralColor;
				ArtSprites[i].color = new Color(color.r, color.g, color.b, ArtSprites[i].color.a);
			}
			return;
		}
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			spriteRenderer.color = new Color(color.r, color.g, color.b, spriteRenderer.color.a);
		}
	}

	public void SetFillAmount(int step)
	{
		if (step >= 0 && step < tabletBlockList.fillSprites.Length)
		{
			fill.sprite = tabletBlockList.fillSprites[step];
		}
	}

	public void SetCurrentFillAmount(int step)
	{
		if (step >= 0 && step < tabletBlockList.fillSprites.Length)
		{
			fillCurrent.sprite = tabletBlockList.fillSprites[step];
		}
	}

	public void SetClickSound(string eventName)
	{
		if (eventName.NullOrEmpty())
		{
			clickSound = "";
		}
		else
		{
			clickSound = "UI_Inventory_Select_" + eventName;
		}
	}

	public void SetHoverSound(string eventName)
	{
		if (eventName.NullOrEmpty())
		{
			hoverSound = "";
		}
		else
		{
			hoverSound = "UI_Inventory_ScrollOn_" + eventName;
		}
	}
}
