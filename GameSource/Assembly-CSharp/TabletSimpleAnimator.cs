using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TabletSimpleAnimator : MonoBehaviour
{
	public Image targetImage;

	public IEnumerator anim;

	private void Update()
	{
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
	}

	public void FadeColor(Color startColor, Color endColor, float duration, Easings.Functions easingFunc)
	{
		ImageColorTweener imageColorTweener = new ImageColorTweener(targetImage, startColor, endColor, duration, easingFunc);
		anim = imageColorTweener.PrimeAndAnimate();
	}
}
