using UnityEngine;

public static class Util_SpriteRenderer
{
	public static void SetAlpha(this SpriteRenderer self, float alpha)
	{
		if (self.color.a != alpha)
		{
			Color color = self.color;
			color.a = alpha;
			self.color = color;
		}
	}
}
