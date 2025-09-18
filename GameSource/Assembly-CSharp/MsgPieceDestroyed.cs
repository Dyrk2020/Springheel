using UnityEngine.Networking;

public class MsgPieceDestroyed : MessageBase
{
	public int BlockID;

	public int SceneLoadNumber;

	public int MachineNetworkNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)BlockID);
		writer.WritePackedUInt32((uint)SceneLoadNumber);
		writer.WritePackedUInt32((uint)MachineNetworkNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		BlockID = (int)reader.ReadPackedUInt32();
		SceneLoadNumber = (int)reader.ReadPackedUInt32();
		MachineNetworkNumber = (int)reader.ReadPackedUInt32();
	}
}
