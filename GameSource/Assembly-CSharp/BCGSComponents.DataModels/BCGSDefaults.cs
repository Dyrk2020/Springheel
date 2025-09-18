using UnityEngine;

namespace BCGSComponents.DataModels;

public static class BCGSDefaults
{
	private static BrainCloudWrapper bc;

	private static GameObject go;

	private static bool _shouldConnect = true;

	private static BCGSInstance _SNInstance;

	internal static BrainCloudWrapper _bc => bcSetup();

	public static string PersistentDataPath { get; set; }

	public static GameObject GO => go;

	internal static int RetryBase => 2000;

	internal static int RetryMax => 60000;

	internal static int RequestTimeout => 15000;

	internal static int DurableConcurrentRequests => 1;

	internal static int DurableDrainInterval => 100;

	internal static int HandshakeOffset => 2000;

	internal static bool ShouldConnect
	{
		get
		{
			return _shouldConnect;
		}
		set
		{
			_shouldConnect = value;
		}
	}

	internal static BCGSInstance Instance
	{
		get
		{
			return _SNInstance;
		}
		set
		{
			_SNInstance = value;
		}
	}

	public static string UserId { get; internal set; }

	private static BrainCloudWrapper bcSetup()
	{
		if (bc == null)
		{
			bc = Object.FindObjectOfType<BrainCloudWrapper>();
			if (bc == null)
			{
				go = new GameObject();
				bc = go.AddComponent<BrainCloudWrapper>();
			}
			bc.Init();
			bc.AuthenticateAnonymous();
		}
		return bc;
	}
}
