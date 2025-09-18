using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PartyModeColor
{
	public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

	public Color initialColor;

	public Color targetColor;

	public Color currentColor;

	public List<GameObject> ColorGroupHolders = new List<GameObject>();

	public List<Material> sharedMaterials;

	public Color setInitialColor()
	{
		foreach (GameObject colorGroupHolder in ColorGroupHolders)
		{
			spriteRenderers.AddRange(colorGroupHolder.GetComponentsInChildren<SpriteRenderer>());
		}
		Dictionary<Material, Material> dictionary = new Dictionary<Material, Material>();
		sharedMaterials = new List<Material>();
		Color color = new Color(0.75f, 0.75f, 0.75f, 0f);
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (!(spriteRenderer == null))
			{
				if (!dictionary.ContainsKey(spriteRenderer.sharedMaterial))
				{
					Material material = new Material(spriteRenderer.sharedMaterial);
					dictionary.Add(spriteRenderer.sharedMaterial, material);
					sharedMaterials.Add(material);
					spriteRenderer.sharedMaterial = material;
				}
				else
				{
					spriteRenderer.sharedMaterial = dictionary[spriteRenderer.sharedMaterial];
				}
				if (spriteRenderer.sharedMaterial.name.Contains("Additive"))
				{
					spriteRenderer.color = color;
				}
				else
				{
					spriteRenderer.color = Color.white;
				}
			}
		}
		foreach (Material sharedMaterial in sharedMaterials)
		{
			sharedMaterial.SetFloat("_GradientTop", -190f);
			sharedMaterial.SetFloat("_GradientBottom", -200f);
		}
		if (spriteRenderers.Count > 0)
		{
			initialColor = spriteRenderers[0].color;
		}
		return initialColor;
	}

	public void SetColor(float t)
	{
		currentColor = Color.Lerp(initialColor, targetColor, t);
		foreach (Material sharedMaterial in sharedMaterials)
		{
			sharedMaterial.color = currentColor;
		}
	}

	public void SetColorEditorMode(float t)
	{
		currentColor = Color.Lerp(initialColor, targetColor, t);
		foreach (Material sharedMaterial in sharedMaterials)
		{
			sharedMaterial.color = currentColor;
		}
	}

	public void CleanUp()
	{
		if (sharedMaterials == null)
		{
			return;
		}
		foreach (Material sharedMaterial in sharedMaterials)
		{
			if (sharedMaterial != null)
			{
				UnityEngine.Object.Destroy(sharedMaterial);
			}
		}
		sharedMaterials.Clear();
	}
}
