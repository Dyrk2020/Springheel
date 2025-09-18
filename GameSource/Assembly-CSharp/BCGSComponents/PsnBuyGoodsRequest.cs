using BCGSComponents.DataModels;

namespace BCGSComponents;

public class PsnBuyGoodsRequest : BCGSTypedRequest<PsnBuyGoodsRequest, BuyVirtualGoodResponse>
{
	public PsnBuyGoodsRequest()
		: base("PsnBuyGoodsRequest")
	{
	}

	public PsnBuyGoodsRequest(BCGSInstance instance)
		: base(instance, "PsnBuyGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new BuyVirtualGoodResponse(response);
	}

	public PsnBuyGoodsRequest SetAuthorizationCode(string authorizationCode)
	{
		request.AddString("authorizationCode", authorizationCode);
		return this;
	}

	public PsnBuyGoodsRequest SetCurrencyCode(string currencyCode)
	{
		request.AddString("currencyCode", currencyCode);
		return this;
	}

	public PsnBuyGoodsRequest SetEntitlementLabel(string entitlementLabel)
	{
		request.AddString("entitlementLabel", entitlementLabel);
		return this;
	}

	public PsnBuyGoodsRequest SetRedirectUri(string redirectUri)
	{
		request.AddString("redirectUri", redirectUri);
		return this;
	}

	public PsnBuyGoodsRequest SetSubUnitPrice(double subUnitPrice)
	{
		request.AddNumber("subUnitPrice", subUnitPrice);
		return this;
	}

	public PsnBuyGoodsRequest SetUniqueTransactionByPlayer(bool uniqueTransactionByPlayer)
	{
		request.AddBoolean("uniqueTransactionByPlayer", uniqueTransactionByPlayer);
		return this;
	}

	public PsnBuyGoodsRequest SetUseCount(long useCount)
	{
		request.AddNumber("useCount", useCount);
		return this;
	}
}
