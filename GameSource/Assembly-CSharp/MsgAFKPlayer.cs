using UnityEngine.Networking;

public class MsgAFKPlayer : MessageBase
{
	public bool isAFK;

	public int PlayerNetworkNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(isAFK);
		writer.WritePackedUInt32((uint)PlayerNetworkNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		isAFK = reader.ReadBoolean();
		PlayerNetworkNumber = (int)reader.ReadPackedUInt32();
	}
}
