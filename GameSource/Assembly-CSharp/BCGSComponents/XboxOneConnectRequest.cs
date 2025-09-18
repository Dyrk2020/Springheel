using BCGSComponents.DataModels;

namespace BCGSComponents;

public class XboxOneConnectRequest : BCGSTypedRequest<XboxOneConnectRequest, AuthenticationResponse>
{
	public XboxOneConnectRequest()
		: base("XboxOneConnectRequest")
	{
	}

	public XboxOneConnectRequest(BCGSInstance instance)
		: base(instance, "XboxOneConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public XboxOneConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public XboxOneConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public XboxOneConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public XboxOneConnectRequest SetSandbox(string sandbox)
	{
		request.AddString("sandbox", sandbox);
		return this;
	}

	public XboxOneConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public XboxOneConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public XboxOneConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}

	public XboxOneConnectRequest SetToken(string token)
	{
		request.AddString("token", token);
		return this;
	}
}
