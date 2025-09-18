using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
	public Button Host;

	public Button Join;

	public Button Disconnect;

	public Button Back;

	public Canvas UICanvas;

	private void Start()
	{
		Hide();
	}

	public void Show()
	{
		UICanvas.enabled = true;
		if (NetworkClient.active)
		{
			Host.enabled = false;
			Join.enabled = false;
			Disconnect.enabled = true;
		}
		else
		{
			Host.enabled = true;
			Join.enabled = true;
			Disconnect.enabled = false;
		}
	}

	public void Hide()
	{
		UICanvas.enabled = false;
	}

	public void StartHost()
	{
		Hide();
	}

	public void StartClient()
	{
		Hide();
	}

	public void Stop()
	{
		if (NetworkClient.active)
		{
			LobbyManager instance = LobbyManager.instance;
			if (instance.IsHost)
			{
				instance.StopHost();
			}
			else
			{
				instance.StopClient();
			}
		}
	}
}
