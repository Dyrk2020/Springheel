using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ScheduleBulkJobAdminRequest : BCGSTypedRequest<ScheduleBulkJobAdminRequest, ScheduleBulkJobAdminResponse>
{
	public ScheduleBulkJobAdminRequest()
		: base("ScheduleBulkJobAdminRequest")
	{
	}

	public ScheduleBulkJobAdminRequest(BCGSInstance instance)
		: base(instance, "ScheduleBulkJobAdminRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ScheduleBulkJobAdminResponse(response);
	}

	public ScheduleBulkJobAdminRequest SetData(BCGSRequestData data)
	{
		request.AddObject("data", data);
		return this;
	}

	public ScheduleBulkJobAdminRequest SetModuleShortCode(string moduleShortCode)
	{
		request.AddString("moduleShortCode", moduleShortCode);
		return this;
	}

	public ScheduleBulkJobAdminRequest SetPlayerQuery(BCGSRequestData playerQuery)
	{
		request.AddObject("playerQuery", playerQuery);
		return this;
	}

	public ScheduleBulkJobAdminRequest SetScheduledTime(DateTime scheduledTime)
	{
		request.AddDate("scheduledTime", scheduledTime);
		return this;
	}

	public ScheduleBulkJobAdminRequest SetScript(string script)
	{
		request.AddString("script", script);
		return this;
	}
}
