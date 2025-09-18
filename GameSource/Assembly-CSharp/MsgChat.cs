using UnityEngine.Networking;

public class MsgChat : MessageBase
{
	public int NetworkPlayerNumber;

	public EmoteMeanings EmoteType;

	public string MessageText;

	public bool isChatMessage;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write((int)EmoteType);
		writer.Write(MessageText);
		writer.Write(isChatMessage);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		EmoteType = (EmoteMeanings)reader.ReadInt32();
		MessageText = reader.ReadString();
		isChatMessage = reader.ReadBoolean();
	}
}
