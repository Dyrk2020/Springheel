using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListTeamChatResponse : BCGSTypedResponse
{
	public class _ChatMessage : BCGSTypedResponse
	{
		public string FromId => response.GetString("fromId");

		public string Id => response.GetString("id");

		public string Message => response.GetString("message");

		public DateTime? When => response.GetDate("when");

		public string Who => response.GetString("who");

		public _ChatMessage(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_ChatMessage> Messages => new BCGSEnumerable<_ChatMessage>(response.GetObjectList("messages"), (BCGSData data) => new _ChatMessage(data));

	public ListTeamChatResponse(BCGSData data)
		: base(data)
	{
	}
}
