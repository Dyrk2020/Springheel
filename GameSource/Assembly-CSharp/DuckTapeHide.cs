using UnityEngine;

public class DuckTapeHide : MonoBehaviour
{
	private SpriteRenderer sprite;

	private void Start()
	{
		sprite = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (component != null && component.ContainsAnyTag(TagComparer.Tag.Player))
		{
			sprite.enabled = false;
		}
	}
}
