using System;
using GameEvent;

public class PickableCustomizationButton : PickableButton
{
	public bool Defaultcolor;

	public CustomizationType customizationType;

	public BackgroundType CustomBackgroundToSet;

	public GameState.LevelName MusicFromLevel = GameState.LevelName.BLANKLEVEL;

	protected override void Start()
	{
		base.Start();
		switch (customizationType)
		{
		case CustomizationType.BlockColors:
			if (Defaultcolor)
			{
				GameEventManager.SendEvent(new SetpieceColorChangeEvent(sprite.color));
			}
			break;
		case CustomizationType.Background:
		case CustomizationType.Music:
			break;
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		switch (customizationType)
		{
		case CustomizationType.BlockColors:
			GameEventManager.SendEvent(new SetpieceColorChangeEvent(sprite.color));
			break;
		case CustomizationType.Background:
			GameEventManager.SendEvent(new SetCustomBackgroundEvent(CustomBackgroundToSet));
			break;
		case CustomizationType.Music:
			MusicFromLevel = getNextLevel(MusicFromLevel);
			GameEventManager.SendEvent(new SetCustomMusicEvent(MusicFromLevel));
			break;
		case CustomizationType.Ambience:
			MusicFromLevel = getNextLevel(MusicFromLevel);
			GameEventManager.SendEvent(new SetCustomAmbienceEvent(MusicFromLevel));
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		switch (customizationType)
		{
		case CustomizationType.Music:
			buttonText.text = LevelSelectController.GetLocalizedLevelName(MusicFromLevel);
			break;
		case CustomizationType.Ambience:
			buttonText.text = LevelSelectController.GetLocalizedLevelName(MusicFromLevel);
			break;
		case CustomizationType.BlockColors:
		case CustomizationType.Background:
			break;
		}
	}

	private GameState.LevelName getNextLevel(GameState.LevelName level)
	{
		level++;
		int i = (int)level;
		Array values = Enum.GetValues(typeof(GameState.LevelName));
		GameState.LevelName[] array = new GameState.LevelName[values.Length];
		values.CopyTo(array, 0);
		for (; i < array.Length && array[i] >= GameState.LevelName.RANDOM; i++)
		{
		}
		if (i >= array.Length)
		{
			level = GameState.LevelName.FARM;
		}
		return level;
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		if (customizationType == CustomizationType.Ambience || customizationType == CustomizationType.Music)
		{
			GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		}
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			short msgType = networkMessageReceivedEvent.Message.msgType;
			if (customizationType == CustomizationType.Music && msgType == NetMsgTypes.SetCustomMusic)
			{
				MsgSetCustomMusic msgSetCustomMusic = (MsgSetCustomMusic)networkMessageReceivedEvent.ReadMessage;
				LevelSelectController.GetLocalizedLevelName(msgSetCustomMusic.newLevelMusic);
				MusicFromLevel = msgSetCustomMusic.newLevelMusic;
			}
			else if (customizationType == CustomizationType.Ambience && msgType == NetMsgTypes.SetCustomAmbience)
			{
				MsgSetCustomAmbience msgSetCustomAmbience = (MsgSetCustomAmbience)networkMessageReceivedEvent.ReadMessage;
				LevelSelectController.GetLocalizedLevelName(msgSetCustomAmbience.newLevelAmbiance);
				MusicFromLevel = msgSetCustomAmbience.newLevelAmbiance;
			}
		}
	}
}
