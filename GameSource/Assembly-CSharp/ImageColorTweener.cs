using UnityEngine;
using UnityEngine.UI;

public class ImageColorTweener : TimedTweener
{
	public Image target;

	public Color startColor;

	public Color endColor;

	public Easings.Functions easingFunc;

	public ImageColorTweener(Image target, Color startColor, Color endColor, float duration, Easings.Functions easingFunc = Easings.Functions.Linear)
	{
		this.target = target;
		this.startColor = startColor;
		this.endColor = endColor;
		base.duration = duration;
		this.easingFunc = easingFunc;
	}

	public override void Prime()
	{
		target.color = startColor;
	}

	public override void Update()
	{
		base.Update();
		if (!IsDone())
		{
			float t = Easings.Interpolate(time / duration, easingFunc);
			target.color = Color.Lerp(startColor, endColor, t);
		}
		else
		{
			target.color = endColor;
		}
	}
}
