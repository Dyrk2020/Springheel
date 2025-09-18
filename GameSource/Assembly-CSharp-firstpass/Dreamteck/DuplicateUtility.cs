using UnityEngine;

namespace Dreamteck;

public static class DuplicateUtility
{
	public static AnimationCurve DuplicateCurve(AnimationCurve input)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		animationCurve.postWrapMode = input.postWrapMode;
		animationCurve.preWrapMode = input.preWrapMode;
		for (int i = 0; i < input.keys.Length; i++)
		{
			animationCurve.AddKey(input.keys[i]);
		}
		return animationCurve;
	}

	public static Gradient DuplicateGradient(Gradient input)
	{
		return null;
	}
}
