using BCGSComponents.DataModels;

namespace BCGSComponents;

public class FindPendingMatchesRequest : BCGSTypedRequest<FindPendingMatchesRequest, FindPendingMatchesResponse>
{
	public FindPendingMatchesRequest()
		: base("FindPendingMatchesRequest")
	{
	}

	public FindPendingMatchesRequest(BCGSInstance instance)
		: base(instance, "FindPendingMatchesRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new FindPendingMatchesResponse(response);
	}

	public FindPendingMatchesRequest SetMatchGroup(string matchGroup)
	{
		request.AddString("matchGroup", matchGroup);
		return this;
	}

	public FindPendingMatchesRequest SetMatchShortCode(string matchShortCode)
	{
		request.AddString("matchShortCode", matchShortCode);
		return this;
	}

	public FindPendingMatchesRequest SetMaxMatchesToFind(long maxMatchesToFind)
	{
		request.AddNumber("maxMatchesToFind", maxMatchesToFind);
		return this;
	}
}
