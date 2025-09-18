using UnityEngine;

public class TeleporterProjectileSend : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public void Die()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetColor(Color color)
	{
		spriteRenderer.color = color;
	}
}
