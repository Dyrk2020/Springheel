using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinTeamRequest : BCGSTypedRequest<JoinTeamRequest, JoinTeamResponse>
{
	public JoinTeamRequest()
		: base("JoinTeamRequest")
	{
	}

	public JoinTeamRequest(BCGSInstance instance)
		: base(instance, "JoinTeamRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new JoinTeamResponse(response);
	}

	public JoinTeamRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public JoinTeamRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public JoinTeamRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
