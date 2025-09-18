using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class WorkerThread
{
	public string name = "Worker Thread";

	public bool stopping;

	private ConcurrentQueue<ThreadJob> jobQueue = new ConcurrentQueue<ThreadJob>();

	public Thread threadReference;

	public void DoWork()
	{
		while (!stopping)
		{
			if (jobQueue.TryDequeue(out var item))
			{
				try
				{
					item.Run();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in thread job for thread " + name + ": " + ex.Message + "\n" + ex.StackTrace);
				}
				WorkerThreadManager.Instance.AddFinishedJob(item);
			}
			else
			{
				Thread.Sleep(32);
			}
		}
	}

	public void RepeatCallback(Action repeatCallback, int sleepMsTimeout)
	{
		while (!stopping)
		{
			repeatCallback();
			Thread.Sleep(sleepMsTimeout);
		}
	}

	public void AddJob(ThreadJob job)
	{
		jobQueue.Enqueue(job);
	}
}
