using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AnalyticsRequest : BCGSTypedRequest<AnalyticsRequest, AnalyticsResponse>
{
	public AnalyticsRequest()
		: base("AnalyticsRequest")
	{
	}

	public AnalyticsRequest(BCGSInstance instance)
		: base(instance, "AnalyticsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AnalyticsResponse(response);
	}

	public AnalyticsRequest SetData(BCGSRequestData data)
	{
		request.AddObject("data", data);
		return this;
	}

	public AnalyticsRequest SetEnd(bool end)
	{
		request.AddBoolean("end", end);
		return this;
	}

	public AnalyticsRequest SetKey(string key)
	{
		request.AddString("key", key);
		return this;
	}

	public AnalyticsRequest SetStart(bool start)
	{
		request.AddBoolean("start", start);
		return this;
	}
}
