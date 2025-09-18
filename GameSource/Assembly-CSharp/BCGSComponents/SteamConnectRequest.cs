using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SteamConnectRequest : BCGSTypedRequest<SteamConnectRequest, AuthenticationResponse>
{
	public SteamConnectRequest()
		: base("SteamConnectRequest")
	{
	}

	public SteamConnectRequest(BCGSInstance instance)
		: base(instance, "SteamConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public SteamConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public SteamConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public SteamConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public SteamConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public SteamConnectRequest SetSessionTicket(string sessionTicket)
	{
		request.AddString("sessionTicket", sessionTicket);
		return this;
	}

	public SteamConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public SteamConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
