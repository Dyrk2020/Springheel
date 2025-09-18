using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.hid;

public struct VibrationFileParserContext
{
	public struct IntPtrArray10 : IList<IntPtr>, ICollection<IntPtr>, IEnumerable<IntPtr>, IEnumerable
	{
		private const int _Length = 10;

		private IntPtr _value0;

		private IntPtr _value1;

		private IntPtr _value2;

		private IntPtr _value3;

		private IntPtr _value4;

		private IntPtr _value5;

		private IntPtr _value6;

		private IntPtr _value7;

		private IntPtr _value8;

		private IntPtr _value9;

		public int Length => 10;

		public IntPtr this[int index]
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
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(IntPtr item)
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

		public int IndexOf(IntPtr item)
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

		public void CopyTo(IntPtr[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7},{_value8},{_value9}}}";
		}

		public IEnumerator<IntPtr> GetEnumerator()
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
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IntPtr item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, IntPtr item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(IntPtr item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public IntPtrArray10 _storage;
}
