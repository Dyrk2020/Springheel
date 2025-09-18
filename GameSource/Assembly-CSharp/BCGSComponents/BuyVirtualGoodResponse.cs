using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class BuyVirtualGoodResponse : BCGSTypedResponse
{
	public class _Boughtitem : BCGSTypedResponse
	{
		public long? Quantity => response.GetLong("quantity");

		public string ShortCode => response.GetString("shortCode");

		public _Boughtitem(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_Boughtitem> BoughtItems => new BCGSEnumerable<_Boughtitem>(response.GetObjectList("boughtItems"), (BCGSData data) => new _Boughtitem(data));

	public BCGSData CurrenciesAdded => response.GetObject("currenciesAdded");

	public long? Currency1Added => response.GetLong("currency1Added");

	public long? Currency2Added => response.GetLong("currency2Added");

	public long? Currency3Added => response.GetLong("currency3Added");

	public long? Currency4Added => response.GetLong("currency4Added");

	public long? Currency5Added => response.GetLong("currency5Added");

	public long? Currency6Added => response.GetLong("currency6Added");

	public long? CurrencyConsumed => response.GetLong("currencyConsumed");

	public string CurrencyShortCode => response.GetString("currencyShortCode");

	public int? CurrencyType => response.GetInt("currencyType");

	public List<string> InvalidItems => response.GetStringList("invalidItems");

	public List<string> TransactionIds => response.GetStringList("transactionIds");

	public BuyVirtualGoodResponse(BCGSData data)
		: base(data)
	{
	}
}
