using System.Collections.Generic;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Common;

public class GroupACL
{
	public enum Access
	{
		None,
		ReadOnly,
		ReadWrite
	}

	public Access Other { get; set; }

	public Access Member { get; set; }

	public GroupACL()
	{
	}

	public GroupACL(Access otherAccess, Access memberAccess)
	{
		Other = otherAccess;
		Member = memberAccess;
	}

	public static GroupACL CreateFromJson(string json)
	{
		GroupACL groupACL = new GroupACL();
		groupACL.ReadFromJson(json);
		return groupACL;
	}

	public void ReadFromJson(string json)
	{
		Dictionary<string, object> dictionary = JsonReader.Deserialize<Dictionary<string, object>>(json);
		Other = (Access)dictionary["other"];
		Member = (Access)dictionary["member"];
	}

	public string ToJsonString()
	{
		return JsonWriter.Serialize(new Dictionary<string, object>
		{
			{
				"other",
				(int)Other
			},
			{
				"member",
				(int)Member
			}
		});
	}
}
