using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageSummaryRequest : BCGSTypedRequest<ListMessageSummaryRequest, ListMessageSummaryResponse>
{
	public ListMessageSummaryRequest()
		: base("ListMessageSummaryRequest")
	{
	}

	public ListMessageSummaryRequest(BCGSInstance instance)
		: base(instance, "ListMessageSummaryRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListMessageSummaryResponse(response);
	}

	public ListMessageSummaryRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListMessageSummaryRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}
}
