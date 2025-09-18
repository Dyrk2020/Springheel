using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.irsensor;

public struct MomentProcessorState
{
	public struct MomentStatisticArray48 : IList<MomentStatistic>, ICollection<MomentStatistic>, IEnumerable<MomentStatistic>, IEnumerable
	{
		private const int _Length = 48;

		private MomentStatistic _value0;

		private MomentStatistic _value1;

		private MomentStatistic _value2;

		private MomentStatistic _value3;

		private MomentStatistic _value4;

		private MomentStatistic _value5;

		private MomentStatistic _value6;

		private MomentStatistic _value7;

		private MomentStatistic _value8;

		private MomentStatistic _value9;

		private MomentStatistic _value10;

		private MomentStatistic _value11;

		private MomentStatistic _value12;

		private MomentStatistic _value13;

		private MomentStatistic _value14;

		private MomentStatistic _value15;

		private MomentStatistic _value16;

		private MomentStatistic _value17;

		private MomentStatistic _value18;

		private MomentStatistic _value19;

		private MomentStatistic _value20;

		private MomentStatistic _value21;

		private MomentStatistic _value22;

		private MomentStatistic _value23;

		private MomentStatistic _value24;

		private MomentStatistic _value25;

		private MomentStatistic _value26;

		private MomentStatistic _value27;

		private MomentStatistic _value28;

		private MomentStatistic _value29;

		private MomentStatistic _value30;

		private MomentStatistic _value31;

		private MomentStatistic _value32;

		private MomentStatistic _value33;

		private MomentStatistic _value34;

		private MomentStatistic _value35;

		private MomentStatistic _value36;

		private MomentStatistic _value37;

		private MomentStatistic _value38;

		private MomentStatistic _value39;

		private MomentStatistic _value40;

		private MomentStatistic _value41;

		private MomentStatistic _value42;

		private MomentStatistic _value43;

		private MomentStatistic _value44;

		private MomentStatistic _value45;

		private MomentStatistic _value46;

		private MomentStatistic _value47;

		public int Length => 48;

		public MomentStatistic this[int index]
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
					24 => _value24, 
					25 => _value25, 
					26 => _value26, 
					27 => _value27, 
					28 => _value28, 
					29 => _value29, 
					30 => _value30, 
					31 => _value31, 
					32 => _value32, 
					33 => _value33, 
					34 => _value34, 
					35 => _value35, 
					36 => _value36, 
					37 => _value37, 
					38 => _value38, 
					39 => _value39, 
					40 => _value40, 
					41 => _value41, 
					42 => _value42, 
					43 => _value43, 
					44 => _value44, 
					45 => _value45, 
					46 => _value46, 
					47 => _value47, 
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
				case 24:
					_value24 = value;
					break;
				case 25:
					_value25 = value;
					break;
				case 26:
					_value26 = value;
					break;
				case 27:
					_value27 = value;
					break;
				case 28:
					_value28 = value;
					break;
				case 29:
					_value29 = value;
					break;
				case 30:
					_value30 = value;
					break;
				case 31:
					_value31 = value;
					break;
				case 32:
					_value32 = value;
					break;
				case 33:
					_value33 = value;
					break;
				case 34:
					_value34 = value;
					break;
				case 35:
					_value35 = value;
					break;
				case 36:
					_value36 = value;
					break;
				case 37:
					_value37 = value;
					break;
				case 38:
					_value38 = value;
					break;
				case 39:
					_value39 = value;
					break;
				case 40:
					_value40 = value;
					break;
				case 41:
					_value41 = value;
					break;
				case 42:
					_value42 = value;
					break;
				case 43:
					_value43 = value;
					break;
				case 44:
					_value44 = value;
					break;
				case 45:
					_value45 = value;
					break;
				case 46:
					_value46 = value;
					break;
				case 47:
					_value47 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(MomentStatistic item)
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

		public int IndexOf(MomentStatistic item)
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

		public void CopyTo(MomentStatistic[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7},{_value8},{_value9},{_value10},{_value11},{_value12},{_value13},{_value14},{_value15},{_value16},{_value17},{_value18},{_value19},{_value20},{_value21},{_value22},{_value23},{_value24},{_value25},{_value26},{_value27},{_value28},{_value29},{_value30},{_value31},{_value32},{_value33},{_value34},{_value35},{_value36},{_value37},{_value38},{_value39},{_value40},{_value41},{_value42},{_value43},{_value44},{_value45},{_value46},{_value47}}}";
		}

		public IEnumerator<MomentStatistic> GetEnumerator()
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
			yield return _value24;
			yield return _value25;
			yield return _value26;
			yield return _value27;
			yield return _value28;
			yield return _value29;
			yield return _value30;
			yield return _value31;
			yield return _value32;
			yield return _value33;
			yield return _value34;
			yield return _value35;
			yield return _value36;
			yield return _value37;
			yield return _value38;
			yield return _value39;
			yield return _value40;
			yield return _value41;
			yield return _value42;
			yield return _value43;
			yield return _value44;
			yield return _value45;
			yield return _value46;
			yield return _value47;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(MomentStatistic item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, MomentStatistic item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(MomentStatistic item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public long samplingNumber;

	public long deltaTimeNanoSeconds;

	public IrCameraAmbientNoiseLevel ambientNoiseLevel;

	private byte _reserved0;

	private byte _reserved1;

	private byte _reserved2;

	private byte _reserved3;

	public MomentStatisticArray48 blocks;
}
