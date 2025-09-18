using UnityEngine.Networking;

public class MsgSetCustomAmbience : MessageBase
{
	public GameState.LevelName newLevelAmbiance;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)newLevelAmbiance);
	}

	public override void Deserialize(NetworkReader reader)
	{
		newLevelAmbiance = (GameState.LevelName)reader.ReadInt32();
	}
}
