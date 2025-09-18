using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using UnityEngine;
using UnityEngine.Networking;

public class LobbySkillTracker : NetworkBehaviour, IGameEventListener
{
	private class DummyInt : IEqualityComparer<int>
	{
		public int value;

		public DummyInt(int value)
		{
			this.value = value;
		}

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}

		private static void dummy()
		{
		}
	}

	private Rating[] ratings = new Rating[4];

	private void Start()
	{
		changeListeners(adding: true);
	}

	private void OnDestroy()
	{
		changeListeners(adding: false);
	}

	private void changeListeners(bool adding)
	{
		GameEventManager.ChangeListener<LobbyPlayerCreatedEvent>(this, adding);
		GameEventManager.ChangeListener<LobbyPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<GameResultsEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(LobbyPlayerRemovedEvent))
		{
			LobbyPlayerRemovedEvent lobbyPlayerRemovedEvent = e as LobbyPlayerRemovedEvent;
			ratings[lobbyPlayerRemovedEvent.PlayerNumber - 1] = null;
			UpdateLobbyInfo();
		}
		if (type == typeof(LobbyPlayerCreatedEvent))
		{
			LobbyPlayerCreatedEvent lobbyPlayerCreatedEvent = e as LobbyPlayerCreatedEvent;
			if (lobbyPlayerCreatedEvent.LobbyPlayerObj != null)
			{
				LobbyPlayer component = lobbyPlayerCreatedEvent.LobbyPlayerObj.GetComponent<LobbyPlayer>();
				if (component != null)
				{
					StartCoroutine(waitForSkill(component));
				}
			}
		}
		if (type == typeof(GameResultsEvent))
		{
			GameResultsEvent gameResultsEvent = e as GameResultsEvent;
			RecalculateScores(gameResultsEvent.PlayerScores);
		}
	}

	private IEnumerator waitForSkill(LobbyPlayer pl)
	{
		while (pl.SkillMean == 0.0)
		{
			yield return null;
		}
		ratings[pl.networkNumber - 1] = new Rating(pl.SkillMean, pl.SkillStdDev);
		UpdateLobbyInfo();
	}

	public void UpdateLobbyInfo()
	{
		if (!SteamManager.Destroyed && !SteamManager.DestroyedByEditor && SteamManager.Initialized)
		{
			string text = "";
			int num = 0;
			for (int i = 0; i != 4; i++)
			{
				Rating rating = ratings[i];
				if (rating != null)
				{
					text = text + "," + rating.Mean + "," + rating.StandardDeviation;
					num++;
				}
			}
			text = num + text;
			if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.CurrentMatchmakingLobby.IsValid() && Matchmaker.Instance.IsLobbyOwner())
			{
				Matchmaker.Instance.CurrentLobby.SetPlayerSkills(text);
			}
		}
		else
		{
			Debug.LogWarning("Could not update lobby skill info; SteamManager is already destroyed.");
		}
	}

	public void RecalculateScores(IDictionary<GamePlayer, int> gameResults)
	{
		if (gameResults.Count <= 1)
		{
			return;
		}
		try
		{
			GamePlayer[] array = new GamePlayer[gameResults.Count];
			gameResults.Keys.CopyTo(array, 0);
			int[] array2 = new int[array.Length];
			for (int i = 0; i != array2.Length; i++)
			{
				int num = gameResults[array[i]];
				int num2 = 1;
				for (int j = 0; j != array.Length; j++)
				{
					if (j != i && gameResults[array[j]] > num)
					{
						num2++;
					}
				}
				array2[i] = num2;
			}
			new DummyInt(0);
			List<Team<DummyInt>> list = new List<Team<DummyInt>>();
			for (int k = 0; k != 4; k++)
			{
				Rating rating = ratings[k];
				if (rating != null)
				{
					list.Add(new Team<DummyInt>(new DummyInt(k), rating));
				}
			}
			IDictionary<DummyInt, Rating> dictionary = new FactorGraphTrueSkillCalculator().CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(list.ToArray()), array2);
			foreach (DummyInt key in dictionary.Keys)
			{
				Rating rating2 = dictionary[key];
				Debug.Log("Player " + key?.ToString() + " skill updated: [" + ratings[key.value].Mean + ", " + ratings[key.value].StandardDeviation + "] => [" + rating2.Mean + ", " + rating2.StandardDeviation);
				MsgPlayerSkillUpdated msgPlayerSkillUpdated = new MsgPlayerSkillUpdated();
				msgPlayerSkillUpdated.NetworkPlayerNumber = key.value;
				msgPlayerSkillUpdated.SkillMean = rating2.Mean;
				msgPlayerSkillUpdated.SkillStdDev = rating2.StandardDeviation;
				NetworkServer.SendToAll(NetMsgTypes.PlayerSkillUpdated, msgPlayerSkillUpdated);
				ratings[key.value] = rating2;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while updating player skill: " + ex.Message + "\n" + ex.StackTrace);
		}
		UpdateLobbyInfo();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
