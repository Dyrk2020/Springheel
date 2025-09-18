using UnityEngine.Networking;

namespace GameEvent;

public class NetworkMessageReceivedEvent : GameEvent
{
	public readonly NetworkMessage Message;

	public readonly MessageBase ReadMessage;

	public NetworkMessageReceivedEvent(NetworkMessage message, MessageBase readMessage)
	{
		Message = message;
		ReadMessage = readMessage;
	}
}
