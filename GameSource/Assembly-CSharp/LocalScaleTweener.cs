using UnityEngine;

public class LocalScaleTweener : TimedTweener
{
	public Transform target;

	public Vector3 startScale;

	public Vector3 endScale;

	public Easings.Functions easingFunc;

	public LocalScaleTweener(Transform target, Vector3 startScale, Vector3 endScale, float duration, Easings.Functions easingFunc = Easings.Functions.Linear)
	{
		this.target = target;
		this.startScale = startScale;
		this.endScale = endScale;
		base.duration = duration;
		this.easingFunc = easingFunc;
	}

	public override void Prime()
	{
		target.localScale = startScale;
	}

	public override void Update()
	{
		base.Update();
		if (!IsDone())
		{
			float t = Easings.Interpolate(time / duration, easingFunc);
			target.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
		}
		else
		{
			target.localScale = endScale;
		}
	}
}
