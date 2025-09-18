using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class RegisterGameRequest : IMessage<RegisterGameRequest>, IMessage, IEquatable<RegisterGameRequest>, IDeepCloneable<RegisterGameRequest>
{
	private static readonly MessageParser<RegisterGameRequest> _parser = new MessageParser<RegisterGameRequest>(() => new RegisterGameRequest());

	private UnknownFieldSet _unknownFields;

	public const int GameIdFieldNumber = 1;

	private string gameId_ = "";

	public const int ServerIdFieldNumber = 2;

	private string serverId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<RegisterGameRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameServerReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string GameId
	{
		get
		{
			return gameId_;
		}
		set
		{
			gameId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RegisterGameRequest()
	{
	}

	[DebuggerNonUserCode]
	public RegisterGameRequest(RegisterGameRequest other)
		: this()
	{
		gameId_ = other.gameId_;
		serverId_ = other.serverId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RegisterGameRequest Clone()
	{
		return new RegisterGameRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RegisterGameRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(RegisterGameRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GameId != other.GameId)
		{
			return false;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (GameId.Length != 0)
		{
			num ^= GameId.GetHashCode();
		}
		if (ServerId.Length != 0)
		{
			num ^= ServerId.GetHashCode();
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
		if (GameId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(GameId);
		}
		if (ServerId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ServerId);
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
		if (GameId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(GameId);
		}
		if (ServerId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RegisterGameRequest other)
	{
		if (other != null)
		{
			if (other.GameId.Length != 0)
			{
				GameId = other.GameId;
			}
			if (other.ServerId.Length != 0)
			{
				ServerId = other.ServerId;
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
				GameId = input.ReadString();
				break;
			case 18u:
				ServerId = input.ReadString();
				break;
			}
		}
	}
}
