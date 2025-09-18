using UnityEngine.Networking;

public class MsgRulesetDirty : MessageBase
{
	public bool dirty = true;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(dirty);
	}

	public override void Deserialize(NetworkReader reader)
	{
		dirty = reader.ReadBoolean();
	}
}
