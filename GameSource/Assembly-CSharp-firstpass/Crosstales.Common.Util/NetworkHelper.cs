using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Crosstales.Common.Util;

public abstract class NetworkHelper
{
	protected const string FILE_PREFIX = "file://";

	public static bool isInternetAvailable => Application.internetReachability != NetworkReachability.NotReachable;

	public static bool OpenURL(string url)
	{
		if (isURL(url))
		{
			openURL(url);
			return true;
		}
		Debug.LogWarning("URL was invalid: " + url);
		return false;
	}

	public static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		bool result = true;
		if (sslPolicyErrors != SslPolicyErrors.None)
		{
			foreach (X509ChainStatus item in chain.ChainStatus.Where((X509ChainStatus t) => t.Status != X509ChainStatusFlags.RevocationStatusUnknown))
			{
				_ = item;
				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
				chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
				chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
				result = chain.Build((X509Certificate2)certificate);
			}
		}
		return result;
	}

	public static string GetURLFromFile(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (!isURL(path))
			{
				return BaseConstants.PREFIX_FILE + Uri.EscapeUriString(FileHelper.ValidateFile(path).Replace('\\', '/'));
			}
			return Uri.EscapeUriString(FileHelper.ValidateFile(path).Replace('\\', '/'));
		}
		return path;
	}

	public static string ValidateURL(string url, bool removeProtocol = false, bool removeWWW = true, bool removeSlash = true)
	{
		if (isURL(url))
		{
			string text = url?.Trim().Replace('\\', '/');
			if (removeProtocol)
			{
				text = text.Substring(text.CTIndexOf("//") + 2);
			}
			if (removeWWW)
			{
				text = text.CTReplace("www.", string.Empty);
			}
			if (removeSlash && text.CTEndsWith("/"))
			{
				text = text.Substring(0, text.Length - 1);
			}
			return Uri.EscapeUriString(text);
		}
		return url;
	}

	public static bool isURL(string url)
	{
		if (!string.IsNullOrEmpty(url))
		{
			if (!url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	public static bool isIPv4(string ip)
	{
		if (!string.IsNullOrEmpty(ip) && BaseConstants.REGEX_IP_ADDRESS.IsMatch(ip))
		{
			string[] array = ip.Split('.');
			for (int i = 0; i < array.Length; i++)
			{
				if ((int.TryParse(array[i], out var result) && result > 255) || result < 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static string GetIP(string host)
	{
		string text = ValidateURL(host, isURL(host));
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				string text2 = Dns.GetHostAddresses(text)[0].ToString();
				return (text2 == "::1") ? "127.0.0.1" : text2;
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"Could not resolve host '{host}': {arg}");
			}
		}
		else
		{
			Debug.LogWarning("Host name is null or empty - can't resolve to IP!");
		}
		return host;
	}

	[Obsolete("Please use 'GetURLFromFile' instead.")]
	public static string ValidURLFromFilePath(string path)
	{
		return GetURLFromFile(path);
	}

	[Obsolete("Please use 'ValidateURL' instead.")]
	public static string CleanUrl(string url, bool removeProtocol = true, bool removeWWW = true, bool removeSlash = true)
	{
		return ValidateURL(url, removeProtocol, removeWWW, removeSlash);
	}

	[Obsolete("Please use 'isURL' instead.")]
	public static bool isValidURL(string url)
	{
		return isURL(url);
	}

	private static void openURL(string url)
	{
		Application.OpenURL(url);
	}
}
