using System;
using GameEvent;
using Steamworks;
using UnityEngine;

public class CheatListener : MonoBehaviour, InputReceiver
{
	[Serializable]
	public struct CheatCode
	{
		public InputEvent.InputKey[] Buttons;

		public bool Activated;

		private int pointer;

		public bool TryCheat(InputEvent.InputKey key)
		{
			if (key == Buttons[pointer])
			{
				pointer++;
			}
			else if (key == Buttons[0])
			{
				pointer = 1;
			}
			else
			{
				pointer = 0;
			}
			if (pointer == Buttons.Length)
			{
				pointer = 0;
				Activated = true;
				return true;
			}
			return false;
		}
	}

	public CheatCode UnlockCode;

	public CheatCode KonamiCode;

	public CheatCode EasyUnlockCode;

	public CheatCode HalfUnlockCode;

	public CheatCode OneUnlocker;

	public CheatCode SteamAchievementResetter;

	public CheatCode DebugModeEnabler;

	public CheatCode DebugNoSpecialUI;

	public CheatCode AddGamePlayedOnRandomUnlockedMap;

	public CheatCode ControllerControlsCamera;

	private bool debugAllowed;

	private InputEvent.InputKey[] cheatKeys = new InputEvent.InputKey[12]
	{
		InputEvent.InputKey.Accept,
		InputEvent.InputKey.Back,
		InputEvent.InputKey.Sprint,
		InputEvent.InputKey.Inventory,
		InputEvent.InputKey.Up,
		InputEvent.InputKey.Down,
		InputEvent.InputKey.Left,
		InputEvent.InputKey.Right,
		InputEvent.InputKey.Start,
		InputEvent.InputKey.Scoreboard,
		InputEvent.InputKey.RotateLeft,
		InputEvent.InputKey.RotateRight
	};

	private void Start()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-debugCheats")
			{
				debugAllowed = true;
			}
		}
	}

	private void Update()
	{
	}

	public void ReceiveEvent(InputEvent e)
	{
		bool flag = false;
		for (int i = 0; i != cheatKeys.Length; i++)
		{
			if (e.Key == cheatKeys[i])
			{
				flag = true;
				break;
			}
		}
		if (!flag || !e.Valueb || !e.Changed)
		{
			return;
		}
		if (KonamiCode.TryCheat(e.Key))
		{
			GameEventManager.SendEvent(new CheatKonamiEvent());
			Debug.Log("Contra is too hard");
		}
		if (EasyUnlockCode.TryCheat(e.Key))
		{
			GameEventManager.SendEvent(new CheatUnlockEvent());
			Debug.Log(" Easy EVERYTHING UNLOCKED");
		}
		if (HalfUnlockCode.TryCheat(e.Key))
		{
			GameEventManager.SendEvent(new CheatUnlockHalfEvent());
			Debug.Log("Half of Stuff Unlocked");
		}
		if (ControllerControlsCamera.TryCheat(e.Key))
		{
			GameEventManager.SendEvent(new ControllerControlsCamera(e.PlayerBitMask));
		}
		if (Application.isEditor && SteamAchievementResetter.TryCheat(e.Key))
		{
			if (!SteamManager.Destroyed && SteamManager.Initialized)
			{
				Debug.Log("Resetting all Steam stats and achievements.");
				SteamUserStats.ResetAllStats(bAchievementsToo: true);
			}
			else
			{
				Debug.Log("Could not reset all Steam stats and achievements - SteamManager not initialized.");
			}
		}
		if (Application.isEditor || GameSettings.GetInstance().DebugOutsideEditor || debugAllowed)
		{
			if (UnlockCode.TryCheat(e.Key))
			{
				GameEventManager.SendEvent(new CheatUnlockEvent());
				Debug.Log("EVERYTHING UNLOCKED");
			}
			if (DebugModeEnabler.TryCheat(e.Key))
			{
				GameState.ToggleDebugMode();
			}
			if (DebugNoSpecialUI.TryCheat(e.Key))
			{
				GameState.ToggleDebugModeNoSpecialUI();
			}
			if (OneUnlocker.TryCheat(e.Key))
			{
				GameEventManager.SendEvent(new OneUnlockMaker());
				Debug.Log(" Makes One UnlockAppear");
			}
			if (AddGamePlayedOnRandomUnlockedMap.TryCheat(e.Key))
			{
				GameEventManager.SendEvent(new CheatRandomGamePlayedEvent());
			}
		}
	}
}
