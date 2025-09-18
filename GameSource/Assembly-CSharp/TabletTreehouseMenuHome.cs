using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletTreehouseMenuHome : MonoBehaviour, IGameEventListener
{
	public Text modeText;

	public TabletToggleImage gameModeLock;

	public TabletDisableGroup gameModeWidgetDisableGroup;

	public Tablet tablet;

	private bool lastUsingHotseat;

	public void Initialize()
	{
		if (modeText != null)
		{
			Localize component = modeText.GetComponent<Localize>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		UpdateModeText(GameSettings.GetInstance().GameMode);
		ChangeListener(adding: true);
		if (LobbyManager.instance != null && !LobbyManager.instance.IsHost)
		{
			gameModeLock.gameObject.SetActive(value: false);
			gameModeWidgetDisableGroup.SetDisabled(disabled: true);
		}
	}

	private void Start()
	{
		Initialize();
	}

	public void Update()
	{
		bool modeLocked = GameSettings.GetInstance().ModeLocked;
		if (gameModeLock.Value != modeLocked)
		{
			gameModeLock.SetValue(modeLocked);
		}
		if (lastUsingHotseat != GameState.GetInstance().UsingHotSeat)
		{
			lastUsingHotseat = GameState.GetInstance().UsingHotSeat;
			HotseatChanged();
		}
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void OnGameModeLockValueChanged()
	{
		if (LobbyManager.instance.CurrentLevelSelectController != null)
		{
			MsgSetGameModeLock msgSetGameModeLock = new MsgSetGameModeLock();
			msgSetGameModeLock.Locked = gameModeLock.Value;
			NetworkServer.SendToAll(NetMsgTypes.SetGameModeLock, msgSetGameModeLock);
		}
	}

	public void HotseatChanged()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.IsHost)
		{
			if (GameState.GetInstance().UsingHotSeat)
			{
				gameModeWidgetDisableGroup.SetDisabled(disabled: true);
			}
			else
			{
				gameModeWidgetDisableGroup.SetDisabled(disabled: false);
			}
		}
	}

	private void UpdateModeText(GameState.GameMode mode)
	{
		if (modeText != null)
		{
			switch (mode)
			{
			case GameState.GameMode.FREEPLAY:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/FreePlayText");
				break;
			case GameState.GameMode.CREATIVE:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/CreativeModeText");
				break;
			case GameState.GameMode.PARTY:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/PartyModeText");
				break;
			case GameState.GameMode.CHALLENGE:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/ChallengeModeText");
				break;
			}
		}
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = (MsgSwitchToMode)networkMessageReceivedEvent.ReadMessage;
				UpdateModeText(msgSwitchToMode.toMode);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.GameRuleSet && !LobbyManager.instance.IsHost && ((MsgGameRuleSet)networkMessageReceivedEvent.ReadMessage).NewRule == TabletRule.GameMode)
			{
				UpdateButtonValue();
			}
		}
		if (e.GetType() == typeof(LanguageChangeEvent))
		{
			UpdateButtonValue();
		}
	}

	private void UpdateButtonValue()
	{
		UpdateModeText(GameSettings.GetInstance().GameMode);
	}

	public void OnClickMode()
	{
		tablet.modalOverlay.Initialize(TabletRule.GameMode, UpdateButtonValue);
	}
}
