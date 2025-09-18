using UnityEngine.Networking;

public class MsgSetCustomBackground : MessageBase
{
	public BackgroundType newBackground;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)newBackground);
	}

	public override void Deserialize(NetworkReader reader)
	{
		newBackground = (BackgroundType)reader.ReadInt32();
	}
}
