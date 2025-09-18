using BCGSComponents.DataModels;

namespace BCGSComponents;

public class IOSBuyGoodsRequest : BCGSTypedRequest<IOSBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public IOSBuyGoodsRequest()
		: base("IOSBuyGoodsRequest")
	{
	}

	public IOSBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "IOSBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public IOSBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public IOSBuyGoodsRequest SetReceipt(string receipt)
	{
		request.AddString("receipt", receipt);
		return this;
	}

	public IOSBuyGoodsRequest SetSandbox(bool sandbox)
	{
		request.AddBoolean("sandbox", sandbox);
		return this;
	}

	public IOSBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public IOSBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
