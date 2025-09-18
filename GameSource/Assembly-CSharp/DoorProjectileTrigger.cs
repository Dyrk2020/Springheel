using UnityEngine;

public class DoorProjectileTrigger : MonoBehaviour
{
	public AutoDoor door;

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (CanTouchTrigger(collision))
		{
			door.OnProjectileTouchedTrigger();
		}
	}

	public void OnTriggerStay2D(Collider2D collision)
	{
		if (CanTouchTrigger(collision))
		{
			door.OnProjectileTouchedTrigger();
		}
	}

	private bool CanTouchTrigger(Collider2D collision)
	{
		if (Modifiers.GetInstance().ProjectilesExplode && (bool)collision.gameObject.GetComponentInParent<ModExplosionCharacterForce>())
		{
			return false;
		}
		return true;
	}
}
