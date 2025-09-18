using UnityEngine.Networking;

public class MsgVoteToKick : MessageBase
{
	public int NetworkPlayerToKick;

	public int NetworkPlayerVoting;

	public bool VoteToKick;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerToKick);
		writer.WritePackedUInt32((uint)NetworkPlayerVoting);
		writer.Write(VoteToKick);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerToKick = (int)reader.ReadPackedUInt32();
		NetworkPlayerVoting = (int)reader.ReadPackedUInt32();
		VoteToKick = reader.ReadBoolean();
	}
}
