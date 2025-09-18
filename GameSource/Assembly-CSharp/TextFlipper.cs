using UnityEngine;

public class TextFlipper : MonoBehaviour
{
	private bool flippedScale;

	private void LateUpdate()
	{
		if (Modifiers.GetInstance().CameraFlippedOnSingleAxis)
		{
			if (!flippedScale)
			{
				ToggleScale();
			}
		}
		else if (flippedScale)
		{
			ToggleScale();
		}
	}

	private void ToggleScale()
	{
		flippedScale = !flippedScale;
		Vector3 localScale = base.transform.localScale;
		localScale.x = 0f - localScale.x;
		base.transform.localScale = localScale;
	}
}
