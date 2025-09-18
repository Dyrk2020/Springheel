namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net;

internal enum InputChunkState
{
	None,
	Data,
	DataEnded,
	Trailer,
	End
}
