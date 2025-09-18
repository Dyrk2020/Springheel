using System;
using System.Collections.Generic;

namespace BrainCloud.Entity.Internal;

internal class EntityUtil
{
	public static T GetObjectAsType<T>(object value)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsGenericType)
		{
			if (typeFromHandle.GetGenericTypeDefinition() == typeof(IList<>))
			{
				Type[] typeArguments = new Type[2]
				{
					typeFromHandle.GetGenericArguments()[0],
					value.GetType().GetGenericArguments()[0]
				};
				object[] parameters = new object[1] { value };
				return (T)typeof(ListWrapper<, >).MakeGenericType(typeArguments).GetConstructor(new Type[1] { value.GetType() }).Invoke(parameters);
			}
			if (typeFromHandle.GetGenericTypeDefinition() == typeof(IDictionary<, >))
			{
				Type[] typeArguments2 = new Type[2]
				{
					typeFromHandle.GetGenericArguments()[1],
					value.GetType().GetGenericArguments()[1]
				};
				object[] parameters2 = new object[1] { value };
				return (T)typeof(DictionaryWrapper<, >).MakeGenericType(typeArguments2).GetConstructor(new Type[1] { value.GetType() }).Invoke(parameters2);
			}
		}
		try
		{
			return (T)value;
		}
		catch (InvalidCastException)
		{
			return (T)Convert.ChangeType(value, typeof(T));
		}
	}
}
