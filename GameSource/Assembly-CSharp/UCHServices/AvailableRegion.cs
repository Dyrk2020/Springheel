using System;
using I2.Loc;

namespace UCHServices;

[Serializable]
public class AvailableRegion
{
	public string id;

	public string queryAddress;

	public string name;

	public string shortName;

	[NonSerialized]
	public int ping = -1;

	private string nameKey;

	private string shortNameKey;

	public string LocalizedName
	{
		get
		{
			if (string.IsNullOrEmpty(nameKey))
			{
				nameKey = "Network/" + name;
			}
			return LocalizationManager.GetTermTranslation(nameKey);
		}
	}

	public string LocalizedShortName
	{
		get
		{
			if (string.IsNullOrEmpty(shortNameKey))
			{
				shortNameKey = "Network/" + shortName;
			}
			return LocalizationManager.GetTermTranslation(shortNameKey);
		}
	}
}
