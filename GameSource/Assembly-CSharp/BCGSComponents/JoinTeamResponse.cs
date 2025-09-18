using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinTeamResponse : BCGSTypedResponse
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

	public BCGSEnumerable<_Player> Members => new BCGSEnumerable<_Player>(response.GetObjectList("members"), (BCGSData data) => new _Player(data));

	public _Player Owner
	{
		get
		{
			if (response.GetObject("owner") == null)
			{
				return null;
			}
			return new _Player(response.GetObject("owner"));
		}
	}

	public string TeamId => response.GetString("teamId");

	public string TeamName => response.GetString("teamName");

	public string TeamType => response.GetString("teamType");

	public JoinTeamResponse(BCGSData data)
		: base(data)
	{
	}
}
