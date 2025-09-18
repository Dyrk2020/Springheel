using UnityEngine.Networking;

public class MsgCharacterPicked : MessageBase
{
	public Character.Animals Animal;

	public int PlayerNetworkNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)Animal);
		writer.WritePackedUInt32((uint)PlayerNetworkNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Animal = (Character.Animals)reader.ReadInt32();
		PlayerNetworkNumber = (int)reader.ReadPackedUInt32();
	}
}
