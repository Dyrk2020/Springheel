using System;
using UnityEngine;
using UnityEngine.UI;

public class TreehouseLoaderDebugDisplay : MonoBehaviour
{
	public static TreehouseLoaderDebugDisplay Instance;

	public Text txt;

	private float timer;

	private float lastMessageTimer;

	private bool firstMessage;

	public void Awake()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		timer += Time.deltaTime;
	}

	public void Clear()
	{
		txt.text = "";
	}

	public void AddLine(string message)
	{
		float time = timer - lastMessageTimer;
		lastMessageTimer = timer;
		int num = (int)(GC.GetTotalMemory(forceFullCollection: false) / 1024);
		Text text = txt;
		text.text = text.text + HighscoreDisplayEntry.GetTimeString(timer) + " (+" + HighscoreDisplayEntry.GetTimeString(time) + ") - [" + num + " KB]: " + message + "\n";
	}
}
