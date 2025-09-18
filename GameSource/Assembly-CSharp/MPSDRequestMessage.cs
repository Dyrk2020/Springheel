using System;
using UnityEngine;

[Serializable]
public class MPSDRequestMessage : RelayMessage
{
	public RuntimePlatform RuntimePlatform;

	public string PlayerID;

	public string PlayerName;

	public string XToken;

	public string LobbyCode;

	public string GamesparksLobby;

	public string SecureDeviceAddress;

	public MPSDRequestMessage(string id)
		: base(id)
	{
	}

	public new static MPSDRequestMessage ToMessage(string data)
	{
		return JsonUtility.FromJson<MPSDRequestMessage>(data);
	}

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
