using UnityEngine;

public class AirplaneProjectileTrigger : MonoBehaviour
{
	public Projectile airplane;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		airplane.ProjectileTrigger(collider);
	}
}
