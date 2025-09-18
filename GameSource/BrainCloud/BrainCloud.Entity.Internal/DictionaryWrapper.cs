using System.Collections;
using System.Collections.Generic;

namespace BrainCloud.Entity.Internal;

internal class DictionaryWrapper<TValue, SValue> : IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
{
	private IDictionary<string, SValue> m_sourceDictionary;

	public ICollection<string> Keys => m_sourceDictionary.Keys;

	public ICollection<TValue> Values
	{
		get
		{
			List<TValue> list = new List<TValue>();
			foreach (SValue value in m_sourceDictionary.Values)
			{
				list.Add(EntityUtil.GetObjectAsType<TValue>(value));
			}
			return list;
		}
	}

	public TValue this[string key]
	{
		get
		{
			return EntityUtil.GetObjectAsType<TValue>(m_sourceDictionary[key]);
		}
		set
		{
			m_sourceDictionary[key] = (SValue)(object)value;
		}
	}

	public int Count => m_sourceDictionary.Count;

	public bool IsReadOnly => m_sourceDictionary.IsReadOnly;

	protected DictionaryWrapper()
	{
	}

	public DictionaryWrapper(IDictionary<string, SValue> sourceDictionary)
	{
		m_sourceDictionary = sourceDictionary;
	}

	public void Add(string key, TValue value)
	{
		m_sourceDictionary.Add(key, (SValue)(object)value);
	}

	public bool ContainsKey(string key)
	{
		return m_sourceDictionary.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		return m_sourceDictionary.Remove(key);
	}

	public bool TryGetValue(string key, out TValue value)
	{
		if (ContainsKey(key))
		{
			value = this[key];
			return true;
		}
		value = default(TValue);
		return false;
	}

	public void Add(KeyValuePair<string, TValue> item)
	{
		KeyValuePair<string, SValue> item2 = new KeyValuePair<string, SValue>(item.Key, (SValue)(object)item.Value);
		m_sourceDictionary.Add(item2);
	}

	public void Clear()
	{
		m_sourceDictionary.Clear();
	}

	public bool Contains(KeyValuePair<string, TValue> item)
	{
		KeyValuePair<string, SValue> item2 = new KeyValuePair<string, SValue>(item.Key, (SValue)(object)item.Value);
		return m_sourceDictionary.Contains(item2);
	}

	public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
	{
		int num = array.Length;
		KeyValuePair<string, SValue>[] array2 = new KeyValuePair<string, SValue>[num];
		m_sourceDictionary.CopyTo(array2, arrayIndex);
		for (int i = 0; i < num; i++)
		{
			array[i] = new KeyValuePair<string, TValue>(array2[i].Key, EntityUtil.GetObjectAsType<TValue>(array2[i].Value));
		}
	}

	public bool Remove(KeyValuePair<string, TValue> item)
	{
		KeyValuePair<string, SValue> item2 = new KeyValuePair<string, SValue>(item.Key, (SValue)(object)item.Value);
		return m_sourceDictionary.Remove(item2);
	}

	public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
	{
		return new DictionaryWrapperEnumerator<TValue, SValue>(m_sourceDictionary.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new DictionaryWrapperEnumerator<TValue, SValue>(m_sourceDictionary.GetEnumerator());
	}
}
