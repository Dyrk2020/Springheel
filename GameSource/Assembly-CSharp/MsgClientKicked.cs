using UnityEngine.Networking;

public class MsgClientKicked : MessageBase
{
	public int NetworkPlayerNumber;

	public LobbyManager.KickReasons kickReason;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write((int)kickReason);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		kickReason = (LobbyManager.KickReasons)reader.ReadInt32();
	}
}
