using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinPendingMatchResponse : BCGSTypedResponse
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

	public _PendingMatch PendingMatch
	{
		get
		{
			if (response.GetObject("pendingMatch") == null)
			{
				return null;
			}
			return new _PendingMatch(response.GetObject("pendingMatch"));
		}
	}

	public JoinPendingMatchResponse(BCGSData data)
		: base(data)
	{
	}
}
