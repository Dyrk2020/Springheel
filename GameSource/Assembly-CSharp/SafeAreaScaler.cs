using UnityEngine;

public class SafeAreaScaler : MonoBehaviour
{
	protected float currentSafeAreaRatio = 1f;

	public static float editorSafeAreatRationTest => GameSettings.GetInstance().safeAreaScaleRatio;

	public static float SafeAreaRatio => 1f;

	public static float SafeAreaRatioForLerp => 1f;

	private void LateUpdate()
	{
		if (currentSafeAreaRatio != editorSafeAreatRationTest)
		{
			currentSafeAreaRatio = editorSafeAreatRationTest;
			base.transform.localScale = new Vector3(currentSafeAreaRatio, currentSafeAreaRatio, currentSafeAreaRatio);
		}
	}
}
