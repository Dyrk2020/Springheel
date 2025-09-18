using UnityEngine.Networking;

public class MsgPlayerHandicapSet : MessageBase
{
	public int Handicap;

	public int NetworkPlayerNumber;

	public handicap.HandicapAction Action;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)Handicap);
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write((int)Action);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Handicap = (int)reader.ReadPackedUInt32();
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		Action = (handicap.HandicapAction)reader.ReadInt32();
	}
}
