using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class SocialStatusResponse : BCGSTypedResponse
{
	public class _SocialStatus : BCGSTypedResponse
	{
		public bool? Active => response.GetBoolean("active");

		public DateTime? Expires => response.GetDate("expires");

		public string SystemId => response.GetString("systemId");

		public _SocialStatus(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_SocialStatus> Statuses => new BCGSEnumerable<_SocialStatus>(response.GetObjectList("statuses"), (BCGSData data) => new _SocialStatus(data));

	public SocialStatusResponse(BCGSData data)
		: base(data)
	{
	}
}
