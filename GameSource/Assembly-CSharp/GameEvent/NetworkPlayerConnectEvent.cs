using UnityEngine.Networking;

namespace GameEvent;

public class NetworkPlayerConnectEvent : GameEvent
{
	public readonly NetworkConnection PlayerConnection;

	public readonly int NetworkNumber;

	public NetworkPlayerConnectEvent(NetworkConnection playerConnection, int networkNumber)
	{
		PlayerConnection = playerConnection;
		NetworkNumber = networkNumber;
	}
}
