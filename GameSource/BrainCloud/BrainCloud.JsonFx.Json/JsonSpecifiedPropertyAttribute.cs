using System;
using System.Reflection;

namespace BrainCloud.JsonFx.Json;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class JsonSpecifiedPropertyAttribute : Attribute
{
	private string specifiedProperty;

	public string SpecifiedProperty
	{
		get
		{
			return specifiedProperty;
		}
		set
		{
			specifiedProperty = value;
		}
	}

	public JsonSpecifiedPropertyAttribute(string propertyName)
	{
		specifiedProperty = propertyName;
	}

	public static string GetJsonSpecifiedProperty(MemberInfo memberInfo)
	{
		if (memberInfo == null || !Attribute.IsDefined(memberInfo, typeof(JsonSpecifiedPropertyAttribute)))
		{
			return null;
		}
		return ((JsonSpecifiedPropertyAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(JsonSpecifiedPropertyAttribute))).SpecifiedProperty;
	}
}
