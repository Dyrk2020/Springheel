using BCGSComponents.DataModels;

namespace BCGSComponents;

public class RevokePurchaseGoodsResponse : BCGSTypedResponse
{
	public BCGSData RevokedGoods => response.GetObject("revokedGoods");

	public RevokePurchaseGoodsResponse(BCGSData data)
		: base(data)
	{
	}
}
