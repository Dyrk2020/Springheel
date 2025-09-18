using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListVirtualGoodsRequest : BCGSTypedRequest<ListVirtualGoodsRequest, ListVirtualGoodsResponse>
{
	public ListVirtualGoodsRequest()
		: base("ListVirtualGoodsRequest")
	{
	}

	public ListVirtualGoodsRequest(BCGSInstance instance)
		: base(instance, "ListVirtualGoodsRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new ListVirtualGoodsResponse(response);
	}

	public ListVirtualGoodsRequest SetIncludeDisabled(bool includeDisabled)
	{
		request.AddBoolean("includeDisabled", includeDisabled);
		return this;
	}

	public ListVirtualGoodsRequest SetTags(List<string> tags)
	{
		request.AddStringList("tags", tags);
		return this;
	}
}
