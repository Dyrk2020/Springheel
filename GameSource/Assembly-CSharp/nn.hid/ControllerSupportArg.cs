using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using nn.util;

namespace nn.hid;

public struct ControllerSupportArg
{
	public struct Color4u8Array8 : IList<Color4u8>, ICollection<Color4u8>, IEnumerable<Color4u8>, IEnumerable
	{
		private const int _Length = 8;

		private Color4u8 _value0;

		private Color4u8 _value1;

		private Color4u8 _value2;

		private Color4u8 _value3;

		private Color4u8 _value4;

		private Color4u8 _value5;

		private Color4u8 _value6;

		private Color4u8 _value7;

		public int Length => 8;

		public Color4u8 this[int index]
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

		public bool Contains(Color4u8 item)
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

		public int IndexOf(Color4u8 item)
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

		public void CopyTo(Color4u8[] array, int arrayIndex)
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

		public IEnumerator<Color4u8> GetEnumerator()
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

		public void Add(Color4u8 item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Color4u8 item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Color4u8 item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private const int ExplainTextSize = 1032;

	public byte playerCountMin;

	public byte playerCountMax;

	[MarshalAs(UnmanagedType.U1)]
	public bool enableTakeOverConnection;

	[MarshalAs(UnmanagedType.U1)]
	public bool enableLeftJustify;

	[MarshalAs(UnmanagedType.U1)]
	public bool enablePermitJoyDual;

	[MarshalAs(UnmanagedType.U1)]
	public bool enableSingleMode;

	[MarshalAs(UnmanagedType.U1)]
	public bool enableIdentificationColor;

	public Color4u8Array8 identificationColor;

	[MarshalAs(UnmanagedType.I1)]
	public bool enableExplainText;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1032)]
	private byte[] explainText;

	public void SetDefault()
	{
		playerCountMin = 0;
		playerCountMax = 4;
		enableTakeOverConnection = true;
		enableLeftJustify = true;
		enablePermitJoyDual = true;
		enableSingleMode = false;
		enableIdentificationColor = false;
		identificationColor = default(Color4u8Array8);
		enableExplainText = false;
		explainText = new byte[1032];
	}

	public override string ToString()
	{
		return $"Min{playerCountMin} Max{playerCountMax} TOC{enableTakeOverConnection} LJ{enableLeftJustify} PJD{enablePermitJoyDual} SM{enableSingleMode} IC{enableIdentificationColor} C0{identificationColor[0]} C1{identificationColor[1]} C2{identificationColor[2]} C3{identificationColor[3]} C4{identificationColor[4]} C5{identificationColor[5]} C6{identificationColor[6]} C7{identificationColor[7]} ET{enableExplainText}";
	}
}
