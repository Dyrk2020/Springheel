using System.Globalization;

namespace BrainCloud;

public class RegionLocale
{
	protected static string m_countryLocale = "";

	public static string UsersCountryLocale
	{
		get
		{
			if (m_countryLocale == "")
			{
				GetCountryLocale();
			}
			return m_countryLocale;
		}
	}

	protected static void GetCountryLocale()
	{
		m_countryLocale = RegionInfo.CurrentRegion.ToString();
	}
}
