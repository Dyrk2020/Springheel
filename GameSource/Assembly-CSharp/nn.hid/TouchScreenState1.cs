using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.hid;

public struct TouchScreenState1
{
	public struct TouchStateArray1 : IList<TouchState>, ICollection<TouchState>, IEnumerable<TouchState>, IEnumerable
	{
		private const int _Length = 1;

		private TouchState _value0;

		public int Length => 1;

		public TouchState this[int index]
		{
			get
			{
				if (index == 0)
				{
					return _value0;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				if (index == 0)
				{
					_value0 = value;
					return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(TouchState item)
		{
			for (int i = 0; i < Length; i++)
			{
				if (this[i] == item)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(TouchState item)
		{
			for (int i = 0; i < Length; i++)
			{
				if (this[i] == item)
				{
					return i;
				}
			}
			return -1;
		}

		public void CopyTo(TouchState[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (arrayIndex + Length < array.Length)
			{
				throw new ArgumentException();
			}
			for (int i = 0; i < Length; i++)
			{
				array[arrayIndex + i] = this[i];
			}
		}

		public override string ToString()
		{
			return $"{{{_value0}}}";
		}

		public IEnumerator<TouchState> GetEnumerator()
		{
			yield return _value0;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(TouchState item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, TouchState item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(TouchState item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public const int TouchCount = 1;

	public long samplingNumber;

	public int count;

	private int _reserved;

	public TouchStateArray1 touches;

	public void SetDefault()
	{
		touches = default(TouchStateArray1);
	}
}
