using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.hid;

public struct TouchScreenState12
{
	public struct TouchStateArray12 : IList<TouchState>, ICollection<TouchState>, IEnumerable<TouchState>, IEnumerable
	{
		private const int _Length = 12;

		private TouchState _value0;

		private TouchState _value1;

		private TouchState _value2;

		private TouchState _value3;

		private TouchState _value4;

		private TouchState _value5;

		private TouchState _value6;

		private TouchState _value7;

		private TouchState _value8;

		private TouchState _value9;

		private TouchState _value10;

		private TouchState _value11;

		public int Length => 12;

		public TouchState this[int index]
		{
			get
			{
				return index switch
				{
					0 => _value0, 
					1 => _value1, 
					2 => _value2, 
					3 => _value3, 
					4 => _value4, 
					5 => _value5, 
					6 => _value6, 
					7 => _value7, 
					8 => _value8, 
					9 => _value9, 
					10 => _value10, 
					11 => _value11, 
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
				case 2:
					_value2 = value;
					break;
				case 3:
					_value3 = value;
					break;
				case 4:
					_value4 = value;
					break;
				case 5:
					_value5 = value;
					break;
				case 6:
					_value6 = value;
					break;
				case 7:
					_value7 = value;
					break;
				case 8:
					_value8 = value;
					break;
				case 9:
					_value9 = value;
					break;
				case 10:
					_value10 = value;
					break;
				case 11:
					_value11 = value;
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7},{_value8},{_value9},{_value10},{_value11}}}";
		}

		public IEnumerator<TouchState> GetEnumerator()
		{
			yield return _value0;
			yield return _value1;
			yield return _value2;
			yield return _value3;
			yield return _value4;
			yield return _value5;
			yield return _value6;
			yield return _value7;
			yield return _value8;
			yield return _value9;
			yield return _value10;
			yield return _value11;
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

	public const int TouchCount = 12;

	public long samplingNumber;

	public int count;

	private int _reserved;

	public TouchStateArray12 touches;

	public void SetDefault()
	{
		touches = default(TouchStateArray12);
	}
}
