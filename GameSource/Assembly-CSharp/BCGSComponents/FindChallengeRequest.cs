using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class FindChallengeRequest : BCGSTypedRequest<FindChallengeRequest, FindChallengeResponse>
{
	public FindChallengeRequest()
		: base("FindChallengeRequest")
	{
	}

	public FindChallengeRequest(BCGSInstance instance)
		: base(instance, "FindChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new FindChallengeResponse(response);
	}

	public FindChallengeRequest SetAccessType(string accessType)
	{
		request.AddString("accessType", accessType);
		return this;
	}

	public FindChallengeRequest SetCount(long count)
	{
		request.AddNumber("count", count);
		return this;
	}

	public FindChallengeRequest SetEligibility(BCGSRequestData eligibility)
	{
		request.AddObject("eligibility", eligibility);
		return this;
	}

	public FindChallengeRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public FindChallengeRequest SetShortCode(List<string> shortCode)
	{
		request.AddStringList("shortCode", shortCode);
		return this;
	}
}
