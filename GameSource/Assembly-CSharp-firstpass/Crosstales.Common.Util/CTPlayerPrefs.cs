using System;
using System.Globalization;
using UnityEngine;

namespace Crosstales.Common.Util;

public abstract class CTPlayerPrefs
{
	public static bool HasKey(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			return PlayerPrefs.HasKey(key);
		}
		return false;
	}

	public static void DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}

	public static void DeleteKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		PlayerPrefs.DeleteKey(key);
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}

	public static string GetString(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		return PlayerPrefs.GetString(key);
	}

	public static float GetFloat(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		return PlayerPrefs.GetFloat(key);
	}

	public static int GetInt(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		return PlayerPrefs.GetInt(key);
	}

	public static bool GetBool(string key)
	{
		return "true".CTEquals(GetString(key));
	}

	public static DateTime GetDate(string key)
	{
		DateTime.TryParseExact(GetString(key), "yyyyMMddHHmmsss", null, DateTimeStyles.None, out var result);
		return result;
	}

	public static Vector2 GetVector2(string key)
	{
		string[] array = GetString(key).Split(';');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		return new Vector2(x, y);
	}

	public static Vector3 GetVector3(string key)
	{
		string[] array = GetString(key).Split(';');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		float z = float.Parse(array[2]);
		return new Vector3(x, y, z);
	}

	public static Vector4 GetVector4(string key)
	{
		string[] array = GetString(key).Split(';');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		float z = float.Parse(array[2]);
		float w = float.Parse(array[3]);
		return new Vector4(x, y, z, w);
	}

	public static Quaternion GetQuaternion(string key)
	{
		return GetVector4(key).CTQuaternion();
	}

	public static Color GetColor(string key)
	{
		return GetVector4(key).CTColorRGBA();
	}

	public static SystemLanguage GetLanguage(string key)
	{
		return (SystemLanguage)Enum.Parse(typeof(SystemLanguage), GetString(key));
	}

	public static void SetString(string key, string value)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		PlayerPrefs.SetString(key, value);
	}

	public static void SetFloat(string key, float value)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		PlayerPrefs.SetFloat(key, value);
	}

	public static void SetInt(string key, int value)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		PlayerPrefs.SetInt(key, value);
	}

	public static void SetBool(string key, bool value)
	{
		SetString(key, value ? "true" : "false");
	}

	public static void SetDate(string key, DateTime value)
	{
		SetString(key, value.ToString("yyyyMMddHHmmsss"));
	}

	public static void SetVector2(string key, Vector2 value)
	{
		SetString(key, $"{value.x};{value.y}");
	}

	public static void SetVector3(string key, Vector3 value)
	{
		SetString(key, $"{value.x};{value.y};{value.z}");
	}

	public static void SetVector4(string key, Vector4 value)
	{
		SetString(key, $"{value.x};{value.y};{value.z};{value.w}");
	}

	public static void SetQuaternion(string key, Quaternion value)
	{
		SetVector4(key, value.CTVector4());
	}

	public static void SetColor(string key, Color value)
	{
		SetVector4(key, value.CTVector4());
	}

	public static void SetLanguage(string key, SystemLanguage language)
	{
		SetString(key, language.ToString());
	}
}
