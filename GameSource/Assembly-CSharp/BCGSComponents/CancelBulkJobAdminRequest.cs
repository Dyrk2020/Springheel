using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class CancelBulkJobAdminRequest : BCGSTypedRequest<CancelBulkJobAdminRequest, CancelBulkJobAdminResponse>
{
	public CancelBulkJobAdminRequest()
		: base("CancelBulkJobAdminRequest")
	{
	}

	public CancelBulkJobAdminRequest(BCGSInstance instance)
		: base(instance, "CancelBulkJobAdminRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new CancelBulkJobAdminResponse(response);
	}

	public CancelBulkJobAdminRequest SetBulkJobIds(List<string> bulkJobIds)
	{
		request.AddStringList("bulkJobIds", bulkJobIds);
		return this;
	}
}
