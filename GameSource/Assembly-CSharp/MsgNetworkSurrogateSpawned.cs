using UnityEngine.Networking;

public class MsgNetworkSurrogateSpawned : MessageBase
{
	public NetworkInstanceId NetSurrogateID;

	public int SpawnedForPieceID;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(NetSurrogateID);
		writer.WritePackedUInt32((uint)SpawnedForPieceID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetSurrogateID = reader.ReadNetworkId();
		SpawnedForPieceID = (int)reader.ReadPackedUInt32();
	}
}
