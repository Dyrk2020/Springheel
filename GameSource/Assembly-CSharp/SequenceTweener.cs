using System.Collections;
using System.Collections.Generic;

public class SequenceTweener : Tweener
{
	private int currentTweenIdx;

	public List<Tweener> tweeners = new List<Tweener>();

	public SequenceTweener Add(Tweener tweener)
	{
		tweeners.Add(tweener);
		return this;
	}

	public override void Prime()
	{
		if (tweeners.Count > 0)
		{
			tweeners[0].Prime();
		}
	}

	public override bool IsDone()
	{
		return currentTweenIdx >= tweeners.Count;
	}

	public override IEnumerator Animate()
	{
		for (currentTweenIdx = 0; currentTweenIdx < tweeners.Count; currentTweenIdx++)
		{
			IEnumerator anim = tweeners[currentTweenIdx].Animate();
			while (anim.MoveNext())
			{
				yield return null;
			}
		}
		Finish();
	}

	public static SequenceTweener operator +(SequenceTweener self, Tweener otherTween)
	{
		self.Add(otherTween);
		return self;
	}
}
