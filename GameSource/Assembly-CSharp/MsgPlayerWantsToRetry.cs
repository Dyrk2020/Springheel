using UnityEngine.Networking;

public class MsgPlayerWantsToRetry : MessageBase
{
	public int networkNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)networkNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		networkNumber = (int)reader.ReadPackedUInt32();
	}
}
