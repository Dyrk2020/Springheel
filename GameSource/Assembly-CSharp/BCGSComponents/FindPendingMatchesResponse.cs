using BCGSComponents.DataModels;

namespace BCGSComponents;

public class FindPendingMatchesResponse : BCGSTypedResponse
{
	public class _PendingMatch : BCGSTypedResponse
	{
		public class _MatchedPlayer : BCGSTypedResponse
		{
			public BCGSData Location => response.GetObject("location");

			public BCGSData ParticipantData => response.GetObject("participantData");

			public string PlayerId => response.GetString("playerId");

			public double? Skill => response.GetDouble("skill");

			public _MatchedPlayer(BCGSData data)
				: base(data)
			{
			}
		}

		public string Id => response.GetString("id");

		public BCGSData MatchData => response.GetObject("matchData");

		public string MatchGroup => response.GetString("matchGroup");

		public string MatchShortCode => response.GetString("matchShortCode");

		public BCGSEnumerable<_MatchedPlayer> MatchedPlayers => new BCGSEnumerable<_MatchedPlayer>(response.GetObjectList("matchedPlayers"), (BCGSData data) => new _MatchedPlayer(data));

		public double? Skill => response.GetDouble("skill");

		public _PendingMatch(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_PendingMatch> PendingMatches => new BCGSEnumerable<_PendingMatch>(response.GetObjectList("pendingMatches"), (BCGSData data) => new _PendingMatch(data));

	public FindPendingMatchesResponse(BCGSData data)
		: base(data)
	{
	}
}
