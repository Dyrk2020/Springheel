using UnityEngine.Networking;

public class MsgPointAwarded : MessageBase
{
	public int PlayerNumber;

	public PointBlock.pointBlockType PointType;

	public bool AlwaysAward;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNumber);
		writer.Write((int)PointType);
		writer.Write(AlwaysAward);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNumber = (int)reader.ReadPackedUInt32();
		PointType = (PointBlock.pointBlockType)reader.ReadInt32();
		AlwaysAward = reader.ReadBoolean();
	}
}
