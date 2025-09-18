using System.Collections;
using GameEvent;
using UnityEngine;

public class ModeDependantColor : MonoBehaviour, IGameEventListener
{
	public Color PartyModeColor;

	public Color CreativeModeColor;

	public Color ChallengeModeColor;

	public Color FreeplayModeColor;

	protected SpriteRenderer[] SRs;

	public float changeSpeedMultiplier = 1f;

	protected GameState.GameMode previousGameMode;

	protected GameState.GameMode targetGameMode;

	protected Color GetColor(GameState.GameMode gameMode)
	{
		return gameMode switch
		{
			GameState.GameMode.FREEPLAY => FreeplayModeColor, 
			GameState.GameMode.CREATIVE => CreativeModeColor, 
			GameState.GameMode.PARTY => PartyModeColor, 
			GameState.GameMode.CHALLENGE => ChallengeModeColor, 
			_ => Color.cyan, 
		};
	}

	private void Start()
	{
		SRs = GetComponents<SpriteRenderer>();
		ChangeListener(adding: true);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public void ChangeModes(GameState.GameMode toMode)
	{
		StopCoroutine("ChangeColours");
		StartCoroutine("ChangeColours", toMode);
	}

	public IEnumerator ChangeColours(GameState.GameMode toMode)
	{
		targetGameMode = toMode;
		float t = 0f;
		while (t < 1f)
		{
			t = Mathf.MoveTowards(t, 1f, Time.deltaTime * changeSpeedMultiplier);
			SetColor(t);
			yield return null;
		}
		previousGameMode = toMode;
	}

	protected void SetColor(float t)
	{
		Color color = Color.Lerp(GetColor(previousGameMode), GetColor(targetGameMode), t);
		SpriteRenderer[] sRs = SRs;
		for (int i = 0; i < sRs.Length; i++)
		{
			sRs[i].color = color;
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = networkMessageReceivedEvent.ReadMessage as MsgSwitchToMode;
				Debug.Log("Received message to switch to mode " + msgSwitchToMode.toMode);
				ChangeModes(msgSwitchToMode.toMode);
			}
		}
	}
}
