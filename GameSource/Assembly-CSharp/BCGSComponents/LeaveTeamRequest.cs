using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LeaveTeamRequest : BCGSTypedRequest<LeaveTeamRequest, LeaveTeamResponse>
{
	public LeaveTeamRequest()
		: base("LeaveTeamRequest")
	{
	}

	public LeaveTeamRequest(BCGSInstance instance)
		: base(instance, "LeaveTeamRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new LeaveTeamResponse(response);
	}

	public LeaveTeamRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public LeaveTeamRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public LeaveTeamRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
