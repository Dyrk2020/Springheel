using UnityEngine;

public class SpriteSplash : UISplashScreen
{
	public Color StartColor = Color.white;

	public Color FadeColor = Color.black;

	private SpriteRenderer spriteRenderer;

	public SpriteRenderer Sprite
	{
		get
		{
			if (spriteRenderer == null)
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
			}
			return spriteRenderer;
		}
	}

	public override void Setup()
	{
	}

	public override void Show()
	{
		base.Show();
		Sprite.material.color = StartColor;
	}

	public override void Hide()
	{
		base.Hide();
		Sprite.material.color = FadeColor;
	}

	public override void Fade(float alpha)
	{
		base.Fade(alpha);
		Sprite.material.color = new Color(Mathf.Lerp(FadeColor.r, StartColor.r, alpha), Mathf.Lerp(FadeColor.g, StartColor.g, alpha), Mathf.Lerp(FadeColor.b, StartColor.b, alpha), Mathf.Lerp(FadeColor.a, StartColor.a, alpha));
	}
}
