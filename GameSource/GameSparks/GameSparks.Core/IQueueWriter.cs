using System;

namespace GameSparks.Core;

public interface IQueueWriter : IDisposable
{
	void WriteLine(string contentToWrite);

	void Initialize(string fileName);
}
