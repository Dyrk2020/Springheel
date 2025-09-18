using BCGSComponents.DataModels;

namespace BCGSComponents;

public class XBOXLiveConnectRequest : BCGSTypedRequest<XBOXLiveConnectRequest, AuthenticationResponse>
{
	public XBOXLiveConnectRequest()
		: base("XBOXLiveConnectRequest")
	{
	}

	public XBOXLiveConnectRequest(BCGSInstance instance)
		: base(instance, "XBOXLiveConnectRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new AuthenticationResponse(response);
	}

	public XBOXLiveConnectRequest SetDisplayName(string displayName)
	{
		request.AddString("displayName", displayName);
		return this;
	}

	public XBOXLiveConnectRequest SetDoNotCreateNewPlayer(bool doNotCreateNewPlayer)
	{
		request.AddBoolean("doNotCreateNewPlayer", doNotCreateNewPlayer);
		return this;
	}

	public XBOXLiveConnectRequest SetDoNotLinkToCurrentPlayer(bool doNotLinkToCurrentPlayer)
	{
		request.AddBoolean("doNotLinkToCurrentPlayer", doNotLinkToCurrentPlayer);
		return this;
	}

	public XBOXLiveConnectRequest SetErrorOnSwitch(bool errorOnSwitch)
	{
		request.AddBoolean("errorOnSwitch", errorOnSwitch);
		return this;
	}

	public XBOXLiveConnectRequest SetSegments(BCGSRequestData segments)
	{
		request.AddObject("segments", segments);
		return this;
	}

	public XBOXLiveConnectRequest SetStsTokenString(string stsTokenString)
	{
		request.AddString("stsTokenString", stsTokenString);
		return this;
	}

	public XBOXLiveConnectRequest SetSwitchIfPossible(bool switchIfPossible)
	{
		request.AddBoolean("switchIfPossible", switchIfPossible);
		return this;
	}

	public XBOXLiveConnectRequest SetSyncDisplayName(bool syncDisplayName)
	{
		request.AddBoolean("syncDisplayName", syncDisplayName);
		return this;
	}
}
