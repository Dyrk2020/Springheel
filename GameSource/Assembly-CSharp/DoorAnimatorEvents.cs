using UnityEngine;

public class DoorAnimatorEvents : MonoBehaviour
{
	public AutoDoor door;

	public void enableDoorCollider()
	{
		door.enableDoorCollider();
	}

	public void disableDoorCollider()
	{
		door.disableDoorCollider();
	}
}
