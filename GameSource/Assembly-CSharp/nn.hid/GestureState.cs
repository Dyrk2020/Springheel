using System;
using System.Collections;
using System.Collections.Generic;
using nn.util;

namespace nn.hid;

public struct GestureState
{
	public struct GesturePointArray4 : IList<GesturePoint>, ICollection<GesturePoint>, IEnumerable<GesturePoint>, IEnumerable
	{
		private const int _Length = 4;

		private GesturePoint _value0;

		private GesturePoint _value1;

		private GesturePoint _value2;

		private GesturePoint _value3;

		public int Length => 4;

		public GesturePoint this[int index]
		{
			get
			{
				return index switch
				{
					0 => _value0, 
					1 => _value1, 
					2 => _value2, 
					3 => _value3, 
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
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count => Length;

		public bool IsReadOnly => true;

		public bool Contains(GesturePoint item)
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

		public int IndexOf(GesturePoint item)
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

		public void CopyTo(GesturePoint[] array, int arrayIndex)
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
			return $"{{{_value0},{_value1},{_value2},{_value3}}}";
		}

		public IEnumerator<GesturePoint> GetEnumerator()
		{
			yield return _value0;
			yield return _value1;
			yield return _value2;
			yield return _value3;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(GesturePoint item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, GesturePoint item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(GesturePoint item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public long eventNumber;

	public long contextNumber;

	public int _type;

	public int _direction;

	public int x;

	public int y;

	public int deltaX;

	public int deltaY;

	public Float2 velocity;

	public GestureAttribute attributes;

	public float scale;

	public float rotationAngle;

	public int pointCount;

	public GesturePointArray4 points;

	public GestureType type => (GestureType)_type;

	public GestureDirection direction => (GestureDirection)_direction;

	public bool isDoubleTap => (attributes & GestureAttribute.IsDoubleTap) == GestureAttribute.IsDoubleTap;

	public void SetDefault()
	{
		points = default(GesturePointArray4);
	}

	public override string ToString()
	{
		return $"event:{eventNumber} con:{contextNumber} type:{type} dir:{direction} pos:({x} {y}) delta:({deltaX} {deltaY}) vel:{velocity} attr:{attributes} scale:{scale} rotA:{rotationAngle} count:{pointCount} p0:{points[0]} p1:{points[1]} p2:{points[2]} p3:{points[3]}";
	}
}
