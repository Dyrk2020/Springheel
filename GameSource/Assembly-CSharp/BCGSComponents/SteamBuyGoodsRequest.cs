using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SteamBuyGoodsRequest : BCGSTypedRequest<SteamBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public SteamBuyGoodsRequest()
		: base("SteamBuyGoodsRequest")
	{
	}

	public SteamBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "SteamBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public SteamBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public SteamBuyGoodsRequest SetOrderId(string orderId)
	{
		request.AddString("orderId", orderId);
		return this;
	}

	public SteamBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public SteamBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
