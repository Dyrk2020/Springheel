using BCGSComponents.DataModels;

namespace BCGSComponents;

public class PSNConnectRequest : BCGSTypedRequest<PSNConnectRequest, AuthenticationResponse>
{
	public PSNConnectRequest()
		: base("PSNConnectRequest")
	{
	}

	public PSNConnectRequest(BCGSInstance instance)
		: base(instance, "PSNConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public PSNConnectRequest SetAuthorizationCode(string authorizationCode)
	{
		request.AddString("authorizationCode", authorizationCode);
		return this;
	}

	public PSNConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public PSNConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public PSNConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public PSNConnectRequest SetRedirectUri(string redirectUri)
	{
		request.AddString("redirectUri", redirectUri);
		return this;
	}

	public PSNConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public PSNConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public PSNConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
