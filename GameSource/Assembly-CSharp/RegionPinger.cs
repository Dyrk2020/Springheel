using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UCHServices;
using UnityEngine;

public class RegionPinger
{
	private static UniTaskCompletionSource currentPingTaskCompletionSource;

	public static async UniTask PingAllRegions()
	{
		if (currentPingTaskCompletionSource != null)
		{
			Debug.Log("Pinger is already doing it's thing, wait for it to complete.");
			await currentPingTaskCompletionSource.Task;
			return;
		}
		currentPingTaskCompletionSource = new UniTaskCompletionSource();
		List<UniTask> list = new List<UniTask>();
		foreach (AvailableRegion aVAILABLE_REGION in RelayConstants.AVAILABLE_REGIONS)
		{
			if (aVAILABLE_REGION.ping == -1)
			{
				list.Add(UCHOnlineConnector.Service.GetServerPing(aVAILABLE_REGION));
			}
		}
		try
		{
			List<UniTask> list2 = new List<UniTask>();
			foreach (UniTask item in list)
			{
				list2.Add(UniTask.WhenAny(item, UniTask.Delay(5000)));
			}
			await UniTask.WhenAll(list2);
		}
		catch (Exception exception)
		{
			Debug.LogError("Error pinging regions");
			Debug.LogException(exception);
		}
		foreach (AvailableRegion item2 in RelayConstants.AVAILABLE_REGIONS.OrderBy((AvailableRegion x) => x.ping))
		{
			if (item2.ping != -1)
			{
				GameSettings.GetInstance().ClosestRegion = item2;
				GameSettings.GetInstance().SelectedRegion = item2;
				Debug.Log($"Default region set to {item2.name} with ping {item2.ping}");
				break;
			}
		}
		currentPingTaskCompletionSource.TrySetResult();
		currentPingTaskCompletionSource = null;
	}
}
