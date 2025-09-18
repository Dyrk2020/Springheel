using BCGSComponents.DataModels;

namespace BCGSComponents;

public class FindMatchRequest : BCGSTypedRequest<FindMatchRequest, FindMatchResponse>
{
	public FindMatchRequest()
		: base("FindMatchRequest")
	{
	}

	public FindMatchRequest(BCGSInstance instance)
		: base(instance, "FindMatchRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new FindMatchResponse(response);
	}

	public FindMatchRequest SetAction(string action)
	{
		request.AddString("action", action);
		return this;
	}

	public FindMatchRequest SetMatchGroup(string matchGroup)
	{
		request.AddString("matchGroup", matchGroup);
		return this;
	}

	public FindMatchRequest SetMatchShortCode(string matchShortCode)
	{
		request.AddString("matchShortCode", matchShortCode);
		return this;
	}

	public FindMatchRequest SetSkill(long skill)
	{
		request.AddNumber("skill", skill);
		return this;
	}
}
