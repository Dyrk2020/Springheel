using System;
using BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.WebSockets;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Server;

public interface IWebSocketSession
{
	WebSocketState ConnectionState { get; }

	WebSocketContext Context { get; }

	string ID { get; }

	string Protocol { get; }

	DateTime StartTime { get; }
}
