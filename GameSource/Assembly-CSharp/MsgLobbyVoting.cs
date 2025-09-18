using UnityEngine.Networking;

public class MsgLobbyVoting : MessageBase
{
	public bool VoteStarted;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(VoteStarted);
	}

	public override void Deserialize(NetworkReader reader)
	{
		VoteStarted = reader.ReadBoolean();
	}
}
