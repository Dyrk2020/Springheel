using Unity;
using UnityEngine.Networking;

public class MsgCommunicateCharacterOutfits : MessageBase
{
	public Character.Animals Animal;

	public int[] OutfitArray;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)Animal);
		GeneratedNetworkCode._WriteArrayInt32_None(writer, OutfitArray);
	}

	public override void Deserialize(NetworkReader reader)
	{
		Animal = (Character.Animals)reader.ReadInt32();
		OutfitArray = GeneratedNetworkCode._ReadArrayInt32_None(reader);
	}
}
