using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class IPMan
{
	public enum ADDRESSFAM
	{
		IPv4,
		IPv6
	}

	public static string GetIP(ADDRESSFAM Addfam)
	{
		string result = "";
		List<string> allIPs = GetAllIPs(Addfam, includeDetails: false);
		if (allIPs.Count > 0)
		{
			result = allIPs[allIPs.Count - 1];
		}
		return result;
	}

	public static List<string> GetAllIPs(ADDRESSFAM Addfam, bool includeDetails)
	{
		if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
		{
			return null;
		}
		List<string> list = new List<string>();
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			NetworkInterfaceType networkInterfaceType = NetworkInterfaceType.Wireless80211;
			NetworkInterfaceType networkInterfaceType2 = NetworkInterfaceType.Ethernet;
			if ((networkInterface.NetworkInterfaceType != networkInterfaceType && networkInterface.NetworkInterfaceType != networkInterfaceType2) || networkInterface.OperationalStatus != OperationalStatus.Up)
			{
				continue;
			}
			foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
			{
				switch (Addfam)
				{
				case ADDRESSFAM.IPv4:
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						string text = unicastAddress.Address.ToString();
						if (includeDetails)
						{
							text = text + "  " + networkInterface.Description.PadLeft(6) + networkInterface.NetworkInterfaceType.ToString().PadLeft(10);
						}
						list.Add(text);
					}
					break;
				case ADDRESSFAM.IPv6:
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6)
					{
						list.Add(unicastAddress.Address.ToString());
					}
					break;
				}
			}
		}
		return list;
	}

	public static string GetIP(string url)
	{
		string text = null;
		try
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			IPAddress[] hostAddresses = Dns.GetHostAddresses(url);
			stopwatch.Stop();
			if (hostAddresses.Length != 0)
			{
				text = hostAddresses[0].ToString();
				Logger.Log($"Domain resolved: {url} -> {text} in {stopwatch.Elapsed.TotalSeconds.ToString()} seconds", "IPMan", ConsoleColor.Cyan);
			}
			else
			{
				Logger.Log($"Failed to resolve domain: {url} in {stopwatch.Elapsed.TotalSeconds.ToString()} seconds", "IPMan", ConsoleColor.Red);
			}
		}
		catch (Exception ex)
		{
			Logger.Log($"Caught exception: {ex.Message}", "IPMan", ConsoleColor.Red);
		}
		return text;
	}
}
