using System.Runtime.InteropServices;
using UnityEngine;

namespace nn;

public static class NNLog
{
	public static void Log(string message, string stacktrace, LogType logType)
	{
		logInternal(message + "\n" + stacktrace + "\n");
	}

	public static void Log(string message)
	{
		logInternal(message + "\n");
	}

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_log_log")]
	private static extern void logInternal(string message);
}
