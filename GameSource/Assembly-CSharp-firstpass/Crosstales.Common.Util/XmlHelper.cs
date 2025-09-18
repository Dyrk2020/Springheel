using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Crosstales.Common.Util;

public abstract class XmlHelper
{
	public static void SerializeToFile<T>(T obj, string filename)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		try
		{
			FileHelper.WriteAllText(filename, SerializeToString(obj));
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not serialize the object to a file: {arg}");
		}
	}

	public static string SerializeToString<T>(T obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		byte[] array = SerializeToByteArray(obj);
		if (array == null)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(array).Trim('\ufeff', '\u200b');
	}

	public static byte[] SerializeToByteArray<T>(T obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		try
		{
			MemoryStream w = new MemoryStream();
			XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
			XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8);
			xmlSerializer.Serialize(xmlTextWriter, obj);
			return ((MemoryStream)xmlTextWriter.BaseStream).ToArray();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not serialize the object to a byte-array: {arg}");
		}
		return null;
	}

	public static T DeserializeFromFile<T>(string filename, bool skipBOM = false)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		try
		{
			if (FileHelper.ExistsFile(filename))
			{
				string text = FileHelper.ReadAllText(filename);
				if (!string.IsNullOrEmpty(text))
				{
					return DeserializeFromString<T>(text, skipBOM);
				}
				Debug.LogWarning("Data was null: " + filename);
			}
			else
			{
				Debug.LogError("File does not exist: " + filename);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not deserialize the object from a file: {arg}");
		}
		return default(T);
	}

	public static T DeserializeFromString<T>(string xmlAsString, bool skipBOM = true)
	{
		if (string.IsNullOrEmpty(xmlAsString))
		{
			throw new ArgumentNullException("xmlAsString");
		}
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using StringReader stringReader = new StringReader(xmlAsString.Trim());
			if (skipBOM)
			{
				stringReader.Read();
			}
			return (T)xmlSerializer.Deserialize(stringReader);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not deserialize the object from a string: {arg}");
		}
		return default(T);
	}

	public static T DeserializeFromByteArray<T>(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			MemoryStream stream = new MemoryStream(data);
			return (T)xmlSerializer.Deserialize(stream);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Could not deserialize the object from a byte-array: {arg}");
		}
		return default(T);
	}

	public static T DeserializeFromResource<T>(string resourceName, bool skipBOM = true)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentNullException("resourceName");
		}
		TextAsset textAsset = Resources.Load(resourceName) as TextAsset;
		if (!(textAsset != null))
		{
			return default(T);
		}
		return DeserializeFromString<T>(textAsset.text, skipBOM);
	}
}
