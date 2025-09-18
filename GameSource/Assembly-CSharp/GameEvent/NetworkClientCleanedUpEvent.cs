using UnityEngine.Networking;

namespace GameEvent;

public class NetworkClientCleanedUpEvent : GameEvent
{
	public readonly NetworkConnection ConnectionToClient;

	public NetworkClientCleanedUpEvent(NetworkConnection connection)
	{
		ConnectionToClient = connection;
	}
}
