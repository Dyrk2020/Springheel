using System;
using UnityEngine;

[Serializable]
public class HSVColorData
{
	public Color tint = Color.white;

	[Range(-1f, 1f)]
	public float hue;

	[Range(-1f, 1f)]
	public float saturation;

	[Range(-1f, 1f)]
	public float value;

	[Range(-1f, 1f)]
	public float contrast = 1f;
}
