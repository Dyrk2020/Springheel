using UnityEngine.Networking;

public class MsgNetworkClientDisconnected : MessageBase
{
	public int PlayerNetworkNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNetworkNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNetworkNumber = (int)reader.ReadPackedUInt32();
	}
}
