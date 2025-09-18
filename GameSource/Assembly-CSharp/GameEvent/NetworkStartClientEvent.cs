using UnityEngine.Networking;

namespace GameEvent;

public class NetworkStartClientEvent : GameEvent
{
	public readonly NetworkConnection Connection;

	public NetworkStartClientEvent(NetworkConnection connection)
	{
		Connection = connection;
	}
}
