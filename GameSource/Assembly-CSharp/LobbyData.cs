using System;
using System.Text;
using UnityEngine;

[Serializable]
public class LobbyData
{
	[SerializeField]
	public string lobbyId;

	public string Serialize()
	{
		string s = JsonUtility.ToJson(this);
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
	}

	public static LobbyData Deserialize(string base64)
	{
		byte[] bytes = Convert.FromBase64String(base64);
		return JsonUtility.FromJson<LobbyData>(Encoding.UTF8.GetString(bytes));
	}
}
