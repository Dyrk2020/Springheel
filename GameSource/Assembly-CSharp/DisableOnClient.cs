using UnityEngine;

public class DisableOnClient : MonoBehaviour
{
	private void Start()
	{
		if (LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
