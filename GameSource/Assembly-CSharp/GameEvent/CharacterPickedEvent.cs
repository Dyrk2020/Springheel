using UnityEngine.Networking;

namespace GameEvent;

public class CharacterPickedEvent : GameEvent
{
	public readonly Character.Animals ChosenCharacter;

	public readonly NetworkInstanceId PlayerObjectId;

	public readonly NetworkInstanceId CursorObjectId;

	public CharacterPickedEvent(Character.Animals chosenCharacter, NetworkInstanceId playerObjectId, NetworkInstanceId cursorObjectId)
	{
		ChosenCharacter = chosenCharacter;
		PlayerObjectId = playerObjectId;
		CursorObjectId = cursorObjectId;
	}
}
