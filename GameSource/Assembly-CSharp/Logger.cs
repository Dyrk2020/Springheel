using System;
using UnityEngine;

public static class Logger
{
	private static bool isLogsEnabled = true;

	public const ConsoleColor SUCCESS_COLOR = ConsoleColor.Green;

	public const ConsoleColor FAILURE_COLOR = ConsoleColor.Red;

	public static void Log(string log, string tag = null, ConsoleColor consoleColor = ConsoleColor.White)
	{
		if (isLogsEnabled && !string.IsNullOrEmpty(log))
		{
			if (!string.IsNullOrEmpty(tag))
			{
				log = $"[{tag}] {log}";
			}
			log = $"[{DateTime.Now.ToString()}] {log}";
			Debug.Log(log);
		}
	}
}
