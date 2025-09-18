using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace BrainCloud.Internal;

public class RequestState
{
	internal enum eWebRequestStatus
	{
		STATUS_PENDING,
		STATUS_DONE,
		STATUS_ERROR
	}

	public long PacketId { get; set; }

	public DateTime TimeSent { get; set; }

	public int Retries { get; set; }

	public string Signature { get; set; }

	public byte[] ByteArray { get; set; }

	public UnityWebRequest WebRequest { get; set; }

	public string RequestString { get; set; }

	public List<object> MessageList { get; set; }

	public bool LoseThisPacket { get; set; }

	public bool PacketRequiresLongTimeout { get; set; }

	public bool PacketNoRetry { get; set; }

	public RequestState()
	{
		CleanupRequest();
	}

	public void CancelRequest()
	{
		try
		{
			CleanupRequest();
		}
		catch (Exception)
		{
		}
	}

	private void CleanupRequest()
	{
		if (WebRequest != null)
		{
			WebRequest.Dispose();
			WebRequest = null;
		}
	}
}
