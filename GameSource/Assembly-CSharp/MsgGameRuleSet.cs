using UnityEngine.Networking;

public class MsgGameRuleSet : MessageBase
{
	public TabletRule NewRule;

	public int Value;

	public int Value2;

	public bool Valueb;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)NewRule);
		writer.WritePackedUInt32((uint)Value);
		writer.WritePackedUInt32((uint)Value2);
		writer.Write(Valueb);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NewRule = (TabletRule)reader.ReadInt32();
		Value = (int)reader.ReadPackedUInt32();
		Value2 = (int)reader.ReadPackedUInt32();
		Valueb = reader.ReadBoolean();
	}
}
