using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class ServiceError : IMessage<ServiceError>, IMessage, IEquatable<ServiceError>, IDeepCloneable<ServiceError>
{
	private static readonly MessageParser<ServiceError> _parser = new MessageParser<ServiceError>(() => new ServiceError());

	private UnknownFieldSet _unknownFields;

	public const int ErrorCodeFieldNumber = 1;

	private int errorCode_;

	public const int PathFieldNumber = 2;

	private string path_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ServiceError> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ServiceErrorReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int ErrorCode
	{
		get
		{
			return errorCode_;
		}
		set
		{
			errorCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Path
	{
		get
		{
			return path_;
		}
		set
		{
			path_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ServiceError()
	{
	}

	[DebuggerNonUserCode]
	public ServiceError(ServiceError other)
		: this()
	{
		errorCode_ = other.errorCode_;
		path_ = other.path_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ServiceError Clone()
	{
		return new ServiceError(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ServiceError);
	}

	[DebuggerNonUserCode]
	public bool Equals(ServiceError other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ErrorCode != other.ErrorCode)
		{
			return false;
		}
		if (Path != other.Path)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ErrorCode != 0)
		{
			num ^= ErrorCode.GetHashCode();
		}
		if (Path.Length != 0)
		{
			num ^= Path.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		if (ErrorCode != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(ErrorCode);
		}
		if (Path.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Path);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (ErrorCode != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ErrorCode);
		}
		if (Path.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Path);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ServiceError other)
	{
		if (other != null)
		{
			if (other.ErrorCode != 0)
			{
				ErrorCode = other.ErrorCode;
			}
			if (other.Path.Length != 0)
			{
				Path = other.Path;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
				break;
			case 8u:
				ErrorCode = input.ReadInt32();
				break;
			case 18u:
				Path = input.ReadString();
				break;
			}
		}
	}
}
