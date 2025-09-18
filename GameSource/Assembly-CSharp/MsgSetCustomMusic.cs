using UnityEngine.Networking;

public class MsgSetCustomMusic : MessageBase
{
	public GameState.LevelName newLevelMusic;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)newLevelMusic);
	}

	public override void Deserialize(NetworkReader reader)
	{
		newLevelMusic = (GameState.LevelName)reader.ReadInt32();
	}
}
