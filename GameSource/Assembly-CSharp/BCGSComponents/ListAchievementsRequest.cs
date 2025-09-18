using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListAchievementsRequest : BCGSTypedRequest<ListAchievementsRequest, ListAchievementsResponse>
{
	public ListAchievementsRequest()
		: base("ListAchievementsRequest")
	{
	}

	public ListAchievementsRequest(BCGSInstance instance)
		: base(instance, "ListAchievementsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListAchievementsResponse(response);
	}
}
