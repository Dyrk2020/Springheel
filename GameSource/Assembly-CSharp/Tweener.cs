using System.Collections;
using UnityEngine.Events;

public class Tweener
{
	private UnityAction onFinish;

	public virtual bool IsDone()
	{
		return false;
	}

	public virtual void Prime()
	{
	}

	public virtual void Update()
	{
	}

	public virtual IEnumerator Animate()
	{
		while (!IsDone())
		{
			Update();
			if (!IsDone())
			{
				yield return null;
			}
		}
		Finish();
	}

	public virtual IEnumerator PrimeAndAnimate()
	{
		Prime();
		return Animate();
	}

	public Tweener SetOnFinish(UnityAction onFinish)
	{
		this.onFinish = onFinish;
		return this;
	}

	public void Finish()
	{
		if (onFinish != null)
		{
			onFinish();
		}
	}
}
