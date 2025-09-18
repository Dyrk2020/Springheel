using System;
using System.IO;
using System.Text;

namespace GameSparks.Core;

public class QueueReader : IQueueReader, IDisposable
{
	private StreamReader sr;

	private string fileName;

	public void Initialize(string fileName)
	{
		this.fileName = fileName;
		if (File.Exists(fileName))
		{
			sr = File.OpenText(fileName);
		}
	}

	public void Dispose()
	{
		if (sr != null)
		{
			sr.Close();
			sr.Dispose();
		}
	}

	public string ReadFully()
	{
		if (sr == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = null;
		do
		{
			text = sr.ReadLine();
			if (text != null)
			{
				stringBuilder.AppendLine(text);
			}
		}
		while (text != null);
		Dispose();
		return stringBuilder.ToString();
	}
}
