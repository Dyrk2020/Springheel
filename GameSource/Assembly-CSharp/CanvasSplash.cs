using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSplash : UISplashScreen
{
	public Image SolidImage;

	public List<Image> Images = new List<Image>();

	public List<Text> Texts = new List<Text>();

	public Color StartColor = Color.black;

	public Color FadeColor = new Color(0f, 0f, 0f, 0f);

	public CanvasGroup CanvasGroup;

	protected virtual void Awake()
	{
		if (Images == null)
		{
			Images = new List<Image>();
		}
		if (!Images.Contains(SolidImage))
		{
			Images.Add(SolidImage);
		}
	}

	public override void Setup()
	{
	}

	public override void Show()
	{
		base.Show();
		if (SkipBool)
		{
			return;
		}
		foreach (Image image in Images)
		{
			Color color = image.color;
			color.a = StartColor.a;
			image.color = color;
		}
		foreach (Text text in Texts)
		{
			Color color2 = text.color;
			color2.a = StartColor.a;
			text.color = color2;
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = StartColor.a;
		}
	}

	public override void Hide()
	{
		base.Hide();
		foreach (Image image in Images)
		{
			Color color = image.color;
			color.a = FadeColor.a;
			image.color = color;
		}
		foreach (Text text in Texts)
		{
			Color color2 = text.color;
			color2.a = FadeColor.a;
			text.color = color2;
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = FadeColor.a;
		}
	}

	public override void Fade(float alpha)
	{
		base.Fade(alpha);
		if (SkipBool)
		{
			return;
		}
		foreach (Image image in Images)
		{
			Color color = image.color;
			color.a = Mathf.Lerp(FadeColor.a, StartColor.a, alpha);
			image.color = color;
		}
		foreach (Text text in Texts)
		{
			Color color2 = text.color;
			color2.a = Mathf.Lerp(FadeColor.a, StartColor.a, alpha);
			text.color = color2;
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = alpha;
		}
	}
}
