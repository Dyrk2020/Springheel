using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListInviteFriendsRequest : BCGSTypedRequest<ListInviteFriendsRequest, ListInviteFriendsResponse>
{
	public ListInviteFriendsRequest()
		: base("ListInviteFriendsRequest")
	{
	}

	public ListInviteFriendsRequest(BCGSInstance instance)
		: base(instance, "ListInviteFriendsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListInviteFriendsResponse(response);
	}
}
