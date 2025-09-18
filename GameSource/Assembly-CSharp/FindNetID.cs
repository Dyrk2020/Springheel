using UnityEngine;
using UnityEngine.Networking;

public class FindNetID : MonoBehaviour, InputReceiver
{
	public uint NetID;

	public GameObject Result;

	public bool Search;

	public string[] objs = new string[100];

	private int buttonTrigger;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Search = true;
		Controller.AddGlobalReceiver(this);
	}

	private void Update()
	{
		if (Search)
		{
			Result = ClientScene.FindLocalObject(new NetworkInstanceId(NetID));
			Search = false;
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (!e.Changed || !e.Valueb || e.Key != InputEvent.InputKey.Inventory)
		{
			return;
		}
		buttonTrigger++;
		if (buttonTrigger == 5)
		{
			for (int i = 0; i != 100; i++)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId((uint)i));
				if (gameObject != null)
				{
					objs[i] = gameObject.ToString();
				}
			}
			for (int j = 0; j != objs.Length; j++)
			{
				if (!objs[j].NullOrEmpty())
				{
					Debug.Log("Net ID: " + j + " - " + objs[j]);
				}
			}
		}
		buttonTrigger = 0;
	}
}
