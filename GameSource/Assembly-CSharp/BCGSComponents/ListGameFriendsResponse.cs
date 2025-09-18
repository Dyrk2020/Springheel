using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListGameFriendsResponse : BCGSTypedResponse
{
	public class _Player : BCGSTypedResponse
	{
		public List<string> Achievements => response.GetStringList("achievements");

		public string DisplayName => response.GetString("displayName");

		public BCGSData ExternalIds => response.GetObject("externalIds");

		public string Id => response.GetString("id");

		public bool? Online => response.GetBoolean("online");

		public List<string> VirtualGoods => response.GetStringList("virtualGoods");

		public _Player(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_Player> Friends => new BCGSEnumerable<_Player>(response.GetObjectList("friends"), (BCGSData data) => new _Player(data));

	public ListGameFriendsResponse(BCGSData data)
		: base(data)
	{
	}
}
