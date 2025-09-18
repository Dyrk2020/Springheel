using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class ControlTipData : IGameEventListener
{
	public enum KnowledgeType
	{
		SPRINT,
		ROTATE
	}

	[Serializable]
	public class playerKnowledge
	{
		public string playerName;

		public int usedSprint;

		public int usedRotate;

		public void clear()
		{
			usedSprint = 0;
			usedRotate = 0;
		}
	}

	public Dictionary<int, playerKnowledge> playerKnowledgeDictionary = new Dictionary<int, playerKnowledge>();

	public int runRounds;

	public int roundsTillNextRunRound;

	public int buildRounds;

	public int roundsTillNextBuildRound;

	private GameControl.GamePhase lastPhase;

	public ControlTipData()
	{
		ChangeListener(adding: true);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LobbyPlayerCreatedEvent>(this, adding);
		GameEventManager.ChangeListener<LobbyPlayerRemovedEvent>(this, adding);
	}

	public void ReceiveInput(int networkNumber, KnowledgeType knowledgeType)
	{
		if (playerKnowledgeDictionary.TryGetValue(networkNumber, out var value) && value != null)
		{
			if (knowledgeType == KnowledgeType.SPRINT)
			{
				value.usedSprint++;
			}
			if (knowledgeType == KnowledgeType.ROTATE)
			{
				value.usedRotate++;
			}
		}
	}

	public bool askForHint(KnowledgeType type, bool debugMessageOn = false)
	{
		bool flag = false;
		if (type == KnowledgeType.SPRINT)
		{
			foreach (playerKnowledge value in playerKnowledgeDictionary.Values)
			{
				if (value.usedSprint >= GameSettings.GetInstance().minSprints)
				{
					continue;
				}
				if (runRounds > GameSettings.GetInstance().RoundsTillSprintTip)
				{
					flag = true;
					if (debugMessageOn)
					{
						Debug.Log(value.playerName + " doesn't know Sprint");
					}
				}
				else if (debugMessageOn)
				{
					Debug.Log(value.playerName + " doesn't know Sprint - but still not time");
				}
			}
		}
		if (type == KnowledgeType.ROTATE)
		{
			foreach (playerKnowledge value2 in playerKnowledgeDictionary.Values)
			{
				if (value2.usedRotate >= GameSettings.GetInstance().minRotates)
				{
					continue;
				}
				if (buildRounds > GameSettings.GetInstance().RoundsTillRotateTip)
				{
					flag = true;
					if (debugMessageOn)
					{
						Debug.Log(value2.playerName + " doesn't know Sprint");
					}
				}
				else if (debugMessageOn)
				{
					Debug.Log(value2.playerName + " doesn't know Sprint - but still not time");
				}
			}
		}
		if (!flag && debugMessageOn)
		{
			Debug.Log("noHint");
		}
		return flag;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (lastPhase != startPhaseEvent.Phase)
			{
				if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
				{
					buildRounds++;
				}
				if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY)
				{
					runRounds++;
				}
			}
			else if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE && startPhaseEvent.Phase == GameControl.GamePhase.PLAY)
			{
				runRounds++;
			}
			lastPhase = startPhaseEvent.Phase;
		}
		if (type == typeof(LobbyPlayerCreatedEvent))
		{
			LobbyPlayer component = (e as LobbyPlayerCreatedEvent).LobbyPlayerObj.GetComponent<LobbyPlayer>();
			if (component != null && component.IsLocalPlayer)
			{
				playerKnowledgeDictionary.Add(component.networkNumber, new playerKnowledge());
				playerKnowledgeDictionary[component.networkNumber].playerName = component.playerName;
				buildRounds = 0;
				runRounds = 0;
			}
		}
		if (type == typeof(LobbyPlayerRemovedEvent))
		{
			LobbyPlayerRemovedEvent lobbyPlayerRemovedEvent = e as LobbyPlayerRemovedEvent;
			playerKnowledgeDictionary.Remove(lobbyPlayerRemovedEvent.PlayerNumber);
		}
	}
}
