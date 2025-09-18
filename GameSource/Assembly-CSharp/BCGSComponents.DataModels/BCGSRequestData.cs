using System;
using System.Collections.Generic;

namespace BCGSComponents.DataModels;

public class BCGSRequestData : BCGSData
{
	public BCGSRequestData()
	{
		base.BaseData = new Dictionary<string, object>();
	}

	public BCGSRequestData(string jsonString)
	{
		base.BaseData = new Dictionary<string, object>((Dictionary<string, object>)BCGSJson.From(jsonString));
	}

	public BCGSRequestData(BCGSData wrapper)
	{
		base.BaseData = new Dictionary<string, object>((Dictionary<string, object>)BCGSJson.From(wrapper.JSON));
	}

	public BCGSRequestData(IDictionary<string, object> data)
	{
		base.BaseData = new Dictionary<string, object>(data);
	}

	public BCGSRequestData Add(string paramName, object value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddBoolean(string paramName, bool value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddDate(string paramName, DateTime date)
	{
		base.BaseData.Add(paramName, date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm'Z'"));
		return this;
	}

	public BCGSRequestData AddJSONStringAsObject(string paramName, string jsonString)
	{
		if (base.BaseData.ContainsKey(paramName))
		{
			base.BaseData.Remove(paramName);
		}
		base.BaseData.Add(paramName, BCGSJson.From(jsonString));
		return this;
	}

	public BCGSRequestData AddNumber(string paramName, long value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumber(string paramName, float value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumber(string paramName, double value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumber(string paramName, int value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumberList(string paramName, List<float> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumberList(string paramName, List<double> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumberList(string paramName, List<int> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddNumberList(string paramName, List<long> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddObject(string paramName, BCGSData value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddObjectList(string paramName, List<BCGSData> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddString(string paramName, string value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public BCGSRequestData AddStringList(string paramName, List<string> value)
	{
		base.BaseData.Add(paramName, value);
		return this;
	}

	public override string ToString()
	{
		return BCGSJson.To(base.BaseData);
	}
}
