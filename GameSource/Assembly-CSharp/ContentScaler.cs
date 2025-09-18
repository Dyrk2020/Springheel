using UnityEngine;

public class ContentScaler : MonoBehaviour
{
	public RectTransform contentRect;

	public bool test;

	private int lastWidth = -1;

	private void LateUpdate()
	{
		int num = Mathf.CeilToInt(contentRect.sizeDelta.x);
		if (num != lastWidth)
		{
			lastWidth = num;
			Recalc();
		}
	}

	private void Recalc()
	{
		float num = ((RectTransform)base.transform).sizeDelta.x / contentRect.sizeDelta.x;
		if (num > 1f)
		{
			num = 1f;
		}
		contentRect.localScale = new Vector3(num, num, num);
	}
}
