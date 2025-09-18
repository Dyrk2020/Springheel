using GameEvent;
using UnityEngine;

public class SurfboardHider : MonoBehaviour
{
	public GameObject surfboard;

	public void ShowSurfboard()
	{
		surfboard.SetActive(value: true);
	}

	public void HideSurfboard()
	{
		surfboard.SetActive(value: false);
	}

	public void holdRespawn()
	{
		GameEventManager.SendEvent(new HoldRespawnEvent(hold: true));
	}

	public void allowRespawn()
	{
		GameEventManager.SendEvent(new HoldRespawnEvent(hold: false));
	}
}
