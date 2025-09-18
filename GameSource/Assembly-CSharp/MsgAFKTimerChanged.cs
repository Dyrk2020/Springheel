using UnityEngine.Networking;

public class MsgAFKTimerChanged : MessageBase
{
	public int Time;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)Time);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Time = (int)reader.ReadPackedUInt32();
	}
}
