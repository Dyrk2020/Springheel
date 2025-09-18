using UnityEngine.Networking;

public class MsgProjectileDestroyed : MessageBase
{
	public int LauncherID;

	public int ProjectileNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)LauncherID);
		writer.WritePackedUInt32((uint)ProjectileNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		LauncherID = (int)reader.ReadPackedUInt32();
		ProjectileNumber = (int)reader.ReadPackedUInt32();
	}
}
