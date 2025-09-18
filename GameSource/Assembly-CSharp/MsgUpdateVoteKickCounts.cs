using UnityEngine.Networking;

public class MsgUpdateVoteKickCounts : MessageBase
{
	public int networkNumber;

	public int votes;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)networkNumber);
		writer.WritePackedUInt32((uint)votes);
	}

	public override void Deserialize(NetworkReader reader)
	{
		networkNumber = (int)reader.ReadPackedUInt32();
		votes = (int)reader.ReadPackedUInt32();
	}
}
