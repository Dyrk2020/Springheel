using UnityEngine;

public class SaveGameOnDisable : MonoBehaviour
{
	private void OnDisable()
	{
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			StatTracker.Instance.SaveGameForAllUsers();
			Debug.Log("Saving on disable");
		}
	}
}
