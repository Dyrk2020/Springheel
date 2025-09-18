using UnityEngine.Networking;

public class MsgCharacterDeath : MessageBase
{
	public int PlayerNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNumber = (int)reader.ReadPackedUInt32();
	}
}
