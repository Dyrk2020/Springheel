using UnityEngine;

public class AnchoredPositionTweener : TimedTweener
{
	public RectTransform target;

	public Vector3 startPos;

	public Vector3 endPos;

	public Easings.Functions easingFunc;

	public AnchoredPositionTweener(RectTransform target, Vector3 startPos, Vector3 endPos, float duration, Easings.Functions easingFunc = Easings.Functions.Linear)
	{
		this.target = target;
		this.startPos = startPos;
		this.endPos = endPos;
		base.duration = duration;
		this.easingFunc = easingFunc;
	}

	public override void Prime()
	{
		target.anchoredPosition = startPos;
	}

	public override void Update()
	{
		base.Update();
		if (!IsDone())
		{
			float t = Easings.Interpolate(time / duration, easingFunc);
			target.anchoredPosition = Vector3.LerpUnclamped(startPos, endPos, t);
		}
		else
		{
			target.anchoredPosition = endPos;
		}
	}
}
