using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetMyTeamsRequest : BCGSTypedRequest<GetMyTeamsRequest, GetMyTeamsResponse>
{
	public GetMyTeamsRequest()
		: base("GetMyTeamsRequest")
	{
	}

	public GetMyTeamsRequest(BCGSInstance instance)
		: base(instance, "GetMyTeamsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new GetMyTeamsResponse(response);
	}

	public GetMyTeamsRequest SetOwnedOnly(bool ownedOnly)
	{
		request.AddBoolean("ownedOnly", ownedOnly);
		return this;
	}

	public GetMyTeamsRequest SetTeamTypes(List<string> teamTypes)
	{
		request.AddStringList("teamTypes", teamTypes);
		return this;
	}
}
