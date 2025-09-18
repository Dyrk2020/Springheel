using System.Runtime.InteropServices;

namespace nn;

public static class Nn
{
	internal const string DllName = "NintendoSDKPlugin";

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_Abort")]
	public static extern void Abort(string message);

	public static void Abort(string message, params object[] args)
	{
		Abort(string.Format(message, args));
	}

	public static void Abort(bool condition, string message)
	{
		if (!condition)
		{
			Abort(message);
		}
	}

	public static void Abort(bool condition, string message, params object[] args)
	{
		if (!condition)
		{
			Abort(string.Format(message, args));
		}
	}
}
