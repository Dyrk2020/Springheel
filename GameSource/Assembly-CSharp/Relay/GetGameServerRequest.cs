using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Relay;

public sealed class GetGameServerRequest : IMessage<GetGameServerRequest>, IMessage, IEquatable<GetGameServerRequest>, IDeepCloneable<GetGameServerRequest>
{
	private static readonly MessageParser<GetGameServerRequest> _parser = new MessageParser<GetGameServerRequest>(() => new GetGameServerRequest());

	private UnknownFieldSet _unknownFields;

	public const int GameIdFieldNumber = 1;

	private string gameId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<GetGameServerRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RelayReflection.Descriptor.MessageTypes[0];

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
	public GetGameServerRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetGameServerRequest(GetGameServerRequest other)
		: this()
	{
		gameId_ = other.gameId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetGameServerRequest Clone()
	{
		return new GetGameServerRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetGameServerRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetGameServerRequest other)
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
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetGameServerRequest other)
	{
		if (other != null)
		{
			if (other.GameId.Length != 0)
			{
				GameId = other.GameId;
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
				GameId = input.ReadString();
			}
		}
	}
}
