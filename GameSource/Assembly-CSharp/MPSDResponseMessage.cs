using System;
using UnityEngine;

[Serializable]
public class MPSDResponseMessage : RelayMessage
{
	public bool IsSuccess;

	public string Response;

	public MPSDResponseMessage(string id)
		: base(id)
	{
	}

	public new static MPSDResponseMessage ToMessage(string data)
	{
		return JsonUtility.FromJson<MPSDResponseMessage>(data);
	}

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
