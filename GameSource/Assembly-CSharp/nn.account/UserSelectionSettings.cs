using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace nn.account;

public struct UserSelectionSettings : IEquatable<UserSelectionSettings>
{
	public struct UidArray8 : IList<Uid>, ICollection<Uid>, IEnumerable<Uid>, IEnumerable
	{
		private const int _Length = 8;

		private Uid _value0;

		private Uid _value1;

		private Uid _value2;

		private Uid _value3;

		private Uid _value4;

		private Uid _value5;

		private Uid _value6;

		private Uid _value7;

		public int Length => 8;

		public Uid this[int index]
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

		public bool Contains(Uid item)
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

		public int IndexOf(Uid item)
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

		public void CopyTo(Uid[] array, int arrayIndex)
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

		public IEnumerator<Uid> GetEnumerator()
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

		public void Add(Uid item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, Uid item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Uid item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public UidArray8 invalidUidList;

	[MarshalAs(UnmanagedType.U1)]
	public bool isSkipEnabled;

	[MarshalAs(UnmanagedType.U1)]
	public bool isNetworkServiceAccountRequired;

	[MarshalAs(UnmanagedType.U1)]
	public bool showSkipButton;

	[MarshalAs(UnmanagedType.U1)]
	public bool additionalSelect;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUnqualifiedUserSelectable;

	public void SetDefault()
	{
		invalidUidList = default(UidArray8);
		isSkipEnabled = false;
		isNetworkServiceAccountRequired = false;
		showSkipButton = false;
		additionalSelect = false;
		isUnqualifiedUserSelectable = false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("skip:{0} netAccount:{1} skipButton:{2} addSelect:{3} unqualifiedUserSelectable:{4}  ignore:[ ", isSkipEnabled, isNetworkServiceAccountRequired, showSkipButton, additionalSelect, isUnqualifiedUserSelectable);
		for (int i = 0; i < 8; i++)
		{
			if (invalidUidList[i] != Uid.Invalid)
			{
				stringBuilder.Append(invalidUidList[i].ToString());
				stringBuilder.Append(" ");
			}
			stringBuilder.Append("]");
		}
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UserSelectionSettings))
		{
			return false;
		}
		return Equals((UserSelectionSettings)obj);
	}

	public bool Equals(UserSelectionSettings other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(UserSelectionSettings lhs, UserSelectionSettings rhs)
	{
		if (lhs.invalidUidList.Length != rhs.invalidUidList.Length)
		{
			return false;
		}
		for (int i = 0; i < lhs.invalidUidList.Length; i++)
		{
			if (lhs.invalidUidList[i] != rhs.invalidUidList[i])
			{
				return false;
			}
		}
		if (lhs.isSkipEnabled == rhs.isSkipEnabled && lhs.isNetworkServiceAccountRequired == rhs.isNetworkServiceAccountRequired && lhs.showSkipButton == rhs.showSkipButton && lhs.additionalSelect == rhs.additionalSelect)
		{
			return lhs.isUnqualifiedUserSelectable == rhs.isUnqualifiedUserSelectable;
		}
		return false;
	}

	public static bool operator !=(UserSelectionSettings lhs, UserSelectionSettings rhs)
	{
		return !(lhs == rhs);
	}
}
