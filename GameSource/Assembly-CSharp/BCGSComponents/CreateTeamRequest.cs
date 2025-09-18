using BCGSComponents.DataModels;

namespace BCGSComponents;

public class CreateTeamRequest : BCGSTypedRequest<CreateTeamRequest, CreateTeamResponse>
{
	public CreateTeamRequest()
		: base("CreateTeamRequest")
	{
	}

	public CreateTeamRequest(BCGSInstance instance)
		: base(instance, "CreateTeamRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new CreateTeamResponse(response);
	}

	public CreateTeamRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public CreateTeamRequest SetTeamName(string teamName)
	{
		request.AddString("teamName", teamName);
		return this;
	}

	public CreateTeamRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
