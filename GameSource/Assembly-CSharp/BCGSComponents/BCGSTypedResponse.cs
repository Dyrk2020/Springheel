using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public abstract class BCGSTypedResponse
{
	protected BCGSData response;

	public string JSONString => response.ToString();

	public IDictionary<string, object> JSONData => response.BaseData;

	public BCGSData ScriptData => response.GetBCGSData("scriptData");

	public bool HasErrors => response.ContainsKey("error");

	public BCGSData Errors => response.GetBCGSData("error");

	public string RequestId => response.GetString("requestId");

	public BCGSData BaseData => response;

	public BCGSTypedResponse(BCGSData response)
	{
		this.response = response;
	}

	public override string ToString()
	{
		return BCGSJson.To(response.BaseData);
	}
}
