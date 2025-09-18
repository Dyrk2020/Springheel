using UnityEngine;

public class AspectRatioScaler : MonoBehaviour
{
	public float scale_5_4 = 0.75f;

	public float scale_16_9 = 1f;

	private void Start()
	{
	}

	private void Update()
	{
		float num = 1.25f;
		float num2 = 1.7777778f;
		float num3 = (float)Screen.width / (float)Screen.height;
		float t = Mathf.Min(1f, (num3 - num) / (num2 - num));
		float num4 = Mathf.LerpUnclamped(scale_5_4, scale_16_9, t);
		base.transform.localScale = new Vector3(num4, num4, num4);
	}
}
