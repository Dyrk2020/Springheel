using System.Collections;
using System.Collections.Generic;

namespace BrainCloud.Entity.Internal;

internal class ListWrapper<T, S> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private IList<S> m_sourceList;

	public T this[int index]
	{
		get
		{
			return EntityUtil.GetObjectAsType<T>(m_sourceList[index]);
		}
		set
		{
			m_sourceList[index] = (S)(object)value;
		}
	}

	public int Count => m_sourceList.Count;

	public bool IsReadOnly => m_sourceList.IsReadOnly;

	protected ListWrapper()
	{
	}

	public ListWrapper(IList<S> sourceList)
	{
		m_sourceList = sourceList;
	}

	public int IndexOf(T item)
	{
		return m_sourceList.IndexOf((S)(object)item);
	}

	public void Insert(int index, T item)
	{
		m_sourceList.Insert(index, (S)(object)item);
	}

	public void RemoveAt(int index)
	{
		m_sourceList.RemoveAt(index);
	}

	public void Add(T item)
	{
		m_sourceList.Add((S)(object)item);
	}

	public void Clear()
	{
		m_sourceList.Clear();
	}

	public bool Contains(T item)
	{
		return m_sourceList.Contains((S)(object)item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		int num = array.Length;
		S[] array2 = new S[num];
		m_sourceList.CopyTo(array2, arrayIndex);
		for (int i = 0; i < num; i++)
		{
			array[i] = EntityUtil.GetObjectAsType<T>(array2[i]);
		}
	}

	public bool Remove(T item)
	{
		return m_sourceList.Remove((S)(object)item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new ListWrapperEnumerator<T, S>(m_sourceList.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new ListWrapperEnumerator<T, S>(m_sourceList.GetEnumerator());
	}
}
