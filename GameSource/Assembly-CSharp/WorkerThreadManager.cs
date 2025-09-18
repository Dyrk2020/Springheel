using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class WorkerThreadManager : MonoBehaviour
{
	private bool initialized;

	public WorkerThread fileOpThread;

	public WorkerThread vfsThread;

	private ConcurrentQueue<ThreadJob> finishedJobs = new ConcurrentQueue<ThreadJob>();

	private static WorkerThreadManager instance;

	public static WorkerThreadManager Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("Worker Thread Manager").AddComponent<WorkerThreadManager>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			StopAllThreads();
		}
	}

	public void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		fileOpThread = new WorkerThread();
		fileOpThread.threadReference = new Thread(fileOpThread.DoWork);
		fileOpThread.threadReference.Start();
		if (RamFS.PlatformUsesRamFS)
		{
			vfsThread = new WorkerThread();
			vfsThread.threadReference = new Thread((ThreadStart)delegate
			{
				vfsThread.RepeatCallback(RamFS.WorkerThreadUpdate, 16);
			});
			vfsThread.threadReference.Start();
		}
		Debug.Log("Worker Thread Manager initialized.");
	}

	public void StopAllThreads()
	{
		if (fileOpThread != null)
		{
			fileOpThread.stopping = true;
			fileOpThread.threadReference.Abort();
			fileOpThread.threadReference.Join();
		}
		if (vfsThread != null)
		{
			vfsThread.stopping = true;
			vfsThread.threadReference.Abort();
			vfsThread.threadReference.Join();
		}
	}

	private void Update()
	{
		ThreadJob item;
		while (finishedJobs.TryDequeue(out item))
		{
			item.OnFinish();
		}
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.MainThreadUpdate();
		}
	}

	public void AddFinishedJob(ThreadJob job)
	{
		finishedJobs.Enqueue(job);
	}

	public void AddFileOpJob(Action runFunc, Action onFinish = null)
	{
		if (fileOpThread != null)
		{
			fileOpThread.AddJob(new ThreadJob(runFunc, onFinish));
		}
		else
		{
			Debug.LogError("Warning! Worker Thread Manager not initialized.");
		}
	}
}
