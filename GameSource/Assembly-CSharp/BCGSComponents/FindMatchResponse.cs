using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class FindMatchResponse : BCGSTypedResponse
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

	public string AccessToken => response.GetString("accessToken");

	public string Host => response.GetString("host");

	public BCGSData MatchData => response.GetObject("matchData");

	public string MatchId => response.GetString("matchId");

	public BCGSEnumerable<_Player> Opponents => new BCGSEnumerable<_Player>(response.GetObjectList("opponents"), (BCGSData data) => new _Player(data));

	public int? PeerId => response.GetInt("peerId");

	public string PlayerId => response.GetString("playerId");

	public int? Port => response.GetInt("port");

	public FindMatchResponse(BCGSData data)
		: base(data)
	{
	}
}
