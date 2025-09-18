using System;
using System.Collections;
using System.Collections.Generic;

namespace BrainCloud.Entity.Internal;

internal class DictionaryWrapperEnumerator<TValue, SValue> : IEnumerator<KeyValuePair<string, TValue>>, IEnumerator, IDisposable
{
	private IEnumerator<KeyValuePair<string, SValue>> m_sourceEnumerator;

	public KeyValuePair<string, TValue> Current
	{
		get
		{
			KeyValuePair<string, SValue> current = m_sourceEnumerator.Current;
			return new KeyValuePair<string, TValue>(current.Key, EntityUtil.GetObjectAsType<TValue>(current.Value));
		}
	}

	object IEnumerator.Current => m_sourceEnumerator.Current;

	public DictionaryWrapperEnumerator(IEnumerator<KeyValuePair<string, SValue>> sourceEnumerator)
	{
		m_sourceEnumerator = sourceEnumerator;
	}

	public void Dispose()
	{
		m_sourceEnumerator.Dispose();
	}

	bool IEnumerator.MoveNext()
	{
		return m_sourceEnumerator.MoveNext();
	}

	void IEnumerator.Reset()
	{
		m_sourceEnumerator.Reset();
	}
}
