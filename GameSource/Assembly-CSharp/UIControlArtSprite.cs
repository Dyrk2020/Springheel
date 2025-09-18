using UnityEngine;

public class UIControlArtSprite : MonoBehaviour
{
	public buttonTypes buttontype;

	public SpriteRenderer targetSprite;

	public void UpdateSprite()
	{
		targetSprite.sprite = UIControlLibrary.GetInstance().GetButtonArt(buttontype);
	}

	public void Disable()
	{
		targetSprite.enabled = false;
	}

	public void Enable()
	{
		targetSprite.enabled = true;
		UpdateSprite();
	}
}
