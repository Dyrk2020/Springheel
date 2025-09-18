public class SwitchMatchmaker : GamesparksMatchmaker
{
	protected override void onLobbyCreated(bool success)
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.SetLobbyPlatform(MatchmakingLobby.LobbyPlatform.SWITCH);
		}
		base.onLobbyCreated(success);
	}
}
