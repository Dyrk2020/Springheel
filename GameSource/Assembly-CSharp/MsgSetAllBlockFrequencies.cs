using UnityEngine.Networking;

public class MsgSetAllBlockFrequencies : MessageBase
{
	public int frequency;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)frequency);
	}

	public override void Deserialize(NetworkReader reader)
	{
		frequency = (int)reader.ReadPackedUInt32();
	}
}
