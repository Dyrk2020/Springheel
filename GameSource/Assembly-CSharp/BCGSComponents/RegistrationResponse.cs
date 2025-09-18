using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class RegistrationResponse : BCGSTypedResponse
{
	public class _Player : BCGSTypedResponse
	{
		public List<string> Achievements => response.GetStringList("achievements");

		public string DisplayName => response.GetString("displayName");

		public BCGSData ExternalIds => response.GetObject("externalIds");

		public string Id => response.GetString("id");

		public bool? Online => response.GetBoolean("online");

		public List<string> VirtualGoods => response.GetStringList("virtualGoods");

		public _Player(BCGSData data)
			: base(data)
		{
		}
	}

	public string AuthToken => response.GetString("authToken");

	public string DisplayName => response.GetString("displayName");

	public bool? NewPlayer => response.GetBoolean("newPlayer");

	public _Player SwitchSummary
	{
		get
		{
			if (response.GetObject("switchSummary") == null)
			{
				return null;
			}
			return new _Player(response.GetObject("switchSummary"));
		}
	}

	public string UserId => response.GetString("userId");

	public RegistrationResponse(BCGSData data)
		: base(data)
	{
	}
}
