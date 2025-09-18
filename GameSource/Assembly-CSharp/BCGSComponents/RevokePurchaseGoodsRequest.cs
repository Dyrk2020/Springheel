using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class RevokePurchaseGoodsRequest : BCGSTypedRequest<RevokePurchaseGoodsRequest, RevokePurchaseGoodsResponse>
{
	public RevokePurchaseGoodsRequest()
		: base("RevokePurchaseGoodsRequest")
	{
	}

	public RevokePurchaseGoodsRequest(BCGSInstance instance)
		: base(instance, "RevokePurchaseGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new RevokePurchaseGoodsResponse(response);
	}

	public RevokePurchaseGoodsRequest SetPlayerId(string playerId)
	{
		request.AddString("playerId", playerId);
		return this;
	}

	public RevokePurchaseGoodsRequest SetStoreType(string storeType)
	{
		request.AddString("storeType", storeType);
		return this;
	}

	public RevokePurchaseGoodsRequest SetTransactionIds(List<string> transactionIds)
	{
		request.AddStringList("transactionIds", transactionIds);
		return this;
	}
}
