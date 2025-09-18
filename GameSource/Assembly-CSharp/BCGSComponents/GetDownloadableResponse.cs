using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class GetDownloadableResponse : BCGSTypedResponse
{
	public DateTime? LastModified => response.GetDate("lastModified");

	public string ShortCode => response.GetString("shortCode");

	public long? Size => response.GetLong("size");

	public string Url => response.GetString("url");

	public GetDownloadableResponse(BCGSData data)
		: base(data)
	{
	}
}
