using UnityEngine;
using UnityEngine.UI;

public static class Util_RawImage
{
	public static void SetAlpha(this RawImage self, float value)
	{
		Color color = self.color;
		color.a = value;
		self.color = color;
	}
}
