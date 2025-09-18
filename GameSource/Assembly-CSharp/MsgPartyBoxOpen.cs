using UnityEngine.Networking;

public class MsgPartyBoxOpen : MessageBase
{
	public bool IsOpen;

	public bool isExtraBox;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(IsOpen);
		writer.Write(isExtraBox);
	}

	public override void Deserialize(NetworkReader reader)
	{
		IsOpen = reader.ReadBoolean();
		isExtraBox = reader.ReadBoolean();
	}
}
