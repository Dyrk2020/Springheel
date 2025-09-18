using UnityEngine.Networking;

public class MsgApplyRuleset : MessageBase
{
	public string rulesetXML;

	public int premadeIdx;

	public bool applyRules;

	public bool applyPoints;

	public bool applyBlocks;

	public bool applyMods;

	public bool temporary;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(rulesetXML);
		writer.WritePackedUInt32((uint)premadeIdx);
		writer.Write(applyRules);
		writer.Write(applyPoints);
		writer.Write(applyBlocks);
		writer.Write(applyMods);
		writer.Write(temporary);
	}

	public override void Deserialize(NetworkReader reader)
	{
		rulesetXML = reader.ReadString();
		premadeIdx = (int)reader.ReadPackedUInt32();
		applyRules = reader.ReadBoolean();
		applyPoints = reader.ReadBoolean();
		applyBlocks = reader.ReadBoolean();
		applyMods = reader.ReadBoolean();
		temporary = reader.ReadBoolean();
	}
}
