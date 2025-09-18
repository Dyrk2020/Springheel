using UnityEngine.Networking;

public class MsgPortalHasUnlock : MessageBase
{
	public int PlayerNetworkNumber;

	public GameState.LevelName LevelWithUnlock;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlayerNetworkNumber);
		writer.Write((int)LevelWithUnlock);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlayerNetworkNumber = (int)reader.ReadPackedUInt32();
		LevelWithUnlock = (GameState.LevelName)reader.ReadInt32();
	}
}
