using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GooglePlusConnectRequest : BCGSTypedRequest<GooglePlusConnectRequest, AuthenticationResponse>
{
	public GooglePlusConnectRequest()
		: base("GooglePlusConnectRequest")
	{
	}

	public GooglePlusConnectRequest(BCGSInstance instance)
		: base(instance, "GooglePlusConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public GooglePlusConnectRequest SetAccessToken(string accessToken)
	{
		request.AddString("accessToken", accessToken);
		return this;
	}

	public GooglePlusConnectRequest SetCode(string code)
	{
		request.AddString("code", code);
		return this;
	}

	public GooglePlusConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public GooglePlusConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public GooglePlusConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public GooglePlusConnectRequest SetRedirectUri(string redirectUri)
	{
		request.AddString("redirectUri", redirectUri);
		return this;
	}

	public GooglePlusConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public GooglePlusConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public GooglePlusConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
