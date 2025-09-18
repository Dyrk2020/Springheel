using UnityEngine;

public class TwitchSettingsVisibilityToggler : MonoBehaviour
{
	public GameObject settingsPane;

	public GameObject hostOnlyPane;

	public GameObject[] settingsToHideIfVotingOff;

	private void Start()
	{
	}

	private void Update()
	{
		bool enableTwitchVoting = GameSettings.GetInstance().enableTwitchVoting;
		GameObject[] array = settingsToHideIfVotingOff;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(enableTwitchVoting);
		}
		bool flag = LobbyManager.instance == null || LobbyManager.instance.IsHost;
		settingsPane.SetActive(flag);
		hostOnlyPane.SetActive(!flag);
	}
}
