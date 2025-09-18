using UnityEngine.Networking;

namespace GameEvent;

public class NetworkClientDisconnectEvent : GameEvent
{
	public readonly NetworkConnection ConnectionToClient;

	public NetworkClientDisconnectEvent(NetworkConnection connection)
	{
		ConnectionToClient = connection;
	}
}
