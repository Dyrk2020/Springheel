using UnityEngine;
using UnityEngine.Networking;

public class MsgPiecePlaced : MessageBase
{
	public int PlayerNumber;

	public Vector3 PiecePosition;

	public Vector3 PieceScale;

	public Quaternion PieceRotation;

	public int PieceID;

	public bool PieceWasMoved;

	public bool ResetPosition;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNumber);
		writer.Write(PiecePosition);
		writer.Write(PieceScale);
		writer.Write(PieceRotation);
		writer.WritePackedUInt32((uint)PieceID);
		writer.Write(PieceWasMoved);
		writer.Write(ResetPosition);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNumber = (int)reader.ReadPackedUInt32();
		PiecePosition = reader.ReadVector3();
		PieceScale = reader.ReadVector3();
		PieceRotation = reader.ReadQuaternion();
		PieceID = (int)reader.ReadPackedUInt32();
		PieceWasMoved = reader.ReadBoolean();
		ResetPosition = reader.ReadBoolean();
	}
}
