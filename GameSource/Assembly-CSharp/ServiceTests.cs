using UnityEngine;

public class ServiceTests : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			UCHOnlineConnector.Service.GetAvailableRegions();
		}
	}
}
