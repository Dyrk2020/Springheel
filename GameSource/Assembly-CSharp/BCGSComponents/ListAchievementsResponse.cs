using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListAchievementsResponse : BCGSTypedResponse
{
	public class _Achievement : BCGSTypedResponse
	{
		public string Description => response.GetString("description");

		public bool? Earned => response.GetBoolean("earned");

		public string Name => response.GetString("name");

		public BCGSData PropertySet => response.GetObject("propertySet");

		public string ShortCode => response.GetString("shortCode");

		public _Achievement(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_Achievement> Achievements => new BCGSEnumerable<_Achievement>(response.GetObjectList("achievements"), (BCGSData data) => new _Achievement(data));

	public ListAchievementsResponse(BCGSData data)
		: base(data)
	{
	}
}
