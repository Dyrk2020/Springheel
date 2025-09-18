using System;
using System.Threading;
using System.Timers;

namespace GameSparks.Core;

public class GameSparksTimer : IGameSparksTimer
{
	private System.Timers.Timer m_timer;

	private Action m_callback;

	public void Initialize(int interval, Action callback)
	{
		m_callback = callback;
		m_timer = new System.Timers.Timer(interval);
		m_timer.Elapsed += TimerCallback;
		m_timer.AutoReset = true;
		m_timer.Start();
	}

	private void TimerCallback(object state, ElapsedEventArgs e)
	{
		if (!Monitor.TryEnter(m_timer))
		{
			return;
		}
		try
		{
			m_callback();
		}
		catch
		{
		}
		finally
		{
			Monitor.Exit(m_timer);
		}
	}

	public void Trigger()
	{
	}

	public void Stop()
	{
		lock (m_timer)
		{
			m_timer.Stop();
		}
	}
}
