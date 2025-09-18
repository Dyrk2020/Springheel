using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListInviteFriendsResponse : BCGSTypedResponse
{
	public class _InvitableFriend : BCGSTypedResponse
	{
		public string DisplayName => response.GetString("displayName");

		public string Id => response.GetString("id");

		public string ProfilePic => response.GetString("profilePic");

		public _InvitableFriend(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_InvitableFriend> Friends => new BCGSEnumerable<_InvitableFriend>(response.GetObjectList("friends"), (BCGSData data) => new _InvitableFriend(data));

	public ListInviteFriendsResponse(BCGSData data)
		: base(data)
	{
	}
}
