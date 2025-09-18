using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageDetailRequest : BCGSTypedRequest<ListMessageDetailRequest, ListMessageDetailResponse>
{
	public ListMessageDetailRequest()
		: base("ListMessageDetailRequest")
	{
	}

	public ListMessageDetailRequest(BCGSInstance instance)
		: base(instance, "ListMessageDetailRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListMessageDetailResponse(response);
	}

	public ListMessageDetailRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListMessageDetailRequest SetInclude(string include)
	{
		request.AddString("include", include);
		return this;
	}

	public ListMessageDetailRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public ListMessageDetailRequest SetStatus(string status)
	{
		request.AddString("status", status);
		return this;
	}
}
