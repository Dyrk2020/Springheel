using BCGSComponents.DataModels;

namespace BCGSComponents;

public class XboxOneBuyGoodsRequest : BCGSTypedRequest<XboxOneBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public XboxOneBuyGoodsRequest()
		: base("XboxOneBuyGoodsRequest")
	{
	}

	public XboxOneBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "XboxOneBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public XboxOneBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public XboxOneBuyGoodsRequest SetItemId(string itemId)
	{
		request.AddString("itemId", itemId);
		return this;
	}

	public XboxOneBuyGoodsRequest SetItemsConsumed(long itemsConsumed)
	{
		request.AddNumber("itemsConsumed", itemsConsumed);
		return this;
	}

	public XboxOneBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public XboxOneBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
