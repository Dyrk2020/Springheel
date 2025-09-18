using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DropTeamRequest : BCGSTypedRequest<DropTeamRequest, DropTeamResponse>
{
	public DropTeamRequest()
		: base("DropTeamRequest")
	{
	}

	public DropTeamRequest(BCGSInstance instance)
		: base(instance, "DropTeamRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new DropTeamResponse(response);
	}

	public DropTeamRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public DropTeamRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public DropTeamRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
