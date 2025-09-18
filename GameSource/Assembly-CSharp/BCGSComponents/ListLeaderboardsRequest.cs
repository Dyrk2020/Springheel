using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListLeaderboardsRequest : BCGSTypedRequest<ListLeaderboardsRequest, ListLeaderboardsResponse>
{
	public ListLeaderboardsRequest()
		: base("ListLeaderboardsRequest")
	{
	}

	public ListLeaderboardsRequest(BCGSInstance instance)
		: base(instance, "ListLeaderboardsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListLeaderboardsResponse(response);
	}
}
