using BCGSComponents.DataModels;

namespace BCGSComponents;

public class WindowsBuyGoodsRequest : BCGSTypedRequest<WindowsBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public WindowsBuyGoodsRequest()
		: base("WindowsBuyGoodsRequest")
	{
	}

	public WindowsBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "WindowsBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public WindowsBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public WindowsBuyGoodsRequest SetPlatform(string platform)
	{
		request.AddString("platform", platform);
		return this;
	}

	public WindowsBuyGoodsRequest SetReceipt(string receipt)
	{
		request.AddString("receipt", receipt);
		return this;
	}

	public WindowsBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public WindowsBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
