using System;

namespace GameSparks;

public interface IGameSparksWebSocket
{
	GameSparksWebSocketState State { get; }

	void Initialize(string url, Action<string> onMessage, Action onClose, Action onOpen, Action<string> onError);

	void Open();

	void Close();

	void Terminate();

	void Send(string request);
}
