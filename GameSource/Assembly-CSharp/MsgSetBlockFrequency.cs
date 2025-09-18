using UnityEngine.Networking;

public class MsgSetBlockFrequency : MessageBase
{
	public int blockIndex;

	public int frequency;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)blockIndex);
		writer.WritePackedUInt32((uint)frequency);
	}

	public override void Deserialize(NetworkReader reader)
	{
		blockIndex = (int)reader.ReadPackedUInt32();
		frequency = (int)reader.ReadPackedUInt32();
	}
}
