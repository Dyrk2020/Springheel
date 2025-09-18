using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.hid;

public struct TouchScreenState2
{
	public struct TouchStateArray2 : IList<TouchState>, ICollection<TouchState>, IEnumerable<TouchState>, IEnumerable
	{
		private const int _Length = 2;

		private TouchState _value0;

		private TouchState _value1;

		public int Length => 2;

		public TouchState this[int index]
		{
			get
			{
				return index switch
				{
					0 => _value0, 
					1 => _value1, 
					_ => throw new IndexOutOfRangeException(), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					_value0 = value;
					break;
				case 1:
					_value1 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
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
			return $"{{{_value0},{_value1}}}";
		}

		public IEnumerator<TouchState> GetEnumerator()
		{
			yield return _value0;
			yield return _value1;
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

	public const int TouchCount = 2;

	public long samplingNumber;

	public int count;

	private int _reserved;

	public TouchStateArray2 touches;

	public void SetDefault()
	{
		touches = default(TouchStateArray2);
	}
}
