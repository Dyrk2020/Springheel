using UnityEngine;
using UnityEngine.UI;

public static class Util_Text
{
	public static int GetBestFitFontSize(this Text self, string text = null)
	{
		if (self.resizeTextForBestFit)
		{
			if (text == null)
			{
				text = self.text;
			}
			self.cachedTextGenerator.Populate(text, self.GetGenerationSettings(self.rectTransform.rect.size));
			return Mathf.FloorToInt((float)self.cachedTextGenerator.fontSizeUsedForBestFit / self.canvas.scaleFactor);
		}
		Debug.LogError("Error: Best fit is disabled on " + self.name + "!");
		return self.fontSize;
	}

	public static void SetAlpha(this Text self, float value)
	{
		Color color = self.color;
		color.a = value;
		self.color = color;
	}
}
