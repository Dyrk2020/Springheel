using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.swkbd;

public struct CustomizedDictionarySet
{
	public struct DictionaryInfoArray24 : IList<DictionaryInfo>, ICollection<DictionaryInfo>, IEnumerable<DictionaryInfo>, IEnumerable
	{
		private const int _Length = 24;

		private DictionaryInfo _value0;

		private DictionaryInfo _value1;

		private DictionaryInfo _value2;

		private DictionaryInfo _value3;

		private DictionaryInfo _value4;

		private DictionaryInfo _value5;

		private DictionaryInfo _value6;

		private DictionaryInfo _value7;

		private DictionaryInfo _value8;

		private DictionaryInfo _value9;

		private DictionaryInfo _value10;

		private DictionaryInfo _value11;

		private DictionaryInfo _value12;

		private DictionaryInfo _value13;

		private DictionaryInfo _value14;

		private DictionaryInfo _value15;

		private DictionaryInfo _value16;

		private DictionaryInfo _value17;

		private DictionaryInfo _value18;

		private DictionaryInfo _value19;

		private DictionaryInfo _value20;

		private DictionaryInfo _value21;

		private DictionaryInfo _value22;

		private DictionaryInfo _value23;

		public int Length => 24;

		public DictionaryInfo this[int index]
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
					12 => _value12, 
					13 => _value13, 
					14 => _value14, 
					15 => _value15, 
					16 => _value16, 
					17 => _value17, 
					18 => _value18, 
					19 => _value19, 
					20 => _value20, 
					21 => _value21, 
					22 => _value22, 
					23 => _value23, 
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
				case 12:
					_value12 = value;
					break;
				case 13:
					_value13 = value;
					break;
				case 14:
					_value14 = value;
					break;
				case 15:
					_value15 = value;
					break;
				case 16:
					_value16 = value;
					break;
				case 17:
					_value17 = value;
					break;
				case 18:
					_value18 = value;
					break;
				case 19:
					_value19 = value;
					break;
				case 20:
					_value20 = value;
					break;
				case 21:
					_value21 = value;
					break;
				case 22:
					_value22 = value;
					break;
				case 23:
					_value23 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(DictionaryInfo item)
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

		public int IndexOf(DictionaryInfo item)
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

		public void CopyTo(DictionaryInfo[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7},{_value8},{_value9},{_value10},{_value11},{_value12},{_value13},{_value14},{_value15},{_value16},{_value17},{_value18},{_value19},{_value20},{_value21},{_value22},{_value23}}}";
		}

		public IEnumerator<DictionaryInfo> GetEnumerator()
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
			yield return _value12;
			yield return _value13;
			yield return _value14;
			yield return _value15;
			yield return _value16;
			yield return _value17;
			yield return _value18;
			yield return _value19;
			yield return _value20;
			yield return _value21;
			yield return _value22;
			yield return _value23;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(DictionaryInfo item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, DictionaryInfo item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(DictionaryInfo item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public IntPtr pDictionaries;

	public uint dictionariesSize;

	public DictionaryInfoArray24 dicInfoList;

	public ushort count;
}
