using UnityEngine;

public class SpriteFont : MonoBehaviour
{
	public enum Align
	{
		LEFT,
		CENTER,
		RIGHT
	}

	public Sprite[] ASCII = new Sprite[256];

	public float CharacterWidth = 1f;

	private static GameObject textContainer;

	public void DrawString(string text, Vector3 position)
	{
		DrawString(text, position, new Vector3(1f, 1f, 1f), Align.LEFT, Color.white, "Default");
	}

	public void DrawString(string text, Vector3 position, Vector3 scale)
	{
		DrawString(text, position, scale, Align.LEFT, Color.white, "Default");
	}

	public void DrawString(string text, Vector3 position, Vector3 scale, Align alignment)
	{
		DrawString(text, position, scale, alignment, Color.white, "Default");
	}

	public void DrawString(string text, Vector3 position, Vector3 scale, Align alignment, Color color)
	{
		DrawString(text, position, scale, alignment, color, "Default");
	}

	public void DrawString(string text, Vector3 position, Vector3 scale, Align alignment, Color color, string sortingLayer)
	{
		if (textContainer == null)
		{
			textContainer = new GameObject("Text Container", typeof(ChildDestroyer));
		}
		GameObject gameObject = new GameObject(text);
		gameObject.transform.parent = textContainer.transform;
		switch (alignment)
		{
		case Align.CENTER:
			position.x -= (float)text.Length * CharacterWidth * scale.x / 2f;
			break;
		case Align.RIGHT:
			position.x -= (float)text.Length * CharacterWidth * scale.x;
			break;
		}
		for (int i = 0; i != text.Length; i++)
		{
			int num = text[i];
			if (!(ASCII[num] == null))
			{
				GameObject obj = new GameObject("_" + text[i], typeof(SpriteRenderer));
				SpriteRenderer component = obj.GetComponent<SpriteRenderer>();
				component.gameObject.layer = base.gameObject.layer;
				component.sprite = ASCII[num];
				component.sortingLayerName = sortingLayer;
				component.material.color = color;
				obj.transform.position = position + new Vector3((float)i * CharacterWidth * scale.x, 0f, 0f);
				obj.transform.localScale = scale;
				obj.transform.parent = gameObject.transform;
			}
		}
	}
}
