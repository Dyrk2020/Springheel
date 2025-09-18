using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class RemoveGameFromServerRequest : IMessage<RemoveGameFromServerRequest>, IMessage, IEquatable<RemoveGameFromServerRequest>, IDeepCloneable<RemoveGameFromServerRequest>
{
	private static readonly MessageParser<RemoveGameFromServerRequest> _parser = new MessageParser<RemoveGameFromServerRequest>(() => new RemoveGameFromServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int GameIdFieldNumber = 1;

	private string gameId_ = "";

	public const int ServerIdFieldNumber = 2;

	private string serverId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<RemoveGameFromServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameServerReflection.Descriptor.MessageTypes[0];

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
	public RemoveGameFromServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public RemoveGameFromServerRequest(RemoveGameFromServerRequest other)
		: this()
	{
		gameId_ = other.gameId_;
		serverId_ = other.serverId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RemoveGameFromServerRequest Clone()
	{
		return new RemoveGameFromServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RemoveGameFromServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(RemoveGameFromServerRequest other)
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
	public void MergeFrom(RemoveGameFromServerRequest other)
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
