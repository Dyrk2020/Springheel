using UnityEngine.Networking;

public class MsgSetPartyPieceID : MessageBase
{
	public int NetworkPlayerNumber;

	public int PieceID;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.WritePackedUInt32((uint)PieceID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		PieceID = (int)reader.ReadPackedUInt32();
	}
}
