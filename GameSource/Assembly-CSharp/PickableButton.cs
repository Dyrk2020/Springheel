using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class PickableButton : MonoBehaviour, IGameEventListener, IPickable
{
	public Animator animator;

	public Collider2D[] PickColliders;

	public List<Cursor> HoveredCursors = new List<Cursor>();

	public SortOrder spriteSortOrder;

	public InventoryBook inventoryBook;

	public int pageNumber;

	protected Vector3 initialScale;

	protected PickCursor lastCursor;

	public string HoverSoundEvent;

	public string ClickSoundEvent;

	protected bool Visible;

	protected bool paused;

	protected bool deactivatedInBook;

	public Text buttonText;

	public Image image;

	public Image[] additionalImages;

	public SpriteRenderer sprite;

	public List<Outline> outlines = new List<Outline>();

	protected Color outlineHighlight;

	protected Color currentOutlineColor;

	public bool outlineHighlightOverride;

	public Color outlineHighlightOverrideColor;

	public float outlineHighlightSizer = 10f;

	public float outlineHighlightSpeedModifier = 1f;

	public bool overrideDeactivatedAlphaBool;

	public float overrideDeactivatedAlphafloat = 0.5f;

	public float hoveredScaleModifier = 1f;

	public Transform scaleTarget;

	public Transform tipContainer;

	private HashSet<Image> tipImages;

	private HashSet<Text> tipText;

	[HideInInspector]
	public bool tipShown;

	public static HashSet<PickableButton> allowedButtons = new HashSet<PickableButton>();

	public static bool maskAll = false;

	public static bool[] maskingLayerStates = new bool[8] { true, true, true, true, true, true, true, true };

	public int maskingLayer;

	public Action<bool> onShowTip;

	protected bool initialized;

	public static GameSettings gameSettings;

	public List<Cursor> IHoveredCursors
	{
		get
		{
			return HoveredCursors;
		}
		set
		{
		}
	}

	public int PageNumber
	{
		get
		{
			return pageNumber;
		}
		set
		{
			pageNumber = value;
		}
	}

	public PickableBlock ThisPickableBlock => null;

	public SortOrder SpriteSortOrder => spriteSortOrder;

	public string Name => base.name;

	public uint netIdValue => 0u;

	public string SFXEventName => HoverSoundEvent;

	public InventoryBook InventoryBook
	{
		get
		{
			return inventoryBook;
		}
		set
		{
			inventoryBook = value;
		}
	}

	public bool Paused => paused;

	public bool DeactivatedInBook
	{
		get
		{
			return deactivatedInBook;
		}
		set
		{
			deactivatedInBook = value;
		}
	}

	public static void ResetMasks()
	{
		maskAll = false;
		allowedButtons.Clear();
	}

	public static bool IsButtonAllowed(PickableButton button)
	{
		if (!maskAll && (allowedButtons.Count == 0 || allowedButtons.Contains(button)))
		{
			return maskingLayerStates[button.maskingLayer];
		}
		return false;
	}

	public bool IsButtonAllowed()
	{
		return IsButtonAllowed(this);
	}

	public static void AllowOnlyButtons(params PickableButton[] buttons)
	{
		ResetMasks();
		foreach (PickableButton item in buttons)
		{
			allowedButtons.Add(item);
		}
	}

	public static void SetMaskingLayerState(int layerId, bool enabled)
	{
		if (layerId < 0 || layerId > maskingLayerStates.Length)
		{
			Debug.LogError("ERROR: " + layerId + " is not a valid layer ID");
		}
		else
		{
			maskingLayerStates[layerId] = enabled;
		}
	}

	public static void ResetMaskingLayerStates()
	{
		for (int i = 0; i < maskingLayerStates.Length; i++)
		{
			maskingLayerStates[i] = true;
		}
	}

	protected virtual void Awake()
	{
		spriteSortOrder = new SortOrder(base.gameObject);
		if (scaleTarget == null)
		{
			scaleTarget = base.transform;
		}
		initialScale = scaleTarget.localScale;
		ChangeListener(adding: true);
		GameObject obj = base.gameObject;
		if (!outlineHighlightOverride)
		{
			outlineHighlight = GameSettings.GetInstance().pickableButtonDefaultHoverColor;
		}
		else
		{
			outlineHighlight = outlineHighlightOverrideColor;
		}
		Outline outline = obj.AddComponent<Outline>();
		outline.effectDistance = new Vector2(outlineHighlightSizer, outlineHighlightSizer);
		outline.effectColor = new Color(0f, 0f, 0f, 0f);
		outlines.Add(outline);
		if (tipContainer != null)
		{
			tipShown = true;
			tipContainer.gameObject.SetActive(value: true);
			tipImages = new HashSet<Image>(tipContainer.gameObject.GetComponentsInChildren<Image>());
			tipText = new HashSet<Text>(tipContainer.gameObject.GetComponentsInChildren<Text>());
			ShowTip(show: false);
		}
	}

	protected virtual void Start()
	{
		Transform parent = base.gameObject.transform.parent;
		while (inventoryBook == null && parent != null)
		{
			inventoryBook = parent.gameObject.GetComponent<InventoryBook>();
			parent = parent.parent;
		}
		initialized = true;
		Enable(Visible);
	}

	public virtual void setInitialScale(float newScale)
	{
		scaleTarget.localScale = Vector3.one * newScale;
		initialScale = scaleTarget.localScale;
	}

	protected virtual void Update()
	{
		if (!Visible || !initialized)
		{
			return;
		}
		float maxDistanceDelta = gameSettings.hoverScaledSpeed * Time.deltaTime * initialScale.x * outlineHighlightSpeedModifier;
		float maxDistanceDelta2 = gameSettings.hoverHighlightSpeed * Time.deltaTime * outlineHighlightSpeedModifier;
		if (HoveredCursors.Count > 0)
		{
			scaleTarget.localScale = Vector3.MoveTowards(scaleTarget.localScale, initialScale * (1f + (gameSettings.hoverScaledAmount - 1f) * hoveredScaleModifier), maxDistanceDelta);
			SetOutlineColor(Vector4.MoveTowards(currentOutlineColor, outlineHighlight, maxDistanceDelta2));
		}
		else
		{
			if (scaleTarget.localScale.x != initialScale.x)
			{
				scaleTarget.localScale = Vector3.MoveTowards(scaleTarget.localScale, initialScale, maxDistanceDelta);
			}
			SetOutlineColor(Vector4.MoveTowards(currentOutlineColor, new Vector4(outlineHighlight.r, outlineHighlight.g, outlineHighlight.b, 0f), maxDistanceDelta2));
		}
		if (tipContainer != null)
		{
			ShowTip(HoveredCursors.Count > 0);
		}
	}

	public void SetOutlineColor(Color newColor)
	{
		if (!(currentOutlineColor != newColor))
		{
			return;
		}
		currentOutlineColor = newColor;
		foreach (Outline outline in outlines)
		{
			outline.effectColor = newColor;
		}
	}

	public void ResetScale()
	{
		scaleTarget.localScale = initialScale;
		SetOutlineColor(Color.clear);
	}

	public void Enable()
	{
		Enable(onOff: true);
	}

	public virtual void Enable(bool onOff)
	{
		Collider2D[] pickColliders = PickColliders;
		foreach (Collider2D collider2D in pickColliders)
		{
			if (collider2D != null)
			{
				if (collider2D.enabled != onOff)
				{
					collider2D.enabled = onOff;
				}
			}
			else
			{
				Debug.LogWarning("Warning: PickableButton \"" + base.name + "\" has a null PickCollider", this);
			}
		}
		Visible = onOff;
		bool visible = onOff;
		if (onOff)
		{
			Update();
		}
		Visible = visible;
		if (buttonText != null && buttonText.enabled != onOff)
		{
			buttonText.enabled = onOff;
		}
		if (image != null && image.enabled != onOff)
		{
			image.enabled = onOff;
		}
		if (sprite != null && sprite.enabled != onOff)
		{
			sprite.enabled = onOff;
		}
		if (onOff)
		{
			Update();
		}
	}

	public virtual void Disable()
	{
		Enable(onOff: false);
	}

	public virtual void PlayHoverSound()
	{
		if (HoverSoundEvent != "")
		{
			AkSoundEngine.PostEvent(HoverSoundEvent, base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("UI_Notebook_DefaultHoverButton", base.gameObject);
		}
	}

	public virtual void OnAccept(PickCursor pickCursor)
	{
		lastCursor = pickCursor;
		if (ClickSoundEvent != "")
		{
			AkSoundEngine.PostEvent(ClickSoundEvent, base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("UI_Notebook_DefaultClickButton", base.gameObject);
		}
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
	}

	public virtual void SetAlpha(float newAlpha)
	{
		if ((bool)buttonText)
		{
			buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, newAlpha);
		}
		if ((bool)image)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
		}
		if ((bool)sprite)
		{
			sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
		}
	}

	public virtual void SetTextCanvasOrder(int num)
	{
	}

	public bool CursorInsideMaskedArea(Vector3 cursorWorldPos)
	{
		RectMask2D componentInParent = GetComponentInParent<RectMask2D>();
		if (componentInParent != null)
		{
			return IsPointInRectTransform(cursorWorldPos, componentInParent.rectTransform);
		}
		return true;
	}

	public static bool IsPointInRectTransform(Vector3 worldPos, RectTransform maskRect)
	{
		Vector3[] array = new Vector3[4];
		maskRect.GetWorldCorners(array);
		Vector2 vector = new Vector2(array[0].x, array[0].y);
		Vector2 vector2 = new Vector2(array[2].x, array[2].y);
		if (worldPos.x >= vector.x && worldPos.x < vector2.x && worldPos.y >= vector.y && worldPos.y < vector2.y)
		{
			return true;
		}
		return false;
	}

	public void ShowTip(bool show)
	{
		if (tipShown == show)
		{
			return;
		}
		tipShown = show;
		if (tipImages != null)
		{
			foreach (Image tipImage in tipImages)
			{
				if (tipImage != null && tipImage.enabled != show)
				{
					tipImage.enabled = show;
				}
			}
		}
		if (tipText != null)
		{
			foreach (Text item in tipText)
			{
				if (item != null && item.enabled != show)
				{
					item.enabled = show;
				}
			}
		}
		if (onShowTip != null)
		{
			onShowTip(show);
		}
	}

	public void AddTipElement(Text text)
	{
		tipText.Add(text);
	}

	public void AddTipElement(Image image)
	{
		tipImages.Add(image);
	}

	public void RemoveTipElement(Text text)
	{
		tipText.Remove(text);
	}

	public void RemoveTipElement(Image image)
	{
		tipImages.Remove(image);
	}

	public void ResetTipElements()
	{
		HashSet<Image> hashSet = new HashSet<Image>();
		foreach (Image tipImage in tipImages)
		{
			if (tipImage != null)
			{
				hashSet.Add(tipImage);
			}
		}
		tipImages = hashSet;
		HashSet<Text> hashSet2 = new HashSet<Text>();
		foreach (Text item in tipText)
		{
			if (item != null)
			{
				hashSet2.Add(item);
			}
		}
		tipText = hashSet2;
	}
}
