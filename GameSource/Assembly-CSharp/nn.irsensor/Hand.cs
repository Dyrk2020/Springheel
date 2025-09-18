using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nn.irsensor;

public struct Hand : IEquatable<Hand>
{
	public struct ProtrusionArray8 : IList<Protrusion>, ICollection<Protrusion>, IEnumerable<Protrusion>, IEnumerable
	{
		private const int _Length = 8;

		private Protrusion _value0;

		private Protrusion _value1;

		private Protrusion _value2;

		private Protrusion _value3;

		private Protrusion _value4;

		private Protrusion _value5;

		private Protrusion _value6;

		private Protrusion _value7;

		public int Length => 8;

		public Protrusion this[int index]
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
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(Protrusion item)
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

		public int IndexOf(Protrusion item)
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

		public void CopyTo(Protrusion[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3},{_value4},{_value5},{_value6},{_value7}}}";
		}

		public IEnumerator<Protrusion> GetEnumerator()
		{
			yield return _value0;
			yield return _value1;
			yield return _value2;
			yield return _value3;
			yield return _value4;
			yield return _value5;
			yield return _value6;
			yield return _value7;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Protrusion item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Protrusion item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Protrusion item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public struct Fingers : IList<Finger>, ICollection<Finger>, IEnumerable<Finger>, IEnumerable
	{
		private const int _Length = 5;

		public Finger thumb;

		public Finger index;

		public Finger middle;

		public Finger ring;

		public Finger little;

		public int Length => 5;

		public Finger this[int index]
		{
			get
			{
				return index switch
				{
					0 => thumb, 
					1 => this.index, 
					2 => middle, 
					3 => ring, 
					4 => little, 
					_ => throw new IndexOutOfRangeException(), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					thumb = value;
					break;
				case 1:
					this.index = value;
					break;
				case 2:
					middle = value;
					break;
				case 3:
					ring = value;
					break;
				case 4:
					little = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(Finger item)
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

		public int IndexOf(Finger item)
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

		public void CopyTo(Finger[] array, int arrayIndex)
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
			return $"{{{thumb},{index},{middle},{ring},{little}}}";
		}

		public IEnumerator<Finger> GetEnumerator()
		{
			yield return thumb;
			yield return index;
			yield return middle;
			yield return ring;
			yield return little;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Finger item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Finger item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Finger item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public int shapeId;

	public int protrusionCount;

	public ProtrusionArray8 protrusions;

	public HandChirality chirality;

	public Fingers fingers;

	[MarshalAs(UnmanagedType.U1)]
	public bool areIndexMiddleFingersTouching;

	[MarshalAs(UnmanagedType.U1)]
	public bool areMiddleRingFingersTouching;

	[MarshalAs(UnmanagedType.U1)]
	public bool areRingLittleFingersTouching;

	public Palm palm;

	public Arm arm;

	public static bool operator ==(Hand lhs, Hand rhs)
	{
		if (lhs.protrusions.Length != rhs.protrusions.Length)
		{
			return false;
		}
		for (int i = 0; i < lhs.protrusions.Length; i++)
		{
			if (lhs.protrusions[i] != rhs.protrusions[i])
			{
				return false;
			}
		}
		if (lhs.fingers.Length != rhs.fingers.Length)
		{
			return false;
		}
		for (int j = 0; j < lhs.protrusions.Length; j++)
		{
			if (lhs.fingers[j] != rhs.fingers[j])
			{
				return false;
			}
		}
		if (lhs.shapeId == rhs.shapeId && lhs.protrusionCount == rhs.protrusionCount && lhs.chirality == rhs.chirality && lhs.areIndexMiddleFingersTouching == rhs.areIndexMiddleFingersTouching && lhs.areMiddleRingFingersTouching == rhs.areMiddleRingFingersTouching && lhs.areRingLittleFingersTouching == rhs.areRingLittleFingersTouching && lhs.palm == rhs.palm)
		{
			return lhs.arm == rhs.arm;
		}
		return false;
	}

	public static bool operator !=(Hand lhs, Hand rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Hand))
		{
			return false;
		}
		return Equals((Hand)right);
	}

	public bool Equals(Hand other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
