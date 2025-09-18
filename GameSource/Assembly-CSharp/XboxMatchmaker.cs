public class XboxMatchmaker : UnityMatchmaker
{
	public override void CreateLobby()
	{
		base.CreateLobby();
		startingLobby = true;
		CurrentLobby = new LocalLobby();
		onLobbyCreated(success: true);
		onLobbyJoined(success: true);
	}
}
