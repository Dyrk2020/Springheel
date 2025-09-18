using System;
using System.IO;
using System.Text;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Networking;

namespace UCHServices;

public static class UnityWebRequestExtensions
{
	public static T GenerateResponse<T>(this UnityWebRequest www) where T : IMessage<T>, new()
	{
		string responseHeader = www.GetResponseHeader("content-type");
		if (responseHeader.Contains("application/json"))
		{
			string json = Encoding.UTF8.GetString(www.downloadHandler.data);
			return new MessageParser<T>(() => new T()).ParseJson(json);
		}
		if (responseHeader.Contains("application/x-protobuf"))
		{
			byte[] array = www.downloadHandler.data;
			if (array == null)
			{
				array = Array.Empty<byte>();
			}
			using MemoryStream input = new MemoryStream(array);
			return new MessageParser<T>(() => new T()).ParseFrom(input);
		}
		Debug.LogError("Unsupported content type received " + responseHeader);
		return default(T);
	}
}
