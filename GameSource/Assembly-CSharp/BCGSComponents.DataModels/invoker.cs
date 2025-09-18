using System;
using UnityEngine;

namespace BCGSComponents.DataModels;

internal class invoker : MonoBehaviour
{
	private Action _callback;

	public void setup(Action callback, int interval)
	{
		_callback = callback;
		InvokeRepeating("invoked", interval, interval);
	}

	public void invoked()
	{
		_callback();
	}

	public void stop()
	{
	}
}
