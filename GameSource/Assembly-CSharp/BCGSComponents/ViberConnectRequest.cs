using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ViberConnectRequest : BCGSTypedRequest<ViberConnectRequest, AuthenticationResponse>
{
	public ViberConnectRequest()
		: base("ViberConnectRequest")
	{
	}

	public ViberConnectRequest(BCGSInstance instance)
		: base(instance, "ViberConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public ViberConnectRequest SetAccessToken(string accessToken)
	{
		request.AddString("accessToken", accessToken);
		return this;
	}

	public ViberConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public ViberConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public ViberConnectRequest SetDoNotRegisterForPush(bool doNotRegisterForPush)
	{
		request.AddBoolean("doNotRegisterForPush", doNotRegisterForPush);
		return this;
	}

	public ViberConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public ViberConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public ViberConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public ViberConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
