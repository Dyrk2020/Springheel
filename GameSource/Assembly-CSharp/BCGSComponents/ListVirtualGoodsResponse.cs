using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListVirtualGoodsResponse : BCGSTypedResponse
{
	public class _VirtualGood : BCGSTypedResponse
	{
		public class _BundledGood : BCGSTypedResponse
		{
			public int? Qty => response.GetInt("qty");

			public string ShortCode => response.GetString("shortCode");

			public _BundledGood(BCGSData data)
				: base(data)
			{
			}
		}

		public string WP8StoreProductId => response.GetString("WP8StoreProductId");

		public string AmazonStoreProductId => response.GetString("amazonStoreProductId");

		public long? BaseCurrency1Cost => response.GetLong("baseCurrency1Cost");

		public long? BaseCurrency2Cost => response.GetLong("baseCurrency2Cost");

		public long? BaseCurrency3Cost => response.GetLong("baseCurrency3Cost");

		public long? BaseCurrency4Cost => response.GetLong("baseCurrency4Cost");

		public long? BaseCurrency5Cost => response.GetLong("baseCurrency5Cost");

		public long? BaseCurrency6Cost => response.GetLong("baseCurrency6Cost");

		public BCGSData BaseCurrencyCosts => response.GetObject("baseCurrencyCosts");

		public BCGSEnumerable<_BundledGood> BundledGoods => new BCGSEnumerable<_BundledGood>(response.GetObjectList("bundledGoods"), (BCGSData data) => new _BundledGood(data));

		public long? Currency1Cost => response.GetLong("currency1Cost");

		public long? Currency2Cost => response.GetLong("currency2Cost");

		public long? Currency3Cost => response.GetLong("currency3Cost");

		public long? Currency4Cost => response.GetLong("currency4Cost");

		public long? Currency5Cost => response.GetLong("currency5Cost");

		public long? Currency6Cost => response.GetLong("currency6Cost");

		public BCGSData CurrencyCosts => response.GetObject("currencyCosts");

		public string Description => response.GetString("description");

		public bool? Disabled => response.GetBoolean("disabled");

		public string GooglePlayProductId => response.GetString("googlePlayProductId");

		public string IosAppStoreProductId => response.GetString("iosAppStoreProductId");

		public long? MaxQuantity => response.GetLong("maxQuantity");

		public string Name => response.GetString("name");

		public BCGSData PropertySet => response.GetObject("propertySet");

		public string PsnStoreProductId => response.GetString("psnStoreProductId");

		public long? SegmentedCurrency1Cost => response.GetLong("segmentedCurrency1Cost");

		public long? SegmentedCurrency2Cost => response.GetLong("segmentedCurrency2Cost");

		public long? SegmentedCurrency3Cost => response.GetLong("segmentedCurrency3Cost");

		public long? SegmentedCurrency4Cost => response.GetLong("segmentedCurrency4Cost");

		public long? SegmentedCurrency5Cost => response.GetLong("segmentedCurrency5Cost");

		public long? SegmentedCurrency6Cost => response.GetLong("segmentedCurrency6Cost");

		public BCGSData SegmentedCurrencyCosts => response.GetObject("segmentedCurrencyCosts");

		public string ShortCode => response.GetString("shortCode");

		public string SteamStoreProductId => response.GetString("steamStoreProductId");

		public string Tags => response.GetString("tags");

		public string Type => response.GetString("type");

		public string W8StoreProductId => response.GetString("w8StoreProductId");

		public _VirtualGood(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_VirtualGood> VirtualGoods => new BCGSEnumerable<_VirtualGood>(response.GetObjectList("virtualGoods"), (BCGSData data) => new _VirtualGood(data));

	public ListVirtualGoodsResponse(BCGSData data)
		: base(data)
	{
	}
}
