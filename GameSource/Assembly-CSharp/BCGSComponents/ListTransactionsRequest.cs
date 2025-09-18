using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListTransactionsRequest : BCGSTypedRequest<ListTransactionsRequest, ListTransactionsResponse>
{
	public ListTransactionsRequest()
		: base("ListTransactionsRequest")
	{
	}

	public ListTransactionsRequest(BCGSInstance instance)
		: base(instance, "ListTransactionsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListTransactionsResponse(response);
	}

	public ListTransactionsRequest SetDateFrom(DateTime dateFrom)
	{
		request.AddDate("dateFrom", dateFrom);
		return this;
	}

	public ListTransactionsRequest SetDateTo(DateTime dateTo)
	{
		request.AddDate("dateTo", dateTo);
		return this;
	}

	public ListTransactionsRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListTransactionsRequest SetInclude(string include)
	{
		request.AddString("include", include);
		return this;
	}

	public ListTransactionsRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}
}
