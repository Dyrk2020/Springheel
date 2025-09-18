using UnityEngine.Networking;

public class MsgThwompTriggered : MessageBase
{
	public int ThwompID;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)ThwompID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		ThwompID = (int)reader.ReadPackedUInt32();
	}
}
