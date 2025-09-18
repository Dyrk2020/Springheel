using UnityEngine;

namespace GameEvent;

public class LobbyPlayerCreatedEvent : GameEvent
{
	public readonly GameObject LobbyPlayerObj;

	public LobbyPlayerCreatedEvent(GameObject lobbyPlayerObj)
	{
		LobbyPlayerObj = lobbyPlayerObj;
	}
}
