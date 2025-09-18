using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListChallengeRequest : BCGSTypedRequest<ListChallengeRequest, ListChallengeResponse>
{
	public ListChallengeRequest()
		: base("ListChallengeRequest")
	{
	}

	public ListChallengeRequest(BCGSInstance instance)
		: base(instance, "ListChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListChallengeResponse(response);
	}

	public ListChallengeRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListChallengeRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public ListChallengeRequest SetShortCode(string shortCode)
	{
		request.AddString("shortCode", shortCode);
		return this;
	}

	public ListChallengeRequest SetState(string state)
	{
		request.AddString("state", state);
		return this;
	}

	public ListChallengeRequest SetStates(List<string> states)
	{
		request.AddStringList("states", states);
		return this;
	}
}
