using System;
using System.Collections;
using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class BCGSEnumerable<T> : IEnumerable<T>, IEnumerable
{
	private List<object> m_list;

	private Func<BCGSData, T> creator;

	public BCGSEnumerable(List<object> data, Func<BCGSData, T> creator)
	{
		if (data != null)
		{
			m_list = data;
		}
		else
		{
			m_list = new List<object>();
		}
		this.creator = creator;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (object item in m_list)
		{
			if (item is IDictionary<string, object>)
			{
				yield return creator(new BCGSData((IDictionary<string, object>)item));
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
