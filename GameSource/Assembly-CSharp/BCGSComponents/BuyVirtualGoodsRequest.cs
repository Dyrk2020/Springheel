using BCGSComponents.DataModels;

namespace BCGSComponents;

public class BuyVirtualGoodsRequest : BCGSTypedRequest<BuyVirtualGoodsRequest, BuyVirtualGoodResponse>
{
	public BuyVirtualGoodsRequest()
		: base("BuyVirtualGoodsRequest")
	{
	}

	public BuyVirtualGoodsRequest(BCGSInstance instance)
		: base(instance, "BuyVirtualGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public BuyVirtualGoodsRequest SetCurrencyShortCode(string currencyShortCode)
	{
		request.AddString("currencyShortCode", currencyShortCode);
		return this;
	}

	public BuyVirtualGoodsRequest SetCurrencyType(long currencyType)
	{
		request.AddNumber("currencyType", currencyType);
		return this;
	}

	public BuyVirtualGoodsRequest SetQuantity(long quantity)
	{
		request.AddNumber("quantity", quantity);
		return this;
	}

	public BuyVirtualGoodsRequest SetShortCode(string shortCode)
	{
		request.AddString("shortCode", shortCode);
		return this;
	}
}
