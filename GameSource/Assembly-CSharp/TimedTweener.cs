using UnityEngine;

public class TimedTweener : Tweener
{
	protected float duration;

	protected float time;

	public bool useUnscaledDeltaTime;

	public override bool IsDone()
	{
		return time >= duration;
	}

	public override void Update()
	{
		if (useUnscaledDeltaTime)
		{
			time += Time.unscaledDeltaTime;
		}
		else
		{
			time += Time.deltaTime;
		}
	}
}
