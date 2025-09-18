using System;
using System.Collections;
using System.Collections.Generic;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp;

internal class PayloadData : IEnumerable<byte>, IEnumerable
{
	private byte[] _data;

	private long _extDataLength;

	private long _length;

	public static readonly PayloadData Empty;

	public static readonly ulong MaxLength;

	internal ushort Code
	{
		get
		{
			if (_length < 2)
			{
				return 1005;
			}
			return _data.SubArray(0, 2).ToUInt16(ByteOrder.Big);
		}
	}

	internal long ExtensionDataLength
	{
		get
		{
			return _extDataLength;
		}
		set
		{
			_extDataLength = value;
		}
	}

	internal bool HasReservedCode
	{
		get
		{
			if (_length >= 2)
			{
				return Code.IsReserved();
			}
			return false;
		}
	}

	internal string Reason
	{
		get
		{
			if (_length <= 2)
			{
				return string.Empty;
			}
			if (!_data.SubArray(2L, _length - 2).TryGetUTF8DecodedString(out var s))
			{
				return string.Empty;
			}
			return s;
		}
	}

	public byte[] ApplicationData
	{
		get
		{
			if (_extDataLength <= 0)
			{
				return _data;
			}
			return _data.SubArray(_extDataLength, _length - _extDataLength);
		}
	}

	public byte[] ExtensionData
	{
		get
		{
			if (_extDataLength <= 0)
			{
				return WebSocket.EmptyBytes;
			}
			return _data.SubArray(0L, _extDataLength);
		}
	}

	public ulong Length => (ulong)_length;

	static PayloadData()
	{
		Empty = new PayloadData(WebSocket.EmptyBytes, 0L);
		MaxLength = 9223372036854775807uL;
	}

	internal PayloadData(byte[] data)
		: this(data, data.LongLength)
	{
	}

	internal PayloadData(byte[] data, long length)
	{
		_data = data;
		_length = length;
	}

	internal PayloadData(ushort code, string reason)
	{
		_data = code.Append(reason);
		_length = _data.LongLength;
	}

	internal void Mask(byte[] key)
	{
		for (long num = 0L; num < _length; num++)
		{
			_data[num] ^= key[num % 4];
		}
	}

	public IEnumerator<byte> GetEnumerator()
	{
		byte[] data = _data;
		for (int i = 0; i < data.Length; i++)
		{
			yield return data[i];
		}
	}

	public byte[] ToArray()
	{
		return _data;
	}

	public override string ToString()
	{
		return BitConverter.ToString(_data);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
