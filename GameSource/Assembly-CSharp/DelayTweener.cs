using UnityEngine;

public class DelayTweener : TimedTweener
{
	private float delayAmount;

	private float delayTimer;

	private Tweener tween;

	public DelayTweener(float delayAmount, Tweener tween)
	{
		this.delayAmount = delayAmount;
		this.tween = tween;
	}

	public override void Prime()
	{
		tween.Prime();
	}

	public override bool IsDone()
	{
		if (delayTimer >= delayAmount)
		{
			return tween.IsDone();
		}
		return false;
	}

	public override void Update()
	{
		if (delayTimer <= delayAmount)
		{
			if (useUnscaledDeltaTime)
			{
				delayTimer += Time.unscaledDeltaTime;
			}
			else
			{
				delayTimer += Time.deltaTime;
			}
		}
		else
		{
			tween.Update();
		}
	}
}
