using System;
using System.Collections.Generic;
using BCGSComponents;
using UnityEngine;

public class BCGSController : MonoBehaviour
{
	private Queue<Action> callQueue = new Queue<Action>();

	private float nextCallTime;

	[SerializeField]
	private float delayForTimeUncriticalCalls = 0.15f;

	private void Start()
	{
		Debug.Log("Setting Up BCGS Controller...");
		nextCallTime = Time.time;
	}

	public void SendRequestToQueue(BCGSTypedRequest<LogEventRequest, LogEventResponse> request, Action<LogEventResponse> onResponse)
	{
		Debug.Log("BCGSController: SendRequestToQueue: ");
		if (!BCGSInstance.Instance().Available || !BCGSInstance.Instance().Authenticated)
		{
			Debug.LogWarning("BCGSController: SendRequestToQueue: BrainCloud Not Ready...");
			return;
		}
		callQueue.Enqueue(delegate
		{
			request.Send(delegate(LogEventResponse response)
			{
				onResponse(response);
			});
		});
	}

	private void Update()
	{
		if (nextCallTime < Time.time && callQueue.Count > 0 && BCGSInstance.Instance().Available && BCGSInstance.Instance().Authenticated && !BCGSInstance.Instance().IsWorking)
		{
			nextCallTime = Time.time + delayForTimeUncriticalCalls;
			callQueue.Dequeue()();
		}
	}
}
