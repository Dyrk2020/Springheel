using UnityEngine.Networking;

public class MsgSetNetworkSurrogateVal : MessageBase
{
	public NetworkInstanceId NetSurrogateID;

	public static int VOID = 0;

	public static int BOOL = 1;

	public static int INT = 2;

	public static int FLOAT = 3;

	public static int STRING = 4;

	public static int TRIGGER = 5;

	public int ValueType;

	public bool BoolVal;

	public int IntVal;

	public float FloatVal;

	public string StringVal;

	public MsgSetNetworkSurrogateVal()
	{
		ValueType = VOID;
	}

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(NetSurrogateID);
		writer.WritePackedUInt32((uint)ValueType);
		writer.Write(BoolVal);
		writer.WritePackedUInt32((uint)IntVal);
		writer.Write(FloatVal);
		writer.Write(StringVal);
	}

	public override void Deserialize(NetworkReader reader)
	{
		NetSurrogateID = reader.ReadNetworkId();
		ValueType = (int)reader.ReadPackedUInt32();
		BoolVal = reader.ReadBoolean();
		IntVal = (int)reader.ReadPackedUInt32();
		FloatVal = reader.ReadSingle();
		StringVal = reader.ReadString();
	}
}
