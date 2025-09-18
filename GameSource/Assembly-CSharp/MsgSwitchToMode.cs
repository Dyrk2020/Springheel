using UnityEngine.Networking;

public class MsgSwitchToMode : MessageBase
{
	public GameState.GameMode toMode;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)toMode);
	}

	public override void Deserialize(NetworkReader reader)
	{
		toMode = (GameState.GameMode)reader.ReadInt32();
	}
}
