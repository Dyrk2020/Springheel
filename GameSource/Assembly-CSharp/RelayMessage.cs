using System;
using UnityEngine;

[Serializable]
public class RelayMessage
{
	public string ID;

	public RelayMessage(string id)
	{
		ID = id;
	}

	public static RelayMessage ToMessage(string data)
	{
		return JsonUtility.FromJson<RelayMessage>(data);
	}

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
