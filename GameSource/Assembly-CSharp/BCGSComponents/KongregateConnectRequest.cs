using BCGSComponents.DataModels;

namespace BCGSComponents;

public class KongregateConnectRequest : BCGSTypedRequest<KongregateConnectRequest, AuthenticationResponse>
{
	public KongregateConnectRequest()
		: base("KongregateConnectRequest")
	{
	}

	public KongregateConnectRequest(BCGSInstance instance)
		: base(instance, "KongregateConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public KongregateConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public KongregateConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public KongregateConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public KongregateConnectRequest SetGameAuthToken(string gameAuthToken)
	{
		request.AddString("gameAuthToken", gameAuthToken);
		return this;
	}

	public KongregateConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public KongregateConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public KongregateConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}

	public KongregateConnectRequest SetUserId(string userId)
	{
		request.AddString("userId", userId);
		return this;
	}
}
