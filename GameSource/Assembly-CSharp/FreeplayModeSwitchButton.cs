using System;
using GameEvent;
using I2.Loc;
using UnityEngine.Networking;

public class FreeplayModeSwitchButton : PickableButton
{
	public GameState.GameMode CurrentMode;

	private string[] modeNames;

	protected override void Start()
	{
		base.Start();
		getModeNames();
		toNextMode();
	}

	private void getModeNames()
	{
		GameState.GameMode[] array = (GameState.GameMode[])Enum.GetValues(typeof(GameState.GameMode));
		modeNames = new string[array.Length];
		modeNames[0] = ScriptLocalization.RuleBook.FreePlayText;
		modeNames[1] = ScriptLocalization.RuleBook.CreativeModeText;
		modeNames[2] = ScriptLocalization.RuleBook.PartyModeText;
		modeNames[3] = ScriptLocalization.RuleBook.ChallengeModeText;
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		toNextMode();
	}

	private void toNextMode()
	{
		int num = 0;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			if (lobbySlots[i] != null)
			{
				num++;
			}
		}
		if (num == 1)
		{
			CurrentMode = ((CurrentMode != GameState.GameMode.CHALLENGE) ? GameState.GameMode.CHALLENGE : GameState.GameMode.FREEPLAY);
		}
		else
		{
			CurrentMode = GameState.NextMode(CurrentMode);
		}
		buttonText.text = ScriptLocalization.RuleBook.Game_Mode + modeNames[(int)CurrentMode];
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(LanguageChangeEvent))
		{
			getModeNames();
		}
	}
}
