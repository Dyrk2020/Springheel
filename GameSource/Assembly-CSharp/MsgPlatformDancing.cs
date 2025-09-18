using UnityEngine;
using UnityEngine.Networking;

public class MsgPlatformDancing : MessageBase
{
	public int PlatformID;

	public int PlayerNumber;

	public bool IsDancing;

	public bool CharacterOnPlatform;

	public Vector3 PlatformPosition;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)PlatformID);
		writer.WritePackedUInt32((uint)PlayerNumber);
		writer.Write(IsDancing);
		writer.Write(CharacterOnPlatform);
		writer.Write(PlatformPosition);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PlatformID = (int)reader.ReadPackedUInt32();
		PlayerNumber = (int)reader.ReadPackedUInt32();
		IsDancing = reader.ReadBoolean();
		CharacterOnPlatform = reader.ReadBoolean();
		PlatformPosition = reader.ReadVector3();
	}
}
