using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AmazonConnectRequest : BCGSTypedRequest<AmazonConnectRequest, AuthenticationResponse>
{
	public AmazonConnectRequest()
		: base("AmazonConnectRequest")
	{
	}

	public AmazonConnectRequest(BCGSInstance instance)
		: base(instance, "AmazonConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public AmazonConnectRequest SetAccessToken(string accessToken)
	{
		request.AddString("accessToken", accessToken);
		return this;
	}

	public AmazonConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public AmazonConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public AmazonConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public AmazonConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public AmazonConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public AmazonConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
