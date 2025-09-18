using UnityEngine;
using UnityEngine.Networking;

public class MsgBookPiecePicked : MessageBase
{
	public int pieceNumber;

	public int NetworkPlayerNumber;

	public bool SetTransform;

	public Vector3 PiecePosition;

	public Vector3 PieceScale;

	public Quaternion PieceRotation;

	public int PieceID;

	public Placeable.RotationDirections PieceRotationDirection;

	public bool canSetCustomColor;

	public Color customColor;

	public int damageLevel;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)pieceNumber);
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write(SetTransform);
		writer.Write(PiecePosition);
		writer.Write(PieceScale);
		writer.Write(PieceRotation);
		writer.WritePackedUInt32((uint)PieceID);
		writer.Write((int)PieceRotationDirection);
		writer.Write(canSetCustomColor);
		writer.Write(customColor);
		writer.WritePackedUInt32((uint)damageLevel);
	}

	public override void Deserialize(NetworkReader reader)
	{
		pieceNumber = (int)reader.ReadPackedUInt32();
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		SetTransform = reader.ReadBoolean();
		PiecePosition = reader.ReadVector3();
		PieceScale = reader.ReadVector3();
		PieceRotation = reader.ReadQuaternion();
		PieceID = (int)reader.ReadPackedUInt32();
		PieceRotationDirection = (Placeable.RotationDirections)reader.ReadInt32();
		canSetCustomColor = reader.ReadBoolean();
		customColor = reader.ReadColor();
		damageLevel = (int)reader.ReadPackedUInt32();
	}
}
