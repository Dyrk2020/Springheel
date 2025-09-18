using System;
using System.Collections;
using System.Collections.Generic;

namespace nn.irsensor;

public struct HandAnalysisSilhouetteState
{
	public struct ShapeArray16 : IList<Shape>, ICollection<Shape>, IEnumerable<Shape>, IEnumerable
	{
		private const int _Length = 16;

		private Shape _value0;

		private Shape _value1;

		private Shape _value2;

		private Shape _value3;

		private Shape _value4;

		private Shape _value5;

		private Shape _value6;

		private Shape _value7;

		private Shape _value8;

		private Shape _value9;

		private Shape _value10;

		private Shape _value11;

		private Shape _value12;

		private Shape _value13;

		private Shape _value14;

		private Shape _value15;

		public int Length => 16;

		public Shape this[int index]
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

		public bool Contains(Shape item)
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

		public int IndexOf(Shape item)
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

		public void CopyTo(Shape[] array, int arrayIndex)
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

		public IEnumerator<Shape> GetEnumerator()
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

		public void Add(Shape item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Shape item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Shape item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public struct HandArray2 : IList<Hand>, ICollection<Hand>, IEnumerable<Hand>, IEnumerable
	{
		private const int _Length = 2;

		private Hand _value0;

		private Hand _value1;

		public int Length => 2;

		public Hand this[int index]
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

		public bool Contains(Hand item)
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

		public int IndexOf(Hand item)
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

		public void CopyTo(Hand[] array, int arrayIndex)
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

		public IEnumerator<Hand> GetEnumerator()
		{
			yield return _value0;
			yield return _value1;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Hand item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Hand item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Hand item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public long samplingNumber;

	public IrCameraAmbientNoiseLevel ambientNoiseLevel;

	public int shapeCount;

	public ShapeArray16 shapes;

	public int handCount;

	public HandArray2 hands;
}
