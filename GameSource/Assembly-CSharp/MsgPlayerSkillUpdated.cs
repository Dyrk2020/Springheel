using UnityEngine.Networking;

public class MsgPlayerSkillUpdated : MessageBase
{
	public int NetworkPlayerNumber;

	public double SkillMean;

	public double SkillStdDev;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)NetworkPlayerNumber);
		writer.Write(SkillMean);
		writer.Write(SkillStdDev);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetworkPlayerNumber = (int)reader.ReadPackedUInt32();
		SkillMean = reader.ReadDouble();
		SkillStdDev = reader.ReadDouble();
	}
}
