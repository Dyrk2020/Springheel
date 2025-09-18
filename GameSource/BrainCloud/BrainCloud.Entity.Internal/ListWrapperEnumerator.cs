using System;
using System.Collections;
using System.Collections.Generic;

namespace BrainCloud.Entity.Internal;

internal class ListWrapperEnumerator<T, S> : IEnumerator<T>, IEnumerator, IDisposable
{
	private IEnumerator<S> m_sourceEnumerator;

	public T Current => EntityUtil.GetObjectAsType<T>(m_sourceEnumerator.Current);

	object IEnumerator.Current => m_sourceEnumerator.Current;

	public ListWrapperEnumerator(IEnumerator<S> sourceEnumerator)
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
