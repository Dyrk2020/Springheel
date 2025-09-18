using UnityEngine;

public class AttachmentIndicator : MonoBehaviour
{
	public MultipiecePart[] attachementParts;

	private SpriteRenderer spriteRenderer;

	private Sprite initialSprite;

	public bool isOn = true;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		initialSprite = spriteRenderer.sprite;
	}

	private void Update()
	{
		if (!isOn)
		{
			return;
		}
		bool flag = false;
		MultipiecePart[] array = attachementParts;
		foreach (MultipiecePart multipiecePart in array)
		{
			if (multipiecePart != null)
			{
				if (multipiecePart.Placed)
				{
					isOn = false;
					break;
				}
				if (multipiecePart.CanPlace())
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			spriteRenderer.sprite = null;
		}
		else
		{
			spriteRenderer.sprite = initialSprite;
		}
	}
}
