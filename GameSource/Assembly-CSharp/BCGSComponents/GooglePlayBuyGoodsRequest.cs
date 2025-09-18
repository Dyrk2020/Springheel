using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GooglePlayBuyGoodsRequest : BCGSTypedRequest<GooglePlayBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public GooglePlayBuyGoodsRequest()
		: base("GooglePlayBuyGoodsRequest")
	{
	}

	public GooglePlayBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "GooglePlayBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public GooglePlayBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public GooglePlayBuyGoodsRequest SetSignature(string signature)
	{
		request.AddString("signature", signature);
		return this;
	}

	public GooglePlayBuyGoodsRequest SetSignedData(string signedData)
	{
		request.AddString("signedData", signedData);
		return this;
	}

	public GooglePlayBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public GooglePlayBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}
}
