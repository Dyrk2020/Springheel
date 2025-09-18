using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListBulkJobsAdminRequest : BCGSTypedRequest<ListBulkJobsAdminRequest, ListBulkJobsAdminResponse>
{
	public ListBulkJobsAdminRequest()
		: base("ListBulkJobsAdminRequest")
	{
	}

	public ListBulkJobsAdminRequest(BCGSInstance instance)
		: base(instance, "ListBulkJobsAdminRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListBulkJobsAdminResponse(response);
	}

	public ListBulkJobsAdminRequest SetBulkJobIds(List<string> bulkJobIds)
	{
		request.AddStringList("bulkJobIds", bulkJobIds);
		return this;
	}
}
