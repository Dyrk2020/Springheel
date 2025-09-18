using UnityEngine.Networking;

public class MsgCharacterSuccess : MessageBase
{
	public int NetworkPlayerNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
	}
}
