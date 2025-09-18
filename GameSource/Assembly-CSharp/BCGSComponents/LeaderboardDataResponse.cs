using BCGSComponents.DataModels;

namespace BCGSComponents;

public class LeaderboardDataResponse : BCGSTypedResponse
{
	public class _LeaderboardData : BCGSTypedResponse
	{
		public string City => response.GetString("city");

		public string Country => response.GetString("country");

		public BCGSData ExternalIds => response.GetObject("externalIds");

		public long? Rank => response.GetLong("rank");

		public string UserId => response.GetString("userId");

		public string UserName => response.GetString("userName");

		public string When => response.GetString("when");

		public _LeaderboardData(BCGSData data)
			: base(data)
		{
		}

		public long? GetNumberValue(string key)
		{
			return response.GetLong(key);
		}

		public string GetStringValue(string key)
		{
			return response.GetString(key);
		}
	}

	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public BCGSEnumerable<_LeaderboardData> Data => new BCGSEnumerable<_LeaderboardData>(response.GetObjectList("data"), (BCGSData data) => new _LeaderboardData(data));

	public BCGSEnumerable<_LeaderboardData> First => new BCGSEnumerable<_LeaderboardData>(response.GetObjectList("first"), (BCGSData data) => new _LeaderboardData(data));

	public BCGSEnumerable<_LeaderboardData> Last => new BCGSEnumerable<_LeaderboardData>(response.GetObjectList("last"), (BCGSData data) => new _LeaderboardData(data));

	public string LeaderboardShortCode => response.GetString("leaderboardShortCode");

	public LeaderboardDataResponse(BCGSData data)
		: base(data)
	{
	}
}
