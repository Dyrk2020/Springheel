using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListLeaderboardsResponse : BCGSTypedResponse
{
	public class _Leaderboard : BCGSTypedResponse
	{
		public string Description => response.GetString("description");

		public string Name => response.GetString("name");

		public BCGSData PropertySet => response.GetObject("propertySet");

		public string ShortCode => response.GetString("shortCode");

		public _Leaderboard(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_Leaderboard> Leaderboards => new BCGSEnumerable<_Leaderboard>(response.GetObjectList("leaderboards"), (BCGSData data) => new _Leaderboard(data));

	public ListLeaderboardsResponse(BCGSData data)
		: base(data)
	{
	}
}
