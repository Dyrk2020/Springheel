using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetTeamRequest : BCGSTypedRequest<GetTeamRequest, GetTeamResponse>
{
	public GetTeamRequest()
		: base("GetTeamRequest")
	{
	}

	public GetTeamRequest(BCGSInstance instance)
		: base(instance, "GetTeamRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetTeamResponse(response);
	}

	public GetTeamRequest SetOwnerId(string ownerId)
	{
		request.AddString("ownerId", ownerId);
		return this;
	}

	public GetTeamRequest SetTeamId(string teamId)
	{
		request.AddString("teamId", teamId);
		return this;
	}

	public GetTeamRequest SetTeamType(string teamType)
	{
		request.AddString("teamType", teamType);
		return this;
	}
}
