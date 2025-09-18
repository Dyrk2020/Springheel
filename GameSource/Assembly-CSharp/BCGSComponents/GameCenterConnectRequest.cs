using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GameCenterConnectRequest : BCGSTypedRequest<GameCenterConnectRequest, AuthenticationResponse>
{
	public GameCenterConnectRequest()
		: base("GameCenterConnectRequest")
	{
	}

	public GameCenterConnectRequest(BCGSInstance instance)
		: base(instance, "GameCenterConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public GameCenterConnectRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public GameCenterConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public GameCenterConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public GameCenterConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public GameCenterConnectRequest SetExternalPlayerId(string externalPlayerId)
	{
		request.AddString("externalPlayerId", externalPlayerId);
		return this;
	}

	public GameCenterConnectRequest SetPublicKeyUrl(string publicKeyUrl)
	{
		request.AddString("publicKeyUrl", publicKeyUrl);
		return this;
	}

	public GameCenterConnectRequest SetSalt(string salt)
	{
		request.AddString("salt", salt);
		return this;
	}

	public GameCenterConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public GameCenterConnectRequest SetSignature(string signature)
	{
		request.AddString("signature", signature);
		return this;
	}

	public GameCenterConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public GameCenterConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}

	public GameCenterConnectRequest SetTimestamp(long timestamp)
	{
		request.AddNumber("timestamp", timestamp);
		return this;
	}
}
