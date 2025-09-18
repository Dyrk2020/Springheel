using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListTeamChatRequest : BCGSTypedRequest<ListTeamChatRequest, ListTeamChatResponse>
{
	public ListTeamChatRequest()
		: base("ListTeamChatRequest")
	{
	}

	public ListTeamChatRequest(BCGSInstance instance)
		: base(instance, "ListTeamChatRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListTeamChatResponse(response);
	}

	public ListTeamChatRequest SetEntryCount(long entryCount)
	{
		request.AddNumber("entryCount", entryCount);
		return this;
	}

	public ListTeamChatRequest SetOffset(long offset)
	{
		request.AddNumber("offset", offset);
		return this;
	}

	public ListTeamChatRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public ListTeamChatRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public ListTeamChatRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
