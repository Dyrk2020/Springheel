using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListGameFriendsRequest : BCGSTypedRequest<ListGameFriendsRequest, ListGameFriendsResponse>
{
	public ListGameFriendsRequest()
		: base("ListGameFriendsRequest")
	{
	}

	public ListGameFriendsRequest(BCGSInstance instance)
		: base(instance, "ListGameFriendsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListGameFriendsResponse(response);
	}
}
