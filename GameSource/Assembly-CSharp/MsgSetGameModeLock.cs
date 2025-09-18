using UnityEngine.Networking;

public class MsgSetGameModeLock : MessageBase
{
	public bool Locked;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(Locked);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Locked = reader.ReadBoolean();
	}
}
