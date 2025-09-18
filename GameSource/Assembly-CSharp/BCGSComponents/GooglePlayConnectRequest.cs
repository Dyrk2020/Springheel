using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GooglePlayConnectRequest : BCGSTypedRequest<GooglePlayConnectRequest, AuthenticationResponse>
{
	public GooglePlayConnectRequest()
		: base("GooglePlayConnectRequest")
	{
	}

	public GooglePlayConnectRequest(BCGSInstance instance)
		: base(instance, "GooglePlayConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public GooglePlayConnectRequest SetAccessToken(string accessToken)
	{
		request.AddString("accessToken", accessToken);
		return this;
	}

	public GooglePlayConnectRequest SetCode(string code)
	{
		request.AddString("code", code);
		return this;
	}

	public GooglePlayConnectRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public GooglePlayConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public GooglePlayConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public GooglePlayConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public GooglePlayConnectRequest SetGooglePlusScope(bool googlePlusScope)
	{
		request.AddBoolean("googlePlusScope", googlePlusScope);
		return this;
	}

	public GooglePlayConnectRequest SetProfileScope(bool profileScope)
	{
		request.AddBoolean("profileScope", profileScope);
		return this;
	}

	public GooglePlayConnectRequest SetRedirectUri(string redirectUri)
	{
		request.AddString("redirectUri", redirectUri);
		return this;
	}

	public GooglePlayConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public GooglePlayConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public GooglePlayConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
