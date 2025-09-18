using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GameSparks.Core;

public static class GameSparksUtil
{
	public static Action<string> LogMessageHandler;

	public static bool ShouldConnect => true;

	public static string MakeHmac(string strToHmac, string secret)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(secret);
		byte[] bytes2 = uTF8Encoding.GetBytes(strToHmac);
		using HMACSHA256 hMACSHA = new HMACSHA256(bytes);
		byte[] inArray = hMACSHA.ComputeHash(bytes2);
		return Convert.ToBase64String(inArray);
	}

	public static void CompleteStream(Stream stream)
	{
		stream.Close();
	}

	internal static void LogError(string p)
	{
		Write("Error: " + p);
	}

	internal static void Log(string p)
	{
		Write("Log: " + p);
	}

	internal static void LogException(Exception e)
	{
		Write("Exception: " + e.ToString());
	}

	private static void Write(string p)
	{
		if (LogMessageHandler != null)
		{
			LogMessageHandler("GSUtil: " + p);
		}
		Debugger.Log(0, "GSUtil", p);
	}
}
