using MLAPI.Relay.Transports;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkDebugDataUI : MonoBehaviour
{
	public Text totalBytesSentTxt;

	public Text bytesPerSecondTxt;

	public Text totalBytesReceivedTxt;

	private float mTimeBetweenBytesPerSecondUpdate;

	private ulong mLastBytesCount;

	private UnetRelayTransport Transport => NetworkManager.activeTransport as UnetRelayTransport;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (!(NetworkManager.singleton == null))
		{
			UpdateTotalBytesSent();
			UpdateTotalBytesReceived();
			UpdateBytesPerSecond();
		}
	}

	private void UpdateTotalBytesSent()
	{
		totalBytesSentTxt.text = Transport.TotalSentBytes.ToString();
	}

	private void UpdateTotalBytesReceived()
	{
		totalBytesReceivedTxt.text = Transport.TotalReceivedBytes.ToString();
	}

	private void UpdateBytesPerSecond()
	{
		mTimeBetweenBytesPerSecondUpdate += Time.deltaTime;
		if (!(mTimeBetweenBytesPerSecondUpdate < 1f))
		{
			mTimeBetweenBytesPerSecondUpdate = 0f;
			ulong totalSentBytes = Transport.TotalSentBytes;
			ulong num = totalSentBytes - mLastBytesCount;
			bytesPerSecondTxt.text = num.ToString();
			mLastBytesCount = totalSentBytes;
		}
	}
}
