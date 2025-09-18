using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class GetGameServerResponse : IMessage<GetGameServerResponse>, IMessage, IEquatable<GetGameServerResponse>, IDeepCloneable<GetGameServerResponse>
{
	private static readonly MessageParser<GetGameServerResponse> _parser = new MessageParser<GetGameServerResponse>(() => new GetGameServerResponse());

	private UnknownFieldSet _unknownFields;

	public const int ServerIpFieldNumber = 1;

	private string serverIp_ = "";

	public const int ServerPortFieldNumber = 2;

	private int serverPort_;

	[DebuggerNonUserCode]
	public static MessageParser<GetGameServerResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RelayReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ServerIp
	{
		get
		{
			return serverIp_;
		}
		set
		{
			serverIp_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int ServerPort
	{
		get
		{
			return serverPort_;
		}
		set
		{
			serverPort_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetGameServerResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetGameServerResponse(GetGameServerResponse other)
		: this()
	{
		serverIp_ = other.serverIp_;
		serverPort_ = other.serverPort_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetGameServerResponse Clone()
	{
		return new GetGameServerResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetGameServerResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetGameServerResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServerIp != other.ServerIp)
		{
			return false;
		}
		if (ServerPort != other.ServerPort)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ServerIp.Length != 0)
		{
			num ^= ServerIp.GetHashCode();
		}
		if (ServerPort != 0)
		{
			num ^= ServerPort.GetHashCode();
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
		if (ServerIp.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ServerIp);
		}
		if (ServerPort != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(ServerPort);
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
		if (ServerIp.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerIp);
		}
		if (ServerPort != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerPort);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetGameServerResponse other)
	{
		if (other != null)
		{
			if (other.ServerIp.Length != 0)
			{
				ServerIp = other.ServerIp;
			}
			if (other.ServerPort != 0)
			{
				ServerPort = other.ServerPort;
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
			case 10u:
				ServerIp = input.ReadString();
				break;
			case 16u:
				ServerPort = input.ReadInt32();
				break;
			}
		}
	}
}
