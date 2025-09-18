using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ColoredThings
{
	public string description;

	public Color colorA;

	public Color colorB;

	public GameObject holder;

	private SpriteRenderer[] spriteRenderers;

	private Text[] texts;

	public bool On = true;

	public void InitilizeColoredThing()
	{
		if ((bool)holder)
		{
			spriteRenderers = holder.GetComponentsInChildren<SpriteRenderer>();
			texts = holder.GetComponentsInChildren<Text>();
			if (spriteRenderers != null || texts != null)
			{
				UpdateColorArray();
			}
		}
	}

	public void UpdateColorArray()
	{
		if (spriteRenderers.Length != 0)
		{
			SpriteRenderer[] array = spriteRenderers;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (spriteRenderer.GetComponent<ColorPicked>() != null)
				{
					if (spriteRenderer.GetComponent<ColorPicked>().colorPicked == ColorPick.A)
					{
						spriteRenderer.color = colorA;
					}
					else
					{
						spriteRenderer.color = colorB;
					}
				}
				else
				{
					spriteRenderer.color = colorA;
				}
			}
		}
		if (texts.Length == 0)
		{
			return;
		}
		Text[] array2 = texts;
		foreach (Text text in array2)
		{
			if (text.GetComponent<ColorPicked>() != null)
			{
				if (text.GetComponent<ColorPicked>().colorPicked == ColorPick.A)
				{
					text.color = colorA;
				}
				else
				{
					text.color = colorB;
				}
			}
			else
			{
				text.color = colorA;
			}
		}
	}

	public void UpdateArrayAlpha(float input)
	{
		if (spriteRenderers.Length != 0)
		{
			SpriteRenderer[] array = spriteRenderers;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.color = newAlpha(spriteRenderer.color, input);
			}
		}
		if (texts.Length != 0)
		{
			Text[] array2 = texts;
			foreach (Text text in array2)
			{
				text.color = newAlpha(text.color, input);
			}
		}
	}

	private Color newAlpha(Color inputColor, float alpha)
	{
		return new Color(inputColor.r, inputColor.g, inputColor.b, alpha);
	}
}
