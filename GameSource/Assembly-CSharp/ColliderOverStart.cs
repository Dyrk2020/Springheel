using GameEvent;
using UnityEngine;

public class ColliderOverStart : MonoBehaviour
{
	public bool OverStartZone { get; protected set; }

	public void Reset()
	{
		OverStartZone = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!OverStartZone && !(collision == null) && !(collision.gameObject == null))
		{
			CollisionTag component = collision.gameObject.GetComponent<CollisionTag>();
			if (component != null && component.ContainsAnyTag(TagComparer.Tag.Start))
			{
				OverStartZone = true;
				GameEventManager.SendEvent(new HoldRespawnEvent(hold: true));
				Debug.Log("Preventing respawns due to lava in the start zone");
			}
		}
	}
}
