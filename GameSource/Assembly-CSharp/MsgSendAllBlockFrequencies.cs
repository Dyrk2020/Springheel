using Unity;
using UnityEngine.Networking;

public class MsgSendAllBlockFrequencies : MessageBase
{
	public int[] frequencies;

	public override void Serialize(NetworkWriter writer)
	{
		GeneratedNetworkCode._WriteArrayInt32_None(writer, frequencies);
	}

	public override void Deserialize(NetworkReader reader)
	{
		frequencies = GeneratedNetworkCode._ReadArrayInt32_None(reader);
	}
}
