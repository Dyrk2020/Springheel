using System;

namespace GameSparks.Core;

public interface IQueueReader : IDisposable
{
	string ReadFully();

	void Initialize(string fileName);
}
