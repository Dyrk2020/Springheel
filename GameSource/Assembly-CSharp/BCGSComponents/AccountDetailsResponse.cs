using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AccountDetailsResponse : BCGSTypedResponse
{
	public class _Location : BCGSTypedResponse
	{
		public string City => response.GetString("city");

		public string Country => response.GetString("country");

		public double? Latitide => response.GetFloat("latitide");

		public double? Longditute => response.GetFloat("longditute");

		public _Location(BCGSData data)
			: base(data)
		{
		}
	}

	public List<string> Achievements => response.GetStringList("achievements");

	public BCGSData Currencies => response.GetObject("currencies");

	public long? Currency1 => response.GetInt("currency1");

	public long? Currency2 => response.GetInt("currency2");

	public long? Currency3 => response.GetInt("currency3");

	public long? Currency4 => response.GetInt("currency4");

	public long? Currency5 => response.GetInt("currency5");

	public long? Currency6 => response.GetInt("currency6");

	public string DisplayName => response.GetString("displayName");

	public BCGSData ExternalIds => response.GetObject("externalIds");

	public _Location Location
	{
		get
		{
			if (response.GetObject("location") == null)
			{
				return null;
			}
			return new _Location(response.GetObject("location"));
		}
	}

	public BCGSData ReservedCurrencies => response.GetObject("reservedCurrencies");

	public BCGSData ReservedCurrency1 => response.GetObject("reservedCurrency1");

	public BCGSData ReservedCurrency2 => response.GetObject("reservedCurrency2");

	public BCGSData ReservedCurrency3 => response.GetObject("reservedCurrency3");

	public BCGSData ReservedCurrency4 => response.GetObject("reservedCurrency4");

	public BCGSData ReservedCurrency5 => response.GetObject("reservedCurrency5");

	public BCGSData ReservedCurrency6 => response.GetObject("reservedCurrency6");

	public string UserId => response.GetString("userId");

	public BCGSData VirtualGoods => response.GetObject("virtualGoods");

	public AccountDetailsResponse(BCGSData data)
		: base(data)
	{
	}
}
