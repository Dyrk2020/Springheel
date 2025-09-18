using System.Collections.Generic;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Common;

public class ACL
{
	public enum Access
	{
		None,
		ReadOnly,
		ReadWrite
	}

	public Access Other { get; set; }

	public ACL()
	{
	}

	public ACL(Access access)
	{
		Other = access;
	}

	public static ACL None()
	{
		return new ACL
		{
			Other = Access.None
		};
	}

	public static ACL ReadOnly()
	{
		return new ACL
		{
			Other = Access.ReadOnly
		};
	}

	public static ACL ReadWrite()
	{
		return new ACL
		{
			Other = Access.ReadWrite
		};
	}

	public static ACL CreateFromJson(Dictionary<string, object> json)
	{
		ACL aCL = new ACL();
		aCL.ReadFromJson(json);
		return aCL;
	}

	public void ReadFromJson(Dictionary<string, object> json)
	{
		Other = (Access)(int)json["other"];
	}

	public string ToJsonString()
	{
		return JsonWriter.Serialize(new Dictionary<string, object> { 
		{
			"other",
			(int)Other
		} });
	}
}
