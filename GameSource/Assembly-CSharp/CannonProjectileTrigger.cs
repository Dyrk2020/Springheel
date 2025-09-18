using UnityEngine;

public class CannonProjectileTrigger : MonoBehaviour
{
	public AnimalCannon animalCannon;

	public void OnTriggerEnter2D(Collider2D collision)
	{
		animalCannon.OnProjectileEnterTrigger(collision);
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		animalCannon.OnProjectileExitTrigger(collision);
	}
}
