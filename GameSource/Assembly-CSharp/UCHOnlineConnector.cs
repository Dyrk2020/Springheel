using UCHServices;
using UnityEngine;

public class UCHOnlineConnector : MonoBehaviour
{
	private static UCHOnlineConnector mInstance;

	private Service mHttpService;

	public static UCHOnlineConnector Instance => mInstance;

	public static Service Service => Instance.mHttpService;

	private void Awake()
	{
		mInstance = this;
		mHttpService = new Service();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnDestroy()
	{
		Debug.Log("UCHOnlineConnector OnDestroy");
		Cleanup();
	}

	public static void Cleanup()
	{
		Service.Cleanup();
	}
}
