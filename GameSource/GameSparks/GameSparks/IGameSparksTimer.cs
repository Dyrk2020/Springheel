using System;

namespace GameSparks;

public interface IGameSparksTimer
{
	void Initialize(int interval, Action callback);

	void Trigger();

	void Stop();
}
