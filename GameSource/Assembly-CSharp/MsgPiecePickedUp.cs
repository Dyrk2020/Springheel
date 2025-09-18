using UnityEngine.Networking;

public class MsgPiecePickedUp : MessageBase
{
	public int PlayerNumber;

	public int PieceID;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNumber);
		writer.WritePackedUInt32((uint)PieceID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNumber = (int)reader.ReadPackedUInt32();
		PieceID = (int)reader.ReadPackedUInt32();
	}
}
