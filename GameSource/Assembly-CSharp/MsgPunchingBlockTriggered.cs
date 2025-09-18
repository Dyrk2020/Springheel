using UnityEngine.Networking;

public class MsgPunchingBlockTriggered : MessageBase
{
	public int blockID;

	public int playerNumber;

	public int hitTriggerMask;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)blockID);
		writer.WritePackedUInt32((uint)playerNumber);
		writer.WritePackedUInt32((uint)hitTriggerMask);
	}

	public override void Deserialize(NetworkReader reader)
	{
		blockID = (int)reader.ReadPackedUInt32();
		playerNumber = (int)reader.ReadPackedUInt32();
		hitTriggerMask = (int)reader.ReadPackedUInt32();
	}
}
