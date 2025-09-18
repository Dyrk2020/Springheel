using UnityEngine.Networking;

public class MsgConnectionQuality : MessageBase
{
	public int NetworkPlayerNumber;

	public LobbyManager.ConnectionQuality Quality;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write((int)Quality);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		Quality = (LobbyManager.ConnectionQuality)reader.ReadInt32();
	}
}
