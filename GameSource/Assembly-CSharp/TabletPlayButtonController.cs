using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TabletPlayButtonController : MonoBehaviour
{
	public TabletButton modePartyButton;

	public TabletButton modeCreativeButton;

	public TabletButton modeFreeplayButton;

	public TabletButton modeChallengeButton;

	public UnityEvent ReloadInNewModeCallback;

	private bool pressedReloadInNewMode;

	private bool PlayingAlone
	{
		get
		{
			int num = 0;
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				if (lobbySlots[i] != null)
				{
					num++;
					if (num > 1)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public void EnablePlayButtons()
	{
		if (LobbyManager.instance != null)
		{
			pressedReloadInNewMode = false;
			base.gameObject.SetActive(value: true);
			bool playingAlone = PlayingAlone;
			if (LobbyManager.instance.IsHost)
			{
				modePartyButton.SetDisabled(playingAlone);
				modeCreativeButton.SetDisabled(playingAlone);
				modeChallengeButton.SetDisabled(disabled: false);
				modeFreeplayButton.SetDisabled(disabled: false);
			}
			else
			{
				modePartyButton.SetDisabled(disabled: true);
				modeCreativeButton.SetDisabled(disabled: true);
				modeChallengeButton.SetDisabled(disabled: true);
				modeFreeplayButton.SetDisabled(disabled: true);
			}
		}
	}

	public void DisablePlayButtons()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnClickPlayInParty(PickCursor pickCursor)
	{
		if (!PlayingAlone && !pressedReloadInNewMode)
		{
			pressedReloadInNewMode = true;
			GameSettings.GetInstance().GameMode = GameState.GameMode.PARTY;
			ReloadInNewModeCallback.Invoke();
		}
	}

	public void OnClickPlayInCreative(PickCursor pickCursor)
	{
		if (!PlayingAlone && !pressedReloadInNewMode)
		{
			pressedReloadInNewMode = true;
			GameSettings.GetInstance().GameMode = GameState.GameMode.CREATIVE;
			ReloadInNewModeCallback.Invoke();
		}
	}

	public void OnClickPlayInFreePlay(PickCursor pickCursor)
	{
		if (!pressedReloadInNewMode)
		{
			pressedReloadInNewMode = true;
			GameSettings.GetInstance().GameMode = GameState.GameMode.FREEPLAY;
			ReloadInNewModeCallback.Invoke();
		}
	}

	public void OnClickPlayInChallenge(PickCursor pickCursor)
	{
		if (!pressedReloadInNewMode)
		{
			pressedReloadInNewMode = true;
			GameSettings.GetInstance().GameMode = GameState.GameMode.CHALLENGE;
			ReloadInNewModeCallback.Invoke();
		}
	}
}
