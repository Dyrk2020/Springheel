using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListMessageDetailResponse : BCGSTypedResponse
{
	public class _PlayerMessage : BCGSTypedResponse
	{
		public string Id => response.GetString("id");

		public BCGSData Message => response.GetObject("message");

		public bool? Seen => response.GetBoolean("seen");

		public string Status => response.GetString("status");

		public DateTime? When => response.GetDate("when");

		public _PlayerMessage(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_PlayerMessage> MessageList => new BCGSEnumerable<_PlayerMessage>(response.GetObjectList("messageList"), (BCGSData data) => new _PlayerMessage(data));

	public ListMessageDetailResponse(BCGSData data)
		: base(data)
	{
	}
}
