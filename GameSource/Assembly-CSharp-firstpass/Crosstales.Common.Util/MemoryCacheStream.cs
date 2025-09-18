using System;
using System.IO;
using UnityEngine;

namespace Crosstales.Common.Util;

public class MemoryCacheStream : Stream
{
	private byte[] _cache;

	private long _writePosition;

	private long _readPosition;

	private long _length;

	private int _size;

	private readonly int _maxSize;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => true;

	public override long Position
	{
		get
		{
			return _readPosition;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
			}
			_readPosition = value;
		}
	}

	public override long Length => _length;

	public MemoryCacheStream(int cacheSize = 65536, int maxCacheSize = 67108864)
	{
		_length = (_writePosition = (_readPosition = 0L));
		_size = cacheSize;
		_maxSize = maxCacheSize;
		createCache();
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = (int)offset;
			break;
		case SeekOrigin.Current:
		{
			long num2 = Position + offset;
			if (num2 < 0)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			Position = num2;
			break;
		}
		case SeekOrigin.End:
		{
			long num = _length + offset;
			if (num < 0)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			Position = num;
			break;
		}
		default:
			throw new ArgumentException("Invalid seek origin.");
		}
		return Position;
	}

	public override void SetLength(long value)
	{
		int num = (int)value;
		if (_size != num)
		{
			_size = num;
			long length = (Position = 0L);
			_length = length;
			createCache();
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_readPosition < _length)
		{
			return read(buffer, offset, count);
		}
		return 0;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (count > _size)
		{
			throw new ArgumentOutOfRangeException("count", "Value is larger than the cache size.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (count != 0)
		{
			write(buffer, offset, count);
		}
	}

	private int read(byte[] buff, int offset, int count)
	{
		int num = (int)(_readPosition % _size);
		if (num + count > _size)
		{
			int num2 = _size - num;
			int length = count - num2;
			Array.Copy(_cache, num, buff, offset, num2);
			Array.Copy(_cache, 0, buff, offset + num2, length);
		}
		else
		{
			Array.Copy(_cache, num, buff, offset, count);
		}
		_readPosition += count;
		return count;
	}

	private void write(byte[] buff, int offset, int count)
	{
		int num = (int)(_writePosition % _size);
		if (num + count > _size)
		{
			int num2 = _size - num;
			int length = count - num2;
			Array.Copy(buff, offset, _cache, num, num2);
			Array.Copy(buff, offset + num2, _cache, 0, length);
		}
		else
		{
			Array.Copy(buff, offset, _cache, num, count);
		}
		_writePosition += count;
		_length = _writePosition;
	}

	private void createCache()
	{
		if (_size > _maxSize)
		{
			_cache = new byte[_maxSize];
			Debug.LogWarning("'size' is larger than 'maxSize'! Using 'maxSize' as cache!");
		}
		else
		{
			_cache = new byte[_size];
		}
	}
}
