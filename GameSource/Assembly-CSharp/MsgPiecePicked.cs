using UnityEngine.Networking;

public class MsgPiecePicked : MessageBase
{
	public uint PickableNetID;

	public int PlayerNumber;

	public int PieceID;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32(PickableNetID);
		writer.WritePackedUInt32((uint)PlayerNumber);
		writer.WritePackedUInt32((uint)PieceID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PickableNetID = reader.ReadPackedUInt32();
		PlayerNumber = (int)reader.ReadPackedUInt32();
		PieceID = (int)reader.ReadPackedUInt32();
	}
}
