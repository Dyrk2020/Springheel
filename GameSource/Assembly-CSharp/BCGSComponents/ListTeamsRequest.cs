using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListTeamsRequest : BCGSTypedRequest<ListTeamsRequest, ListTeamsResponse>
{
	public ListTeamsRequest()
		: base("ListTeamsRequest")
	{
	}

	public ListTeamsRequest(BCGSInstance instance)
		: base(instance, "ListTeamsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListTeamsResponse(response);
	}

	public ListTeamsRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListTeamsRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public ListTeamsRequest SetTeamNameFilter(string teamNameFilter)
	{
		request.AddString("teamNameFilter", teamNameFilter);
		return this;
	}

	public ListTeamsRequest SetTeamTypeFilter(string teamTypeFilter)
	{
		request.AddString("teamTypeFilter", teamTypeFilter);
		return this;
	}
}
