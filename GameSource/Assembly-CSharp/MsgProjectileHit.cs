using UnityEngine;
using UnityEngine.Networking;

public class MsgProjectileHit : MessageBase
{
	public NetworkInstanceId ProjectileID;

	public NetworkInstanceId CollidedWith;

	public Vector3 HitPosition;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write(ProjectileID);
		writer.Write(CollidedWith);
		writer.Write(HitPosition);
	}

	public override void Deserialize(NetworkReader reader)
	{
		ProjectileID = reader.ReadNetworkId();
		CollidedWith = reader.ReadNetworkId();
		HitPosition = reader.ReadVector3();
	}
}
