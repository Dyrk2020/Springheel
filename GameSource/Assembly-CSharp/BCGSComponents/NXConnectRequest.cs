using BCGSComponents.DataModels;

namespace BCGSComponents;

public class NXConnectRequest : BCGSTypedRequest<NXConnectRequest, AuthenticationResponse>
{
	public NXConnectRequest()
		: base("NXConnectRequest")
	{
	}

	public NXConnectRequest(BCGSInstance instance)
		: base(instance, "NXConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public NXConnectRequest SetAccountPerLoginId(bool accountPerLoginId)
	{
		request.AddBoolean("accountPerLoginId", accountPerLoginId);
		return this;
	}

	public NXConnectRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public NXConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public NXConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public NXConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public NXConnectRequest SetNsaIdToken(string nsaIdToken)
	{
		request.AddString("nsaIdToken", nsaIdToken);
		return this;
	}

	public NXConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public NXConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public NXConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
