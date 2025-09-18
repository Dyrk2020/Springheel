using System;
using System.Linq;
using System.Reflection;

namespace Assets.FrontendScripts;

public static class EnumsHelper
{
	public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
	{
		return enumVal.GetType().GetTypeInfo().DeclaredMembers.First((MemberInfo x) => x.Name == enumVal.ToString()).GetCustomAttribute<T>();
	}

	public static string GetDescription(this Enum enumVal)
	{
		MyDescriptionAttribute attributeOfType = enumVal.GetAttributeOfType<MyDescriptionAttribute>();
		if (attributeOfType == null)
		{
			return string.Empty;
		}
		return attributeOfType.Text;
	}
}
