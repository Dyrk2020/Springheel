using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListChallengeResponse : BCGSTypedResponse
{
	public class _Challenge : BCGSTypedResponse
	{
		public class _PlayerDetail : BCGSTypedResponse
		{
			public BCGSData ExternalIds => response.GetObject("externalIds");

			public string Id => response.GetString("id");

			public string Name => response.GetString("name");

			public _PlayerDetail(BCGSData data)
				: base(data)
			{
			}
		}

		public class _PlayerTurnCount : BCGSTypedResponse
		{
			public string Count => response.GetString("count");

			public string PlayerId => response.GetString("playerId");

			public _PlayerTurnCount(BCGSData data)
				: base(data)
			{
			}
		}

		public BCGSEnumerable<_PlayerDetail> Accepted => new BCGSEnumerable<_PlayerDetail>(response.GetObjectList("accepted"), (BCGSData data) => new _PlayerDetail(data));

		public string ChallengeId => response.GetString("challengeId");

		public string ChallengeMessage => response.GetString("challengeMessage");

		public string ChallengeName => response.GetString("challengeName");

		public BCGSEnumerable<_PlayerDetail> Challenged => new BCGSEnumerable<_PlayerDetail>(response.GetObjectList("challenged"), (BCGSData data) => new _PlayerDetail(data));

		public _PlayerDetail Challenger
		{
			get
			{
				if (response.GetObject("challenger") == null)
				{
					return null;
				}
				return new _PlayerDetail(response.GetObject("challenger"));
			}
		}

		public long? Currency1Wager => response.GetLong("currency1Wager");

		public long? Currency2Wager => response.GetLong("currency2Wager");

		public long? Currency3Wager => response.GetLong("currency3Wager");

		public long? Currency4Wager => response.GetLong("currency4Wager");

		public long? Currency5Wager => response.GetLong("currency5Wager");

		public long? Currency6Wager => response.GetLong("currency6Wager");

		public BCGSData CurrencyWagers => response.GetObject("currencyWagers");

		public BCGSEnumerable<_PlayerDetail> Declined => new BCGSEnumerable<_PlayerDetail>(response.GetObjectList("declined"), (BCGSData data) => new _PlayerDetail(data));

		public DateTime? EndDate => response.GetDate("endDate");

		public DateTime? ExpiryDate => response.GetDate("expiryDate");

		public long? MaxTurns => response.GetLong("maxTurns");

		public string NextPlayer => response.GetString("nextPlayer");

		public string ShortCode => response.GetString("shortCode");

		public DateTime? StartDate => response.GetDate("startDate");

		public string State => response.GetString("state");

		public BCGSEnumerable<_PlayerTurnCount> TurnCount => new BCGSEnumerable<_PlayerTurnCount>(response.GetObjectList("turnCount"), (BCGSData data) => new _PlayerTurnCount(data));

		public _Challenge(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_Challenge> ChallengeInstances => new BCGSEnumerable<_Challenge>(response.GetObjectList("challengeInstances"), (BCGSData data) => new _Challenge(data));

	public ListChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
