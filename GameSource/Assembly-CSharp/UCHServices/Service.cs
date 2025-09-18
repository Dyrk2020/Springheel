using System;
using System.Collections.Generic;
using System.Xml;
using Cysharp.Threading.Tasks;
using Relay;
using UnityEngine;
using UnityEngine.Networking;

namespace UCHServices;

public class Service
{
	private const bool IS_TEST_REGION = false;

	private const int PING_COUNT = 5;

	private static List<int> pings = new List<int>();

	private string mServerId = Guid.NewGuid().ToString();

	private ServiceSettings mSettings = new ServiceSettings();

	public string ServerId => mServerId;

	public ServiceSettings Settings => mSettings;

	public bool IsReady
	{
		get
		{
			if (GameSettings.GetInstance().SelectedRegion != null)
			{
				return !string.IsNullOrEmpty(GameSettings.GetInstance().SelectedRegion.id);
			}
			return false;
		}
	}

	public async UniTask<AvailableRegionsResponse> GetAvailableRegions()
	{
		AvailableRegionsResponse result = default(AvailableRegionsResponse);
		object obj;
		int num;
		UnityWebRequest listReq;
		try
		{
			bool isTestRegion = false;
			listReq = UnityWebRequest.Get("https://uch-available-regions" + (isTestRegion ? "-test" : "") + ".nyc3.digitaloceanspaces.com/?list-type=2");
			await listReq.SendWebRequest();
			List<string> list = ParseAvailableRegionsFiles(listReq.downloadHandler.text);
			AvailableRegionsResponse response = new AvailableRegionsResponse();
			foreach (string item2 in list)
			{
				Debug.Log("Found region file " + item2);
				UnityWebRequest getReq = UnityWebRequest.Get("https://uch-available-regions" + (isTestRegion ? "-test" : "") + ".nyc3.digitaloceanspaces.com/" + item2);
				await getReq.SendWebRequest();
				AvailableRegion item = JsonUtility.FromJson<AvailableRegion>(getReq.downloadHandler.text);
				response.availableRegions.Add(item);
			}
			result = response;
			return result;
		}
		catch (UnityWebRequestException ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		UnityWebRequestException arg = (UnityWebRequestException)obj;
		Debug.LogError($"Error getting available regions {arg}. Request from fallback server.");
		listReq = UnityWebRequest.Get("https://uch-regions.clevendeav.net");
		await listReq.SendWebRequest();
		AvailableRegionList availableRegionList = JsonUtility.FromJson<AvailableRegionList>(listReq.downloadHandler.text);
		return new AvailableRegionsResponse
		{
			availableRegions = availableRegionList.regions
		};
	}

	private List<string> ParseAvailableRegionsFiles(string aXml)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(aXml);
		List<string> list = new List<string>();
		foreach (XmlNode item in xmlDocument.GetElementsByTagName("Contents"))
		{
			list.Add(item["Key"].InnerText);
		}
		return list;
	}

	public async UniTask<DynamicConfigResponse> GetDynamicConfig()
	{
		DynamicConfigResponse result = default(DynamicConfigResponse);
		object obj;
		int num;
		UnityWebRequest getReq;
		try
		{
			getReq = UnityWebRequest.Get("https://uch-conf.nyc3.digitaloceanspaces.com/config.json");
			await getReq.SendWebRequest();
			Debug.Log("Received dynamic config from main server: " + getReq.downloadHandler.text);
			result = new DynamicConfigResponse
			{
				dynamicConfig = JsonUtility.FromJson<DynamicConfig>(getReq.downloadHandler.text)
			};
			return result;
		}
		catch (UnityWebRequestException ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		UnityWebRequestException arg = (UnityWebRequestException)obj;
		Debug.LogError($"Error getting available regions {arg}. Request from fallback server.");
		getReq = UnityWebRequest.Get("https://uch-regions.clevendeav.net/configs");
		await getReq.SendWebRequest();
		return new DynamicConfigResponse
		{
			dynamicConfig = JsonUtility.FromJson<DynamicConfig>(getReq.downloadHandler.text)
		};
	}

	public async UniTask<GetIpResponse> SendGetIpRequest()
	{
		GetIpResponse getIpResponse = await new GetIpRequest(this).SendAsync<GetIpResponse>();
		Debug.Log("Received GetIpRequest " + getIpResponse.Ip);
		return getIpResponse;
	}

	public async UniTask<ServerConnectionData> SendGetNextAvailableServer()
	{
		GetNextAvailableServerResponse getNextAvailableServerResponse = await new GetNextAvailableServerRequest(this).SendAsync<GetNextAvailableServerResponse>();
		Debug.Log($"Received GetNextAvailableServerResponse {getNextAvailableServerResponse}");
		return new ServerConnectionData
		{
			ip = getNextAvailableServerResponse.ServerIp,
			port = getNextAvailableServerResponse.ServerPort
		};
	}

	public async UniTask<ServerConnectionData> SendGetServerForGame(string aGameId)
	{
		GetGameServerResponse getGameServerResponse = await new GetServerForGameRequest(this, aGameId).SendAsync<GetGameServerResponse>();
		Debug.Log($"Received GetGameServerResponse {getGameServerResponse}");
		return new ServerConnectionData
		{
			ip = getGameServerResponse.ServerIp,
			port = getGameServerResponse.ServerPort
		};
	}

	public async UniTask GetServerPing(AvailableRegion aRegion)
	{
		List<int> pingsTime = new List<int>();
		for (int i = 0; i < 5; i++)
		{
			int beforeTime = (int)(Time.time * 1000f);
			await new PingServerRequest(this, aRegion.queryAddress).SendAsync<PingResponse>();
			pingsTime.Add((int)(Time.time * 1000f) - beforeTime);
		}
		pingsTime.Sort((int a, int b) => a - b);
		pingsTime.RemoveAt(pingsTime.Count - 1);
		pingsTime.RemoveAt(0);
		int num = 0;
		foreach (int item in pingsTime)
		{
			num += item;
		}
		aRegion.ping = num / pingsTime.Count;
		Debug.Log($"Region {aRegion.name} ping: {aRegion.ping}");
	}

	public void Cleanup()
	{
	}

	public override string ToString()
	{
		return "[ Service = ServerId : " + ServerId + " ]";
	}
}
