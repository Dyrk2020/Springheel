using MLAPI.Relay.Transports;
using UnityEngine.Networking;

public sealed class Transporter
{
	private static Transporter instance;

	public static Transporter Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Transporter();
			}
			return instance;
		}
	}

	private Transporter()
	{
	}

	public void Initialize()
	{
		int num = 1;
		if (num > 0 && NetworkManager.activeTransport != null && !NetworkManager.activeTransport.IsStarted && num == 1)
		{
			NetworkManager.activeTransport = new UnetRelayTransport();
		}
	}
}
