using System;
using System.Collections;
using UnityEngine;

public class SwitchConnectButton : MonoBehaviour
{
	public GameObject Spinner;

	public TabletButton tabletButton;

	public PickableButton pickableButton;

	private bool connected => true;

	private void Start()
	{
		setWorking(working: false);
	}

	private void setWorking(bool working)
	{
		Spinner.SetActive(working);
		if (tabletButton != null)
		{
			tabletButton.SetDisabled(working);
		}
		if (pickableButton != null)
		{
			pickableButton.DeactivatedInBook = working;
		}
	}

	public void Show(bool show = true)
	{
		if (tabletButton != null)
		{
			tabletButton.gameObject.SetActive(show);
		}
		if (pickableButton != null)
		{
			pickableButton.gameObject.SetActive(show);
		}
	}

	public void Hide()
	{
		Show(show: false);
	}

	public void TryConnect(PickCursor cursor)
	{
		Debug.Log("Connect button clicked");
		setWorking(working: true);
		TabletMainMenuOnlineIndicator.EnsureMainUserOnlinePermissionsValid(delegate(bool success)
		{
			Debug.Log("MainUserOnlinePermissionsValid: " + success);
		}, delegate
		{
		}, delegate
		{
		}, this, force: true);
	}

	private IEnumerator waitForGamesparks(Action<bool> onFinish)
	{
		Debug.Log("Waiting for backend");
		bool triedConnecting = false;
		bool failedToConnect = !GameSparksManager.Instance.Connected;
		while (!GameSparksManager.Instance.Connected)
		{
			if (GameSparksManager.Instance.Connecting)
			{
				triedConnecting = true;
			}
			if (!triedConnecting)
			{
				break;
			}
			if (!connected)
			{
				Debug.Log("No longer connected after starting to wait for backend!");
				break;
			}
			if (!GameSparksManager.Instance.Connecting)
			{
				failedToConnect = true;
				break;
			}
			if (!GameSparksManager.Instance.Connecting)
			{
				triedConnecting = false;
			}
			yield return null;
		}
		PickableButton.ResetMasks();
		Debug.Log("Done waiting for backend");
		onFinish(!failedToConnect);
	}
}
