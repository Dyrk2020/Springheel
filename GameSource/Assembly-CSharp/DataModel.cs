using System.Collections.Generic;
using UnityEngine;

public class DataModel
{
	public Dictionary<string, object> dict = new Dictionary<string, object>();

	public bool dirty;

	public void Set<T>(string name, T value)
	{
		if (!dict.ContainsKey(name))
		{
			dict.Add(name, value);
			dirty = true;
		}
		else if (!object.Equals(dict[name], value))
		{
			dirty = true;
			dict[name] = value;
		}
	}

	public bool TryGet<T>(string name, out T value)
	{
		if (dict.TryGetValue(name, out var value2))
		{
			value = (T)value2;
			return true;
		}
		value = default(T);
		return false;
	}

	public bool TryGetInt(string name, out int value)
	{
		if (dict.TryGetValue(name, out var value2))
		{
			value = (int)value2;
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryGetBool(string name, out bool value)
	{
		if (dict.TryGetValue(name, out var value2))
		{
			value = (bool)value2;
			return true;
		}
		value = false;
		return false;
	}

	public bool GetBool(string name, bool defaultValue = false)
	{
		if (TryGetBool(name, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public int GetInt(string name, int defaultValue = 0)
	{
		if (TryGetInt(name, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public int IncrementInt(string name, int amount)
	{
		int num = GetInt(name) + amount;
		Set(name, num);
		return num;
	}

	public int IncrementIntClamped(string name, int amount, int min, int max)
	{
		int num = GetInt(name);
		num = Mathf.Clamp(num + amount, min, max);
		Set(name, num);
		return num;
	}
}
