using System;
using System.Collections.Generic;
using System.Threading;

namespace SuperSocket.ClientEngine;

public class ConcurrentBatchQueue<T> : IBatchQueue<T>
{
	private class Entity
	{
		public int Count;

		public T[] Array { get; set; }
	}

	private object m_Entity;

	private object m_BackEntity;

	private static readonly T m_Null = default(T);

	private Func<T, bool> m_NullValidator;

	private bool m_Rebuilding;

	public bool IsEmpty => ((Entity)m_Entity).Count <= 0;

	public int Count => ((Entity)m_Entity).Count;

	public ConcurrentBatchQueue()
		: this(16)
	{
	}

	public ConcurrentBatchQueue(int capacity)
		: this(new T[capacity])
	{
	}

	public ConcurrentBatchQueue(int capacity, Func<T, bool> nullValidator)
		: this(new T[capacity], nullValidator)
	{
	}

	public ConcurrentBatchQueue(T[] array)
		: this(array, (Func<T, bool>)((T t) => t == null))
	{
	}

	public ConcurrentBatchQueue(T[] array, Func<T, bool> nullValidator)
	{
		m_Entity = new Entity();
		((Entity)m_Entity).Array = array;
		m_BackEntity = new Entity();
		((Entity)m_BackEntity).Array = new T[array.Length];
		m_NullValidator = nullValidator;
	}

	public bool Enqueue(T item)
	{
		bool full;
		while (!TryEnqueue(item, out full) && !full)
		{
		}
		return !full;
	}

	private bool TryEnqueue(T item, out bool full)
	{
		full = false;
		EnsureNotRebuild();
		Entity entity = (Entity)m_Entity;
		T[] array = entity.Array;
		int count = entity.Count;
		if (count >= array.Length)
		{
			full = true;
			return false;
		}
		if (entity != m_Entity)
		{
			return false;
		}
		int num = Interlocked.CompareExchange(ref entity.Count, count + 1, count);
		if (num != count)
		{
			return false;
		}
		array[count] = item;
		return true;
	}

	public bool Enqueue(IList<T> items)
	{
		bool full;
		while (!TryEnqueue(items, out full) && !full)
		{
		}
		return !full;
	}

	private bool TryEnqueue(IList<T> items, out bool full)
	{
		full = false;
		Entity entity = (Entity)m_Entity;
		T[] array = entity.Array;
		int count = entity.Count;
		int count2 = items.Count;
		int num = count + count2;
		if (num > array.Length)
		{
			full = true;
			return false;
		}
		if (entity != m_Entity)
		{
			return false;
		}
		int num2 = Interlocked.CompareExchange(ref entity.Count, num, count);
		if (num2 != count)
		{
			return false;
		}
		foreach (T item in items)
		{
			array[count++] = item;
		}
		return true;
	}

	private void EnsureNotRebuild()
	{
		if (m_Rebuilding)
		{
			do
			{
				Thread.SpinWait(1);
			}
			while (m_Rebuilding);
		}
	}

	public bool TryDequeue(IList<T> outputItems)
	{
		Entity entity = (Entity)m_Entity;
		int count = entity.Count;
		if (count <= 0)
		{
			return false;
		}
		Interlocked.Exchange(ref m_Entity, m_BackEntity);
		Thread.SpinWait(1);
		count = entity.Count;
		T[] array = entity.Array;
		int num = 0;
		while (true)
		{
			T arg = array[num];
			while (m_NullValidator(arg))
			{
				Thread.SpinWait(1);
				arg = array[num];
			}
			outputItems.Add(array[num]);
			array[num] = m_Null;
			if (entity.Count <= num + 1)
			{
				break;
			}
			num++;
		}
		m_BackEntity = entity;
		((Entity)m_BackEntity).Count = 0;
		return true;
	}
}
