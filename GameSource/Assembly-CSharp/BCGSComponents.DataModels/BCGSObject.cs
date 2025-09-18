using System.Collections.Generic;

namespace BCGSComponents.DataModels;

public class BCGSObject : BCGSRequestData
{
	public string Type => GetString("scriptName");

	public BCGSObject(IDictionary<string, object> data)
		: base(data)
	{
	}

	public BCGSObject(BCGSData wrapper)
	{
		base.BaseData = new Dictionary<string, object>((Dictionary<string, object>)BCGSJson.From(wrapper.JSON));
	}

	public BCGSObject(string type)
	{
		AddString("scriptName", type);
	}

	protected BCGSObject()
	{
		base.BaseData = new Dictionary<string, object>();
	}

	public static BCGSObject FromJson(string json)
	{
		return new BCGSObject((Dictionary<string, object>)BCGSJson.From(json));
	}
}
