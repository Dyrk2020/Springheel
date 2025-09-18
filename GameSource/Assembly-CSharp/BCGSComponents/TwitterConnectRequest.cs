using BCGSComponents.DataModels;

namespace BCGSComponents;

public class TwitterConnectRequest : BCGSTypedRequest<TwitterConnectRequest, AuthenticationResponse>
{
	public TwitterConnectRequest()
		: base("TwitterConnectRequest")
	{
	}

	public TwitterConnectRequest(BCGSInstance instance)
		: base(instance, "TwitterConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public TwitterConnectRequest SetAccessSecret(string accessSecret)
	{
		request.AddString("accessSecret", accessSecret);
		return this;
	}

	public TwitterConnectRequest SetAccessToken(string accessToken)
	{
		request.AddString("accessToken", accessToken);
		return this;
	}

	public TwitterConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public TwitterConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public TwitterConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public TwitterConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public TwitterConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public TwitterConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
