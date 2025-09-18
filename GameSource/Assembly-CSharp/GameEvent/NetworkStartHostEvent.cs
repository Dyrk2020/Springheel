using UnityEngine.Networking;

namespace GameEvent;

public class NetworkStartHostEvent : GameEvent
{
	public readonly NetworkConnection Connection;

	public NetworkStartHostEvent(NetworkConnection connection)
	{
		Connection = connection;
	}
}
