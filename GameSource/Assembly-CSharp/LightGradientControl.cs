using System.Collections;
using UnityEngine;

public class LightGradientControl : MonoBehaviour
{
	public Color lightGradientTint;

	public Transform GradientTop;

	public Transform GradientBottom;

	protected Vector2 lastTopPosition;

	protected Vector2 lastBottomPosition;

	public Material spriteLitmaterial;

	public Material spriteLitmaterialHSV;

	private void Start()
	{
		StartCoroutine(waitForMaterial());
	}

	private IEnumerator waitForMaterial()
	{
		yield return null;
		yield return null;
		SetAllmaterials();
	}

	private void Update()
	{
		if (lastTopPosition.y != GradientTop.position.y || lastBottomPosition.y != GradientBottom.position.y)
		{
			SetAllmaterials();
		}
	}

	private void SetAllmaterials()
	{
		SetSpriteGradient(spriteLitmaterial);
		SetSpriteGradient(spriteLitmaterialHSV);
	}

	private void SetSpriteGradient(Material mat)
	{
		mat.SetColor("_GradientColor", lightGradientTint);
		mat.SetFloat("_GradientTop", GradientTop.position.y);
		mat.SetFloat("_GradientBottom", GradientBottom.position.y);
	}
}
