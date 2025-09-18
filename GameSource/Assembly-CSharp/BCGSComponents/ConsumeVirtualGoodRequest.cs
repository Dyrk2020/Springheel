using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ConsumeVirtualGoodRequest : BCGSTypedRequest<ConsumeVirtualGoodRequest, ConsumeVirtualGoodResponse>
{
	public ConsumeVirtualGoodRequest()
		: base("ConsumeVirtualGoodRequest")
	{
	}

	public ConsumeVirtualGoodRequest(BCGSInstance instance)
		: base(instance, "ConsumeVirtualGoodRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ConsumeVirtualGoodResponse(response);
	}

	public ConsumeVirtualGoodRequest SetQuantity(long quantity)
	{
		request.AddNumber("quantity", quantity);
		return this;
	}

	public ConsumeVirtualGoodRequest SetShortCode(string shortCode)
	{
		request.AddString("shortCode", shortCode);
		return this;
	}
}
