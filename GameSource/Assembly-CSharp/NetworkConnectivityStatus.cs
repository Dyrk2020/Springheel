using UnityEngine;

public static class NetworkConnectivityStatus
{
	public static bool Connected => Application.internetReachability != NetworkReachability.NotReachable;
}
