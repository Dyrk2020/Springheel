using UnityEngine;

namespace GameEvent;

public class LobbyCursorCreatedEvent : GameEvent
{
	public readonly GameObject LobbyCursorObj;

	public LobbyCursorCreatedEvent(GameObject lobbyCursorObj)
	{
		LobbyCursorObj = lobbyCursorObj;
	}
}
