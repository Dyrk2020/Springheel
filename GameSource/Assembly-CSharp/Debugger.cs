using System;
using System.IO;
using UnityEngine;

public class Debugger : MonoBehaviour
{
	private StreamWriter logFile;

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
		int num = 1;
		string path = "Log " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ((num > 1) ? ("(" + num + ")") : "") + ".txt";
		try
		{
			if (File.Exists(path))
			{
				logFile = File.AppendText(path);
			}
			else
			{
				logFile = File.CreateText(path);
			}
		}
		catch (IOException ex)
		{
			num++;
			Debug.LogError(ex.Message);
		}
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
		logFile.Close();
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (logFile != null)
		{
			logFile.Write("[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "] ");
			logFile.Write(" " + type.ToString() + ": ");
			logFile.Write(logString + "\r\n");
			if (type == LogType.Error || type == LogType.Exception)
			{
				logFile.WriteLine("\t Stack trace: \r\n\t" + stackTrace.Replace("\n", "\n\t"));
			}
			logFile.Flush();
		}
	}
}
