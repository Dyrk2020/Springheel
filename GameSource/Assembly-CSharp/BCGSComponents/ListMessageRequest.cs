using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageRequest : BCGSTypedRequest<ListMessageRequest, ListMessageResponse>
{
	public ListMessageRequest()
		: base("ListMessageRequest")
	{
	}

	public ListMessageRequest(BCGSInstance instance)
		: base(instance, "ListMessageRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListMessageResponse(response);
	}

	public ListMessageRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListMessageRequest SetInclude(string include)
	{
		request.AddString("include", include);
		return this;
	}

	public ListMessageRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}
}
