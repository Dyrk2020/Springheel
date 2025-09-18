using UnityEngine.Networking;

public class MsgPointsCleared : MessageBase
{
	public bool ClearAll;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(ClearAll);
	}

	public override void Deserialize(NetworkReader reader)
	{
		ClearAll = reader.ReadBoolean();
	}
}
