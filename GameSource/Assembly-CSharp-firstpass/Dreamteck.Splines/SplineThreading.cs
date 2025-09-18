using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Dreamteck.Splines;

public static class SplineThreading
{
	public delegate void EmptyHandler();

	internal class ThreadDef
	{
		internal class Worker
		{
			internal bool computing;

			internal Queue<EmptyHandler> instructions = new Queue<EmptyHandler>();
		}

		internal delegate void BoolHandler(bool flag);

		private ParameterizedThreadStart start;

		internal Thread thread;

		private Worker worker = new Worker();

		internal bool isAlive
		{
			get
			{
				if (thread != null)
				{
					return thread.IsAlive;
				}
				return false;
			}
		}

		internal bool computing => worker.computing;

		internal ThreadDef()
		{
			start = RunThread;
		}

		internal void Queue(EmptyHandler handler)
		{
			worker.instructions.Enqueue(handler);
		}

		internal void Interrupt()
		{
			thread.Interrupt();
		}

		internal void Restart()
		{
			thread = new Thread(start);
			thread.Start(worker);
		}

		internal void Abort()
		{
			if (isAlive)
			{
				thread.Abort();
			}
		}
	}

	internal static ThreadDef[] threads;

	internal static readonly object locker;

	public static int threadCount
	{
		get
		{
			return threads.Length;
		}
		set
		{
			if (value > threads.Length)
			{
				while (threads.Length < value)
				{
					ThreadDef threadDef = new ThreadDef();
					threadDef.Restart();
					ArrayUtility.Add(ref threads, threadDef);
				}
			}
		}
	}

	static SplineThreading()
	{
		threads = new ThreadDef[2];
		locker = new object();
		Application.quitting += Quitting;
		for (int i = 0; i < threads.Length; i++)
		{
			threads[i] = new ThreadDef();
		}
	}

	private static void Quitting()
	{
		Stop();
	}

	private static void RunThread(object o)
	{
		ThreadDef.Worker worker = (ThreadDef.Worker)o;
		while (true)
		{
			try
			{
				worker.computing = false;
				Thread.Sleep(-1);
			}
			catch (ThreadInterruptedException)
			{
				worker.computing = true;
				lock (locker)
				{
					while (worker.instructions.Count > 0)
					{
						worker.instructions.Dequeue()?.Invoke();
					}
				}
			}
			catch (Exception ex2)
			{
				if (ex2.Message != "")
				{
					Debug.Log("THREAD EXCEPTION " + ex2.Message);
				}
				break;
			}
		}
		Debug.Log("Thread stopped");
		worker.computing = false;
	}

	public static void Run(EmptyHandler handler)
	{
		for (int i = 0; i < threads.Length; i++)
		{
			if (!threads[i].isAlive)
			{
				threads[i].Restart();
			}
			if (!threads[i].computing || i == threads.Length - 1)
			{
				threads[i].Queue(handler);
				if (!threads[i].computing)
				{
					threads[i].Interrupt();
				}
				break;
			}
		}
	}

	public static void PrewarmThreads()
	{
		for (int i = 0; i < threads.Length; i++)
		{
			if (!threads[i].isAlive)
			{
				threads[i].Restart();
			}
		}
	}

	public static void Stop()
	{
		for (int i = 0; i < threads.Length; i++)
		{
			threads[i].Abort();
		}
	}
}
