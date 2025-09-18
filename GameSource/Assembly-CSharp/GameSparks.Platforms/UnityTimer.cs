using System;

namespace GameSparks.Platforms;

public class UnityTimer : IControlledTimer, IGameSparksTimer
{
	private Action callback;

	private int interval;

	private long elapsedTicks;

	private bool running;

	private TimerController controller;

	public void SetController(TimerController controller)
	{
		this.controller = controller;
		this.controller.AddTimer(this);
	}

	public void Initialize(int interval, Action callback)
	{
		this.callback = callback;
		this.interval = interval;
		running = true;
	}

	public void Trigger()
	{
	}

	public void Stop()
	{
		running = false;
		callback = null;
		controller.RemoveTimer(this);
	}

	public void Update(long ticks)
	{
		if (!running)
		{
			return;
		}
		elapsedTicks += ticks;
		if (elapsedTicks > interval)
		{
			elapsedTicks -= interval;
			if (callback != null)
			{
				callback();
			}
		}
	}
}
