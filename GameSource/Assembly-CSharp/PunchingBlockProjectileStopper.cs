using UnityEngine;

public class PunchingBlockProjectileStopper : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collider)
	{
		Projectile component = collider.GetComponent<Projectile>();
		if (component != null)
		{
			component.collided = true;
		}
	}
}
