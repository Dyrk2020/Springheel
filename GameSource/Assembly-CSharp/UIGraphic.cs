using UnityEngine;
using UnityEngine.UI;

public class UIGraphic : UIElement
{
	public Bounds BoundingBox;

	private SpriteRenderer[] srs = new SpriteRenderer[0];

	private Image[] imgs = new Image[0];

	private Text[] txts = new Text[0];

	private MultiControllerButton[] controllerButtons = new MultiControllerButton[0];

	public CanvasGroup canvasGroup;

	public float canvasFadeTime = 1f;

	public bool Visible { get; protected set; }

	protected virtual void Awake()
	{
		srs = GetComponentsInChildren<SpriteRenderer>();
		imgs = GetComponentsInChildren<Image>();
		txts = GetComponentsInChildren<Text>();
		controllerButtons = GetComponentsInChildren<MultiControllerButton>();
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
		}
	}

	public virtual void Update()
	{
		if (canvasGroup != null)
		{
			float num = ((!Visible) ? 0f : 1f);
			if (!Mathf.Approximately(num, canvasGroup.alpha))
			{
				canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, num, Time.unscaledDeltaTime / canvasFadeTime);
			}
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		if (!canvasGroup || forceQuickHide)
		{
			SpriteRenderer[] array = srs;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (spriteRenderer != null)
				{
					spriteRenderer.enabled = false;
				}
			}
			Image[] array2 = imgs;
			foreach (Image image in array2)
			{
				if (image != null)
				{
					image.enabled = false;
				}
			}
			Text[] array3 = txts;
			foreach (Text text in array3)
			{
				if (text != null)
				{
					text.enabled = false;
				}
			}
		}
		Visible = false;
	}

	public override void Show()
	{
		SpriteRenderer[] array = srs;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.enabled = true;
			}
		}
		Image[] array2 = imgs;
		foreach (Image image in array2)
		{
			if (image != null)
			{
				image.enabled = true;
			}
		}
		Text[] array3 = txts;
		foreach (Text text in array3)
		{
			if (text != null)
			{
				text.enabled = true;
			}
		}
		MultiControllerButton[] array4 = controllerButtons;
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].MarkDirty();
		}
		Visible = true;
	}
}
