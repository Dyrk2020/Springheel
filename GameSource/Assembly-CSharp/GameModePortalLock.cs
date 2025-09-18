using System.Collections;
using System.Linq;
using GameEvent;
using UnityEngine;

public class GameModePortalLock : MonoBehaviour, IGameEventListener
{
	public LevelPortal levelPortal;

	public GameState.GameMode[] allowInModes;

	private void Awake()
	{
		ChangeListener(adding: true);
	}

	private void Start()
	{
		LockOrUnlock(GameSettings.GetInstance().GameMode);
	}

	private void OnEnable()
	{
		LockOrUnlock(GameSettings.GetInstance().GameMode);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = (MsgSwitchToMode)networkMessageReceivedEvent.ReadMessage;
				LockOrUnlock(msgSwitchToMode.toMode);
			}
		}
	}

	public void LockOrUnlock(GameState.GameMode toMode)
	{
		bool flag = allowInModes.Contains(toMode);
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (flag)
		{
			if (levelPortal.Locked)
			{
				StartCoroutine(setLockInaFrame(value: false));
			}
		}
		else if (!levelPortal.Locked)
		{
			StartCoroutine(setLockInaFrame(value: true));
		}
	}

	private IEnumerator setLockInaFrame(bool value)
	{
		yield return null;
		levelPortal.Locked = value;
	}
}
