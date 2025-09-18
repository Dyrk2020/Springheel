using UnityEngine;

public class CanvasGroupAlphaTweener : TimedTweener
{
	public CanvasGroup target;

	public float startAlpha;

	public float endAlpha;

	public Easings.Functions easingFunc;

	public CanvasGroupAlphaTweener(CanvasGroup target, float startAlpha, float endAlpha, float duration, Easings.Functions easingFunc = Easings.Functions.Linear)
	{
		this.target = target;
		this.startAlpha = startAlpha;
		this.endAlpha = endAlpha;
		base.duration = duration;
		this.easingFunc = easingFunc;
	}

	public override void Prime()
	{
		target.alpha = startAlpha;
	}

	public override void Update()
	{
		base.Update();
		if (!IsDone())
		{
			float t = Easings.Interpolate(time / duration, easingFunc);
			target.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, t);
		}
		else
		{
			target.alpha = endAlpha;
		}
	}
}
