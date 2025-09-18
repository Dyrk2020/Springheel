using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class BatchAdminRequest : BCGSTypedRequest<BatchAdminRequest, BatchAdminResponse>
{
	public BatchAdminRequest()
		: base("BatchAdminRequest")
	{
	}

	public BatchAdminRequest(BCGSInstance instance)
		: base(instance, "BatchAdminRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BatchAdminResponse(response);
	}

	public BatchAdminRequest SetPlayerIds(List<string> playerIds)
	{
		request.AddStringList("playerIds", playerIds);
		return this;
	}

	public BatchAdminRequest SetRequest(BCGSRequestData request)
	{
		request.AddObject("request", request);
		return this;
	}
}
