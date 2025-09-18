using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AmazonBuyGoodsRequest : BCGSTypedRequest<AmazonBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public AmazonBuyGoodsRequest()
		: base("AmazonBuyGoodsRequest")
	{
	}

	public AmazonBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "AmazonBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public AmazonBuyGoodsRequest SetAmazonUserId(string amazonUserId)
	{
		request.AddString("amazonUserId", amazonUserId);
		return this;
	}

	public AmazonBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public AmazonBuyGoodsRequest SetReceiptId(string receiptId)
	{
		request.AddString("receiptId", receiptId);
		return this;
	}

	public AmazonBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public AmazonBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
