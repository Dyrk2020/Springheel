using UnityEngine;

[ExecuteInEditMode]
public class snapper : MonoBehaviour
{
	public bool snapOn = true;

	public int mult = 1;

	public void Awake()
	{
		if (Application.isPlaying && base.enabled)
		{
			base.enabled = false;
		}
	}

	public void Update()
	{
		if (snapOn)
		{
			float num = 1f / (float)mult;
			Vector3 localScale = base.transform.localScale;
			if (localScale.x < num)
			{
				localScale.x = num;
			}
			if (localScale.y < num)
			{
				localScale.y = num;
			}
			Bounds bounds = new Bounds(base.transform.position, localScale);
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			float num2 = 1f / (float)(mult * 2);
			Vector3 vector = new Vector3(num2, num2, 0f);
			if (mult % 2 == 1)
			{
				min -= vector;
				max -= vector;
			}
			min *= (float)mult;
			max *= (float)mult;
			min = new Vector3(Mathf.Round(min.x), Mathf.Round(min.y), 0f);
			max = new Vector3(Mathf.Round(max.x), Mathf.Round(max.y), 0f);
			min /= (float)mult;
			max /= (float)mult;
			if (mult % 2 == 1)
			{
				min += vector;
				max += vector;
			}
			Vector3 localScale2 = max - min;
			localScale2.x = Mathf.Max(num, localScale2.x);
			localScale2.y = Mathf.Max(num, localScale2.y);
			base.transform.position = (min + max) / 2f;
			base.transform.localScale = localScale2;
		}
	}
}
