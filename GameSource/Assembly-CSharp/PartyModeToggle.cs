using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;

public class PartyModeToggle : MonoBehaviour, IGameEventListener
{
	public Animator discoBallAnimator;

	public ButtonSlide PartyButton;

	public Animator textFlipper;

	public TextChanger textChanger;

	public PartyModeColor[] cs;

	public PartyLight[] ls;

	protected List<FlashOffsetSpeed> flashingArrowAnimators = new List<FlashOffsetSpeed>();

	public TreehouseModeTheme[] Themes;

	public float changeSpeedMultiplier;

	public GameState.GameMode lastAudioMode;

	private void Start()
	{
		PartyModeColor[] array = cs;
		foreach (PartyModeColor obj in array)
		{
			obj.setInitialColor();
			obj.currentColor = obj.initialColor;
		}
		PartyLight[] array2 = ls;
		foreach (PartyLight obj2 in array2)
		{
			obj2.setInitialLight();
			obj2.currentLightColor = obj2.initialLightColor;
			obj2.currentLightIntensity = obj2.initialLightIntensity;
			obj2.currentLightCookieSize = obj2.initialLightCookieSize;
		}
		flashingArrowAnimators.AddRange(UnityEngine.Object.FindObjectsOfType<FlashOffsetSpeed>());
		textChanger.nextString = ScriptLocalization.InLobby.PartyText;
		ForceLighting(GameSettings.GetInstance().GameMode);
		ChangeListener(adding: true);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
		PartyModeColor[] array = cs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.CleanUp();
		}
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	private void Update()
	{
		if (PartyButton.TriggeredThisFrame)
		{
			if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
			{
				Debug.LogWarning("Ignored button press - we are locked for load!");
			}
			else
			{
				GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
				MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
				GameState.GameMode mode = (msgSwitchToMode.toMode = GameState.NextMode(gameMode));
				NetworkManager.singleton.client.Send(NetMsgTypes.SwitchToMode, msgSwitchToMode);
				GameEventManager.SendEvent(new GameModeSetEvent(mode));
			}
		}
		PartyButton.TriggeredThisFrame = false;
	}

	public void changeModes(GameState.GameMode toMode)
	{
		StopCoroutine("ChangeColours");
		StartCoroutine("ChangeColours", toMode);
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if (gameMode == toMode)
		{
			switch (gameMode)
			{
			case GameState.GameMode.CREATIVE:
				textChanger.nextString = ScriptLocalization.InLobby.CreativeText;
				break;
			case GameState.GameMode.PARTY:
				textChanger.nextString = ScriptLocalization.InLobby.PartyText;
				break;
			case GameState.GameMode.FREEPLAY:
				textChanger.nextString = ScriptLocalization.InLobby.FreePlayButtonText;
				break;
			case GameState.GameMode.CHALLENGE:
				textChanger.nextString = ScriptLocalization.InLobby.ChallengeModeButtonText;
				break;
			}
			textFlipper.SetTrigger("Flip");
			return;
		}
		switch (toMode)
		{
		case GameState.GameMode.PARTY:
			textChanger.nextString = ScriptLocalization.InLobby.PartyText;
			break;
		case GameState.GameMode.CREATIVE:
			textChanger.nextString = ScriptLocalization.InLobby.CreativeText;
			break;
		case GameState.GameMode.FREEPLAY:
			textChanger.nextString = ScriptLocalization.InLobby.FreePlayButtonText;
			break;
		case GameState.GameMode.CHALLENGE:
			textChanger.nextString = ScriptLocalization.InLobby.ChallengeModeButtonText;
			break;
		}
		GameSettings.GetInstance().GameMode = toMode;
		textFlipper.SetTrigger("Flip");
		GameEventManager.SendEvent(new GameModeSetEvent(toMode));
	}

	public IEnumerator ChangeColours(GameState.GameMode toMode)
	{
		bool modeChange = toMode != GameSettings.GetInstance().GameMode;
		if (Themes.Length <= (int)toMode || Themes[(int)toMode] == null)
		{
			yield break;
		}
		TreehouseModeTheme treehouseModeTheme = Themes[(int)toMode];
		discoBallAnimator.SetBool("Deploy", treehouseModeTheme.ShowDiscoBall);
		for (int i = 0; i != cs.Length; i++)
		{
			cs[i].initialColor = cs[i].currentColor;
		}
		for (int j = 0; j != ls.Length; j++)
		{
			PartyLight obj = ls[j];
			obj.initialLightColor = obj.currentLightColor;
			obj.initialLightCookieSize = obj.currentLightCookieSize;
			obj.initialLightIntensity = obj.currentLightIntensity;
		}
		foreach (FlashOffsetSpeed flashingArrowAnimator in flashingArrowAnimators)
		{
			flashingArrowAnimator.Show(treehouseModeTheme.ShowFlashingArrows);
		}
		cs[0].targetColor = treehouseModeTheme.SkyGradient;
		cs[1].targetColor = treehouseModeTheme.Clouds;
		cs[2].targetColor = treehouseModeTheme.GrassFront;
		cs[3].targetColor = treehouseModeTheme.GrassMid;
		cs[4].targetColor = treehouseModeTheme.GrassBack;
		cs[5].targetColor = treehouseModeTheme.Branches;
		cs[6].targetColor = treehouseModeTheme.FoliageFront;
		cs[7].targetColor = treehouseModeTheme.FoliageBack;
		cs[8].targetColor = treehouseModeTheme.Sunrise;
		cs[9].targetColor = treehouseModeTheme.PartyButton;
		cs[10].targetColor = treehouseModeTheme.Stars;
		cs[11].targetColor = treehouseModeTheme.LevelCountSign;
		cs[12].targetColor = treehouseModeTheme.HandicapSign;
		cs[13].targetColor = treehouseModeTheme.TreehousePlatforms;
		cs[14].targetColor = treehouseModeTheme.ChallengeModeDecorA;
		cs[15].targetColor = treehouseModeTheme.ChallengeModeDecorB;
		cs[16].targetColor = treehouseModeTheme.ChallengeModeDecorC;
		cs[17].targetColor = treehouseModeTheme.UnderGroundDarkDirt;
		cs[18].targetColor = treehouseModeTheme.UnderGroundWall;
		cs[19].targetColor = treehouseModeTheme.UnderGroundRoots;
		cs[20].targetColor = treehouseModeTheme.ChallengeModeLightBlankBehind;
		ls[0].targetLightIntensity = treehouseModeTheme.WorldLightIntensity;
		ls[0].targetLightColor = treehouseModeTheme.WorldLightColour;
		ls[0].targetlightCookieSize = treehouseModeTheme.WorldLightCookieSize;
		ls[1].targetLightIntensity = treehouseModeTheme.DiscoLight1Intensity;
		ls[1].targetLightColor = treehouseModeTheme.DiscoLight1Colour;
		ls[1].targetlightCookieSize = treehouseModeTheme.DiscoLight1CookieSize;
		ls[2].targetLightIntensity = treehouseModeTheme.DiscoLight2Intensity;
		ls[2].targetLightColor = treehouseModeTheme.DiscoLight2Colour;
		ls[2].targetlightCookieSize = treehouseModeTheme.DiscoLight2CookieSize;
		float t = 0f;
		while (t < 1f)
		{
			t = Mathf.MoveTowards(t, 1f, Time.deltaTime * changeSpeedMultiplier);
			for (int k = 0; k != cs.Length; k++)
			{
				cs[k].SetColor(t);
			}
			for (int l = 0; l != ls.Length; l++)
			{
				ls[l].setLight(t);
			}
			yield return null;
		}
		for (int m = 0; m != cs.Length; m++)
		{
			cs[m].initialColor = cs[m].targetColor;
		}
		for (int n = 0; n != ls.Length; n++)
		{
			PartyLight obj2 = ls[n];
			obj2.initialLightColor = obj2.targetLightColor;
			obj2.initialLightCookieSize = obj2.targetlightCookieSize;
			obj2.initialLightIntensity = obj2.targetLightIntensity;
		}
		if (PartyButton.WithAudio)
		{
			switch (lastAudioMode)
			{
			case GameState.GameMode.FREEPLAY:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_FreePlay_Off", base.gameObject);
				break;
			case GameState.GameMode.CREATIVE:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Creative_Off", base.gameObject);
				break;
			case GameState.GameMode.PARTY:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Party_Off", base.gameObject);
				break;
			case GameState.GameMode.CHALLENGE:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Challenge_Off", base.gameObject);
				break;
			}
		}
		if (PartyButton.WithAudio || modeChange)
		{
			lastAudioMode = toMode;
			switch (toMode)
			{
			case GameState.GameMode.FREEPLAY:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_FreePlay_On", base.gameObject);
				AkSoundEngine.PostEvent("Lobby_Freeplay", base.gameObject);
				break;
			case GameState.GameMode.CREATIVE:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Creative_On", base.gameObject);
				AkSoundEngine.PostEvent("Lobby_Normal", base.gameObject);
				break;
			case GameState.GameMode.PARTY:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Party_On", base.gameObject);
				AkSoundEngine.PostEvent("Lobby_PartyMode", base.gameObject);
				break;
			case GameState.GameMode.CHALLENGE:
				AkSoundEngine.PostEvent("SFX_Lobby_Mode_Challenge_On", base.gameObject);
				AkSoundEngine.PostEvent("Lobby_Challenge", base.gameObject);
				break;
			}
		}
		if (!PartyButton.WithAudio)
		{
			PartyButton.WithAudio = true;
			lastAudioMode = toMode;
		}
	}

	public void CopyInitialColors()
	{
		PartyModeColor[] array = cs;
		foreach (PartyModeColor obj in array)
		{
			obj.initialColor = obj.setInitialColor();
		}
		PartyLight[] array2 = ls;
		foreach (PartyLight obj2 in array2)
		{
			obj2.initialLightColor = obj2.light.color;
			obj2.initialLightCookieSize = obj2.light.cookieSize;
			obj2.initialLightIntensity = obj2.light.intensity;
		}
	}

	public void ForceLighting(GameState.GameMode toMode)
	{
		TreehouseModeTheme treehouseModeTheme = Themes[(int)toMode];
		discoBallAnimator.SetBool("Deploy", treehouseModeTheme.ShowDiscoBall);
		cs[0].targetColor = treehouseModeTheme.SkyGradient;
		cs[1].targetColor = treehouseModeTheme.Clouds;
		cs[2].targetColor = treehouseModeTheme.GrassFront;
		cs[3].targetColor = treehouseModeTheme.GrassMid;
		cs[4].targetColor = treehouseModeTheme.GrassBack;
		cs[5].targetColor = treehouseModeTheme.Branches;
		cs[6].targetColor = treehouseModeTheme.FoliageFront;
		cs[7].targetColor = treehouseModeTheme.FoliageBack;
		cs[8].targetColor = treehouseModeTheme.Sunrise;
		cs[9].targetColor = treehouseModeTheme.PartyButton;
		cs[10].targetColor = treehouseModeTheme.Stars;
		cs[11].targetColor = treehouseModeTheme.LevelCountSign;
		cs[12].targetColor = treehouseModeTheme.HandicapSign;
		cs[13].targetColor = treehouseModeTheme.TreehousePlatforms;
		cs[14].targetColor = treehouseModeTheme.ChallengeModeDecorA;
		cs[15].targetColor = treehouseModeTheme.ChallengeModeDecorB;
		cs[16].targetColor = treehouseModeTheme.ChallengeModeDecorC;
		cs[17].targetColor = treehouseModeTheme.UnderGroundDarkDirt;
		cs[18].targetColor = treehouseModeTheme.UnderGroundWall;
		cs[19].targetColor = treehouseModeTheme.UnderGroundRoots;
		cs[20].targetColor = treehouseModeTheme.ChallengeModeLightBlankBehind;
		ls[0].targetLightIntensity = treehouseModeTheme.WorldLightIntensity;
		ls[0].targetLightColor = treehouseModeTheme.WorldLightColour;
		ls[0].targetlightCookieSize = treehouseModeTheme.WorldLightCookieSize;
		ls[1].targetLightIntensity = treehouseModeTheme.DiscoLight1Intensity;
		ls[1].targetLightColor = treehouseModeTheme.DiscoLight1Colour;
		ls[1].targetlightCookieSize = treehouseModeTheme.DiscoLight1CookieSize;
		ls[2].targetLightIntensity = treehouseModeTheme.DiscoLight2Intensity;
		ls[2].targetLightColor = treehouseModeTheme.DiscoLight2Colour;
		ls[2].targetlightCookieSize = treehouseModeTheme.DiscoLight2CookieSize;
		for (int i = 0; i != cs.Length; i++)
		{
			cs[i].SetColorEditorMode(1f);
		}
		for (int j = 0; j != ls.Length; j++)
		{
			ls[j].setLight(1f);
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = networkMessageReceivedEvent.ReadMessage as MsgSwitchToMode;
				Debug.Log("Received message to switch to mode " + msgSwitchToMode.toMode);
				changeModes(msgSwitchToMode.toMode);
			}
		}
		if (type == typeof(LanguageChangeEvent))
		{
			if (GameSettings.GetInstance().GameMode == GameState.GameMode.CREATIVE)
			{
				textChanger.textBox.text = ScriptLocalization.InLobby.CreativeText;
			}
			else if (GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
			{
				textChanger.textBox.text = ScriptLocalization.InLobby.PartyText;
			}
			else if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
			{
				textChanger.textBox.text = ScriptLocalization.InLobby.FreePlayButtonText;
			}
		}
	}
}
