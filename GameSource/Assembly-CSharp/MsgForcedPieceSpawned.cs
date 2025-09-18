using UnityEngine.Networking;

public class MsgForcedPieceSpawned : MessageBase
{
	public int playerNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)playerNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		playerNumber = (int)reader.ReadPackedUInt32();
	}
}
