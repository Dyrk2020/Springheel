using UnityEngine;

public class PreserveRelativeScale : MonoBehaviour
{
	public float relativeScale = 1f;

	private float lastScale = -1f;

	private void LateUpdate()
	{
		float num = relativeScale / Modifiers.GetInstance().CharacterRelativeScale;
		if (lastScale != num)
		{
			lastScale = num;
			Vector3 localScale = base.transform.localScale;
			localScale.x = Mathf.Sign(localScale.x) * num;
			localScale.y = num;
			base.transform.localScale = localScale;
		}
	}
}
