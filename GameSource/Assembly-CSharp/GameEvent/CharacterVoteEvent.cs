using UnityEngine.Networking;

namespace GameEvent;

public class CharacterVoteEvent : GameEvent
{
	public readonly bool IsVoting;

	public readonly NetworkInstanceId PlayerObjectId;

	public readonly NetworkInstanceId CharacterObjectId;

	public CharacterVoteEvent(bool isVoting, NetworkInstanceId playerObjectId, NetworkInstanceId characterObjectId)
	{
		IsVoting = isVoting;
		PlayerObjectId = playerObjectId;
		CharacterObjectId = characterObjectId;
	}
}
