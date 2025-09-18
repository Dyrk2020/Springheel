using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListChallengeTypeResponse : BCGSTypedResponse
{
	public class _ChallengeType : BCGSTypedResponse
	{
		public string ChallengeShortCode => response.GetString("challengeShortCode");

		public string Description => response.GetString("description");

		public string GetleaderboardName => response.GetString("getleaderboardName");

		public string Name => response.GetString("name");

		public string Tags => response.GetString("tags");

		public _ChallengeType(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_ChallengeType> ChallengeTemplates => new BCGSEnumerable<_ChallengeType>(response.GetObjectList("challengeTemplates"), (BCGSData data) => new _ChallengeType(data));

	public ListChallengeTypeResponse(BCGSData data)
		: base(data)
	{
	}
}
