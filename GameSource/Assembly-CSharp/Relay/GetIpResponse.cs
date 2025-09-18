using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class GetIpResponse : IMessage<GetIpResponse>, IMessage, IEquatable<GetIpResponse>, IDeepCloneable<GetIpResponse>
{
	private static readonly MessageParser<GetIpResponse> _parser = new MessageParser<GetIpResponse>(() => new GetIpResponse());

	private UnknownFieldSet _unknownFields;

	public const int IpFieldNumber = 1;

	private string ip_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetIpResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => HealthReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Ip
	{
		get
		{
			return ip_;
		}
		set
		{
			ip_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public GetIpResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetIpResponse(GetIpResponse other)
		: this()
	{
		ip_ = other.ip_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetIpResponse Clone()
	{
		return new GetIpResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetIpResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetIpResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Ip != other.Ip)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Ip.Length != 0)
		{
			num ^= Ip.GetHashCode();
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
		if (Ip.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Ip);
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
		if (Ip.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Ip);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetIpResponse other)
	{
		if (other != null)
		{
			if (other.Ip.Length != 0)
			{
				Ip = other.Ip;
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
			}
			else
			{
				Ip = input.ReadString();
			}
		}
	}
}
