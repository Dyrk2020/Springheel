using UnityEngine;

public class TabletFreezeLobbyGroup : MonoBehaviour
{
	public TabletCheckbox freezeCheckbox;

	public RectTransform inviteButton;

	private bool freezeChange;

	private Vector3 inviteButtonPosition;

	private void Awake()
	{
		inviteButtonPosition = inviteButton.localPosition;
	}

	public void UpdateCheckboxState()
	{
		if (!Matchmaker.Instance.IsLobbyOwner())
		{
			inviteButton.localPosition = base.transform.localPosition;
			base.gameObject.SetActive(value: false);
			return;
		}
		inviteButton.localPosition = inviteButtonPosition;
		base.gameObject.SetActive(value: true);
		freezeChange = true;
		freezeCheckbox.SetValue(!string.IsNullOrEmpty(GameSettings.GetInstance().frozenLobbyCode));
		freezeChange = false;
	}

	public void OnCheckboxValueChanged()
	{
		if (!freezeChange && Matchmaker.Instance.IsLobbyOwner())
		{
			freezeChange = true;
			if (freezeCheckbox.value)
			{
				OnFreezeLobbyGroup();
			}
			else
			{
				OnUnfreezeLobbyGroup();
			}
		}
	}

	public void OnFreezeLobbyGroup()
	{
		freezeCheckbox.SetDisabled(disabled: true);
		string lobbyCode = Matchmaker.CurrentMatchmakingLobby.GetLobbyCode();
		(GameSparksManager.Instance as BraincloudManager).FreezeLobbyCode(delegate(bool success)
		{
			if (!success)
			{
				freezeCheckbox.value = false;
			}
			freezeChange = false;
			freezeCheckbox.SetDisabled(disabled: false);
		}, lobbyCode);
	}

	public void OnUnfreezeLobbyGroup()
	{
		freezeCheckbox.SetDisabled(disabled: true);
		(GameSparksManager.Instance as BraincloudManager).UnfreezeLobbyCode(delegate(bool success)
		{
			if (!success)
			{
				freezeCheckbox.value = true;
			}
			freezeChange = false;
			freezeCheckbox.SetDisabled(disabled: false);
		});
	}
}
