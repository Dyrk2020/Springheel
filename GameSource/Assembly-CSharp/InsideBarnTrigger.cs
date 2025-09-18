using UnityEngine;

public class InsideBarnTrigger : MonoBehaviour
{
	public SpriteRenderer barnfront;

	public SpriteRenderer[] AllSprites;

	public SpriteRenderer[] ShowSprites;

	public float fadeSpeed = 1f;

	public bool characterInside;

	public void FixedUpdate()
	{
		float a = barnfront.color.a;
		a = ((!characterInside) ? Mathf.MoveTowards(a, 1f, fadeSpeed * Time.fixedDeltaTime) : Mathf.MoveTowards(a, 0f, fadeSpeed * Time.fixedDeltaTime));
		if (barnfront != null)
		{
			barnfront.color = new Color(barnfront.color.r, barnfront.color.g, barnfront.color.b, a);
		}
		if (AllSprites != null && AllSprites.Length != 0)
		{
			SpriteRenderer[] allSprites = AllSprites;
			foreach (SpriteRenderer spriteRenderer in allSprites)
			{
				if (spriteRenderer != null)
				{
					spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, a);
				}
			}
		}
		if (ShowSprites != null && ShowSprites.Length != 0)
		{
			SpriteRenderer[] allSprites = ShowSprites;
			foreach (SpriteRenderer spriteRenderer2 in allSprites)
			{
				if (spriteRenderer2 != null)
				{
					spriteRenderer2.color = new Color(spriteRenderer2.color.r, spriteRenderer2.color.g, spriteRenderer2.color.b, 1f - a);
				}
			}
		}
		characterInside = false;
	}

	public void OnTriggerStay2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (component != null && component.ContainsAnyTag(TagComparer.Tag.Player))
		{
			characterInside = true;
		}
	}
}
