using System;
using System.Collections.Generic;

namespace BCGSComponents.DataModels;

public class BCGSData : IBCGSData
{
	public Dictionary<string, object> BaseData { get; set; }

	public string JSON => ToString();

	IDictionary<string, object> IBCGSData.BaseData => new Dictionary<string, object>();

	public BCGSData()
	{
		BaseData = new Dictionary<string, object>();
	}

	public BCGSData(BCGSData wrapper)
	{
		BaseData = new Dictionary<string, object>(wrapper.BaseData);
	}

	public BCGSData(IDictionary<string, object> data)
	{
		BaseData = new Dictionary<string, object>(data);
	}

	public BCGSData(string jsonString)
	{
		object obj = BCGSJson.From(jsonString);
		if (obj is IDictionary<string, object>)
		{
			BaseData = (Dictionary<string, object>)obj;
		}
	}

	public bool ContainsKey(string key)
	{
		return BaseData.ContainsKey(key);
	}

	public bool? GetBoolean(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<bool>(value, out var result))
		{
			return result;
		}
		return null;
	}

	public List<bool> GetBooleanList(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<List<bool>>(value, out var result))
		{
			return result;
		}
		return null;
	}

	[Obsolete("GetObjectList is deprecated, please use GetGSDataList instead.")]
	public List<object> GetObjectList(string name)
	{
		if (BaseData.ContainsKey(name) && BaseData[name] is List<object>)
		{
			return (List<object>)BaseData[name];
		}
		return null;
	}

	[Obsolete("GetObject is deprecated, please use GetGSData instead.")]
	public BCGSData GetObject(string name)
	{
		if (BaseData.ContainsKey(name))
		{
			object obj = BaseData[name];
			if (obj is BCGSData)
			{
				return (BCGSData)BaseData[name];
			}
			if (obj is Dictionary<string, object>)
			{
				return new BCGSData((Dictionary<string, object>)obj);
			}
		}
		return null;
	}

	public DateTime? GetDate(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<DateTime>(value, out var result))
		{
			return result;
		}
		return null;
	}

	public double? GetDouble(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<double>(value, out var result))
		{
			return result;
		}
		try
		{
			return Convert.ToDouble(value);
		}
		catch
		{
			return null;
		}
	}

	public List<double> GetDoubleList(string name)
	{
		object value = null;
		if (BaseData.TryGetValue(name, out value))
		{
			if (value is List<double>)
			{
				return (List<double>)value;
			}
			if (value is List<object>)
			{
				List<object> obj = (List<object>)value;
				List<double> list = new List<double>();
				{
					foreach (object item in obj)
					{
						list.Add(Convert.ToInt64((double)item));
					}
					return list;
				}
			}
		}
		return null;
	}

	public float? GetFloat(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<float>(value, out var result))
		{
			return result;
		}
		try
		{
			return Convert.ToSingle(value);
		}
		catch
		{
			return null;
		}
	}

	public List<float> GetFloatList(string name)
	{
		object value = null;
		if (BaseData.TryGetValue(name, out value))
		{
			if (value is List<float>)
			{
				return (List<float>)value;
			}
			if (value is List<object>)
			{
				List<object> obj = (List<object>)value;
				List<float> list = new List<float>();
				{
					foreach (object item in obj)
					{
						list.Add(Convert.ToSingle((double)item));
					}
					return list;
				}
			}
		}
		return null;
	}

	public BCGSData GetBCGSData(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		try
		{
			if (value is BCGSData)
			{
				return (BCGSData)value;
			}
			if (value is Dictionary<string, object>)
			{
				return new BCGSData((Dictionary<string, object>)value);
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public List<BCGSData> GetBCGSDataList(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		try
		{
			if (tryCast<List<BCGSData>>(value, out var result))
			{
				return result;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public int? GetInt(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<int>(value, out var result))
		{
			return result;
		}
		try
		{
			return Convert.ToInt32(value);
		}
		catch
		{
			return null;
		}
	}

	public List<int> GetIntList(string name)
	{
		object value = null;
		if (BaseData.TryGetValue(name, out value))
		{
			if (value is List<int>)
			{
				return (List<int>)value;
			}
			if (value is List<object>)
			{
				List<object> obj = (List<object>)value;
				List<int> list = new List<int>();
				{
					foreach (object item in obj)
					{
						list.Add(Convert.ToInt32((double)item));
					}
					return list;
				}
			}
		}
		return null;
	}

	public long? GetLong(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<long>(value, out var result))
		{
			return result;
		}
		try
		{
			return Convert.ToInt64(value);
		}
		catch
		{
			return null;
		}
	}

	public List<long> GetLongList(string name)
	{
		object value = null;
		if (BaseData.TryGetValue(name, out value))
		{
			if (value is List<long>)
			{
				return (List<long>)value;
			}
			if (value is List<object>)
			{
				List<object> obj = (List<object>)value;
				List<long> list = new List<long>();
				{
					foreach (object item in obj)
					{
						list.Add(Convert.ToInt64((double)item));
					}
					return list;
				}
			}
		}
		return null;
	}

	public long? GetNumber(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<long>(value, out var result))
		{
			return result;
		}
		try
		{
			return Convert.ToInt64(value);
		}
		catch
		{
			return null;
		}
	}

	public string GetString(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<string>(value, out var result))
		{
			return result;
		}
		return null;
	}

	public List<string> GetStringList(string name)
	{
		object value = null;
		BaseData.TryGetValue(name, out value);
		if (tryCast<List<string>>(value, out var result))
		{
			return result;
		}
		return null;
	}

	public override string ToString()
	{
		return BCGSJson.To(BaseData);
	}

	private bool tryCast<T>(object obj, out T result)
	{
		if (obj is T)
		{
			result = (T)obj;
			return true;
		}
		result = default(T);
		return false;
	}
}
