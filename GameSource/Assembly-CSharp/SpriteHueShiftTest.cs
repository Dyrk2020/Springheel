using UnityEngine;

public class SpriteHueShiftTest : MonoBehaviour
{
	public SpriteRenderer[] sprites;

	private float timer;

	private void Start()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		int num = sprites.Length;
		float num2 = 1f / (float)num;
		float num3 = 0f;
		SpriteRenderer[] array = sprites;
		foreach (SpriteRenderer obj in array)
		{
			num3 += num2;
			materialPropertyBlock.SetFloat("_HueShiftAmount", num3 * 0.5f);
			obj.SetPropertyBlock(materialPropertyBlock);
		}
	}

	private void Update()
	{
		timer += Time.deltaTime;
		while (timer > 1f)
		{
			timer -= 1f;
		}
	}
}
