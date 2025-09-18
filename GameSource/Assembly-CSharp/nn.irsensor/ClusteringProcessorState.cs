using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace nn.irsensor;

public struct ClusteringProcessorState
{
	public struct ClusteringDataArray16 : IList<ClusteringData>, ICollection<ClusteringData>, IEnumerable<ClusteringData>, IEnumerable
	{
		private const int _Length = 16;

		private ClusteringData _value0;

		private ClusteringData _value1;

		private ClusteringData _value2;

		private ClusteringData _value3;

		private ClusteringData _value4;

		private ClusteringData _value5;

		private ClusteringData _value6;

		private ClusteringData _value7;

		private ClusteringData _value8;

		private ClusteringData _value9;

		private ClusteringData _value10;

		private ClusteringData _value11;

		private ClusteringData _value12;

		private ClusteringData _value13;

		private ClusteringData _value14;

		private ClusteringData _value15;

		public int Length => 16;

		public ClusteringData this[int index]
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
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(ClusteringData item)
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

		public int IndexOf(ClusteringData item)
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

		public void CopyTo(ClusteringData[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7},{_value8},{_value9},{_value10},{_value11},{_value12},{_value13},{_value14},{_value15}}}";
		}

		public IEnumerator<ClusteringData> GetEnumerator()
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
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ClusteringData item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, ClusteringData item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(ClusteringData item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public long samplingNumber;

	public long timeStampNanoSeconds;

	public sbyte objectCount;

	public byte _reserved0;

	public byte _reserved1;

	public byte _reserved2;

	public IrCameraAmbientNoiseLevel ambientNoiseLevel;

	public ClusteringDataArray16 objects;

	public void SetDefault()
	{
		objects = default(ClusteringDataArray16);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("({0} {1} {2} {3})\n", samplingNumber, timeStampNanoSeconds, objectCount, ambientNoiseLevel.ToString());
		for (int i = 0; i < objectCount; i++)
		{
			stringBuilder.AppendFormat("object[{0}]:{1}\n", i, objects[i].ToString());
		}
		return stringBuilder.ToString();
	}
}
