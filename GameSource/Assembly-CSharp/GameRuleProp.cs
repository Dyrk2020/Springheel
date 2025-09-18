using GameEvent;

public class GameRuleProp : UsableProp
{
	public static InventoryBook GameRuleBook;

	public InventoryPage.PageTypes targetPage;

	public string audioString;

	protected override void Start()
	{
		base.Start();
	}

	public override bool Use(LobbyPlayer lobbyPlayer, InputEvent.InputKey usedInputKey)
	{
		if (lobbyPlayer.CharacterInstance.InMenu || hidden || GameRuleBook == null)
		{
			return false;
		}
		GameRuleBook.AddPlayer(lobbyPlayer.localNumber, lobbyPlayer.networkNumber, lobbyPlayer.LocalPlayer.UseController, lobbyPlayer.CharacterInstance.CharacterSprite);
		bool bookSoundEffect = true;
		if (!audioString.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(audioString, base.gameObject);
			bookSoundEffect = false;
		}
		GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: true, lobbyPlayer.networkNumber, bookSoundEffect));
		GameRuleBook.GotoPage(fakeVariable: true, targetPage);
		if (targetPage == InventoryPage.PageTypes.SecondComputerTerminal)
		{
			GameEventManager.SendEvent(new UndergroundComputerOpenedEvent());
		}
		return true;
	}
}
