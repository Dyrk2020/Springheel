using System;

public class ThreadJob
{
	public Action runFunc;

	public Action onFinish;

	public ThreadJob(Action runFunc, Action onFinish = null)
	{
		this.runFunc = runFunc;
		this.onFinish = onFinish;
	}

	public void Run()
	{
		if (runFunc != null)
		{
			runFunc();
		}
	}

	public void OnFinish()
	{
		if (onFinish != null)
		{
			onFinish();
		}
	}
}
