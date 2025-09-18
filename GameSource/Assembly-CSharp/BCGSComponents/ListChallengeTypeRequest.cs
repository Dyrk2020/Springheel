using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListChallengeTypeRequest : BCGSTypedRequest<ListChallengeTypeRequest, ListChallengeTypeResponse>
{
	public ListChallengeTypeRequest()
		: base("ListChallengeTypeRequest")
	{
	}

	public ListChallengeTypeRequest(BCGSInstance instance)
		: base(instance, "ListChallengeTypeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListChallengeTypeResponse(response);
	}
}
