using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UCHServices;
using UnityEngine;
using UnityEngine.Networking;

public class RelayConstants
{
	public const bool IS_TESTING_LOCALLY = false;

	public const int TEST_RELAY_REGION_ID = -1;

	private static List<AvailableRegion> availableRegions = new List<AvailableRegion>();

	private static DynamicConfig dynamicConfig;

	public const int CLIENT_PORT = 17778;

	public const int TRANSPORT_ID = 1;

	public const int MAX_MESSAGE_SIZE = 8192;

	public const string CRYPTO_KEY = "McQfTjWnZr4u7x!A%D*G-KaPdRgUkXp2";

	public static string SERVER_ADDRESS => GameSettings.GetInstance().RelayServerConnectionData.ip;

	public static List<AvailableRegion> AVAILABLE_REGIONS => availableRegions;

	public static DynamicConfig DYNAMIC_CONFIG => dynamicConfig;

	public static int SERVER_PORT => GameSettings.GetInstance().RelayServerConnectionData.port;

	public static async UniTask PopulateAvailableRegions()
	{
		if (availableRegions.Count <= 0)
		{
			AvailableRegionsResponse availableRegionsResponse = await UCHOnlineConnector.Service.GetAvailableRegions();
			availableRegions.AddRange(availableRegionsResponse.availableRegions);
		}
	}

	public static async UniTask LoadDynamicConfigs()
	{
		if (DYNAMIC_CONFIG == null)
		{
			dynamicConfig = (await UCHOnlineConnector.Service.GetDynamicConfig()).dynamicConfig;
		}
	}

	private static async UniTask TestForDNSAvailability(List<AvailableRegion> regions)
	{
		try
		{
			AvailableRegion availableRegion = regions[0];
			Debug.Log("Testing main DNS availability " + availableRegion.queryAddress);
			await UnityWebRequest.Get("https://" + availableRegion.queryAddress + "/health/check").SendWebRequest();
			Debug.Log("Main DNS available.");
		}
		catch (UnityWebRequestException ex)
		{
			Debug.LogError("Error calling test DNS changing to fallback DNS\n" + ex.Message + "\n" + ex.StackTrace);
			foreach (AvailableRegion region in regions)
			{
				region.queryAddress = region.queryAddress.Replace(".clevendeav.net", ".ultimatechickenhorseserver.com");
			}
			UCHOnlineConnector.Service.Settings.connectionPort = 444;
			try
			{
				AvailableRegion availableRegion2 = regions[0];
				Debug.Log("Testing fallback DNS availability " + availableRegion2.queryAddress);
				await UnityWebRequest.Get("https://" + availableRegion2.queryAddress + ":444/health/check").SendWebRequest();
				Debug.Log("Fallback DNS working properly.");
			}
			catch (UnityWebRequestException ex2)
			{
				Debug.LogError("Fallback DNS not working either\n" + ex2.Message + "\n" + ex2.StackTrace);
			}
		}
	}

	public static AvailableRegion FindRegionById(string regionId)
	{
		return availableRegions.Find((AvailableRegion x) => x.id == regionId);
	}
}
