using BCGSComponents.DataModels;

namespace BCGSComponents;

public class PSNAccountConnectRequest : BCGSTypedRequest<PSNAccountConnectRequest, AuthenticationResponse>
{
	public PSNAccountConnectRequest()
		: base("PSNAccountConnectRequest")
	{
	}

	public PSNAccountConnectRequest(BCGSInstance instance)
		: base(instance, "PSNAccountConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public PSNAccountConnectRequest SetAuthorizationCode(string authorizationCode)
	{
		request.AddString("authorizationCode", authorizationCode);
		return this;
	}

	public PSNAccountConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public PSNAccountConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public PSNAccountConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public PSNAccountConnectRequest SetRedirectUri(string redirectUri)
	{
		request.AddString("redirectUri", redirectUri);
		return this;
	}

	public PSNAccountConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public PSNAccountConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public PSNAccountConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
