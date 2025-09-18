using System;
using System.IO;

namespace GameSparks.Core;

public class QueueWriter : IQueueWriter, IDisposable
{
	private StreamWriter sw;

	public void Initialize(string fileName)
	{
		sw = new StreamWriter(fileName, append: false);
	}

	public void WriteLine(string line)
	{
		if (sw != null)
		{
			sw.WriteLine(line);
			sw.Flush();
		}
	}

	public void Dispose()
	{
		if (sw != null)
		{
			sw.Flush();
			sw.Close();
			sw.Dispose();
		}
	}
}
