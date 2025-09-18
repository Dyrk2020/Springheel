using System.Collections.Generic;

public class MultiTweener : Tweener
{
	private int tweenersDone;

	public List<Tweener> tweeners = new List<Tweener>();

	public MultiTweener Add(Tweener tweener)
	{
		tweeners.Add(tweener);
		return this;
	}

	public override void Prime()
	{
		foreach (Tweener tweener in tweeners)
		{
			tweener.Prime();
		}
	}

	public override bool IsDone()
	{
		return tweenersDone == tweeners.Count;
	}

	public override void Update()
	{
		foreach (Tweener tweener in tweeners)
		{
			if (!tweener.IsDone())
			{
				tweener.Update();
				if (tweener.IsDone())
				{
					tweenersDone++;
				}
			}
		}
	}
}
