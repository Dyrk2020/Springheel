using System;
using UnityEngine;

public class FadePulse : MonoBehaviour
{
	public float Period;

	public float Min;

	public float Max = 1f;

	private float time;

	private void Start()
	{
		Min = Mathf.Abs(Min);
		Max = Mathf.Abs(Max);
		if (Min > Max)
		{
			float min = Min;
			Min = Max;
			Max = min;
		}
		if (Max > 1f)
		{
			Max = 1f;
		}
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (GetComponent<Renderer>() != null && time > 0f)
		{
			float a = (Mathf.Sin(time / Period * MathF.PI * 2f) / 2f + 0.5f) * (Max - Min) + Min;
			Color color = new Color(1f, 1f, 1f, a);
			GetComponent<Renderer>().material.color = color;
		}
	}
}
