using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreKeeper : IGameEventListener
{
	private struct scoreInfo
	{
		public int totalScore;

		public int loseStreak;

		public int winStreak;

		public bool disconnected;
	}

	private static ScoreKeeper instance;

	public List<PointBlock> newPointBlocks = new List<PointBlock>();

	private List<PointBlock> historyPointBlocks = new List<PointBlock>();

	private Dictionary<GamePlayer, scoreInfo> playerTotal = new Dictionary<GamePlayer, scoreInfo>();

	public bool IsOnServer;

	public static ScoreKeeper Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ScoreKeeper();
			}
			return instance;
		}
	}

	private ScoreKeeper()
	{
		ChangeListeners(adding: true);
	}

	~ScoreKeeper()
	{
		ChangeListeners(adding: false);
	}

	public void Setup()
	{
		newPointBlocks.Clear();
		historyPointBlocks.Clear();
		playerTotal.Clear();
		foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
		{
			if (allGameNetID == 0)
			{
				continue;
			}
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (!(gameObject == null))
			{
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				if (!playerTotal.ContainsKey(component))
				{
					playerTotal.Add(component, default(scoreInfo));
				}
			}
		}
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public bool AwardPoint(PointBlock pointBlock, bool addImmediate = false)
	{
		if (GameSettings.GetInstance().PointTypeEnabled(pointBlock.type))
		{
			if (addImmediate)
			{
				addPointBlock(pointBlock);
			}
			else
			{
				MsgPointAwarded msgPointAwarded = new MsgPointAwarded();
				msgPointAwarded.PlayerNumber = pointBlock.playerNumber;
				msgPointAwarded.PointType = pointBlock.type;
				msgPointAwarded.AlwaysAward = pointBlock.AlwaysAward;
				NetworkManager.singleton.client.Send(NetMsgTypes.PointAwarded, msgPointAwarded);
			}
			return true;
		}
		return false;
	}

	public void TallyPointBlockAllPlayers(bool checkStreaks)
	{
		if (newPointBlocks.Count == 0)
		{
			return;
		}
		foreach (GamePlayer item in new List<GamePlayer>(playerTotal.Keys))
		{
			if (item == null || !playerTotal.ContainsKey(item))
			{
				continue;
			}
			Debug.Log(item);
			int num = 0;
			scoreInfo value = playerTotal[item];
			if (value.disconnected)
			{
				continue;
			}
			bool flag = false;
			foreach (PointBlock newPointBlock in newPointBlocks)
			{
				if (newPointBlock.playerNumber == item.networkNumber)
				{
					num += newPointBlock.pointValue;
					if (newPointBlock.type == PointBlock.pointBlockType.win || newPointBlock.type == PointBlock.pointBlockType.winDead)
					{
						flag = true;
					}
				}
			}
			if (num > 0)
			{
				value.totalScore += (int)((float)num * ((float)item.Handicap / 100f));
			}
			if (checkStreaks)
			{
				if (flag)
				{
					value.loseStreak = 0;
					value.winStreak++;
				}
				else
				{
					value.winStreak = 0;
					value.loseStreak++;
					Debug.Log("Player " + item.networkNumber + " lose streak: " + value.loseStreak);
				}
			}
			playerTotal[item] = value;
		}
		historyPointBlocks.AddRange(newPointBlocks);
		newPointBlocks.Clear();
	}

	public void ClearNewPointBlocks(bool clearAll = false)
	{
		List<PointBlock> list = new List<PointBlock>();
		foreach (PointBlock newPointBlock in newPointBlocks)
		{
			if (newPointBlock.AlwaysAward)
			{
				list.Add(newPointBlock);
			}
		}
		newPointBlocks.Clear();
		newPointBlocks.AddRange(list);
	}

	public void RemoveSpecialPoints()
	{
		List<PointBlock> list = new List<PointBlock>();
		foreach (PointBlock newPointBlock in newPointBlocks)
		{
			if (GameSettings.GetInstance().PointTypeEnabled(newPointBlock.type))
			{
				list.Add(newPointBlock);
			}
		}
		newPointBlocks.Clear();
		newPointBlocks.AddRange(list);
	}

	public bool AreThereCoinPoints()
	{
		foreach (PointBlock newPointBlock in newPointBlocks)
		{
			if (newPointBlock.type == PointBlock.pointBlockType.coin)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreThereNonCoinPoints()
	{
		foreach (PointBlock newPointBlock in newPointBlocks)
		{
			if (newPointBlock.type != PointBlock.pointBlockType.coin)
			{
				return true;
			}
		}
		return false;
	}

	public void MarkPlayerDisconnected(GamePlayer player)
	{
		if (playerTotal.ContainsKey(player))
		{
			scoreInfo value = playerTotal[player];
			value.disconnected = true;
			playerTotal[player] = value;
		}
	}

	public int GetPlayerTotal(GamePlayer player)
	{
		if (playerTotal.ContainsKey(player))
		{
			return playerTotal[player].totalScore;
		}
		return 0;
	}

	public void AddPointsDirectly(GamePlayer player, int points)
	{
		if (playerTotal.ContainsKey(player))
		{
			scoreInfo value = playerTotal[player];
			value.totalScore += points;
			playerTotal[player] = value;
		}
	}

	public int GetPlayerLoseStreak(GamePlayer player)
	{
		if (playerTotal.ContainsKey(player))
		{
			return playerTotal[player].loseStreak;
		}
		return 0;
	}

	public int GetPlayerWinStreak(GamePlayer player)
	{
		if (playerTotal.ContainsKey(player))
		{
			return playerTotal[player].winStreak;
		}
		return 0;
	}

	public bool IsPlayerInLoseStreak(int playerNumber)
	{
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		foreach (GamePlayer key in playerTotal.Keys)
		{
			if (key == null || !playerTotal.ContainsKey(key) || key.CharacterInstance == null)
			{
				continue;
			}
			if (playerTotal[key].disconnected)
			{
				return false;
			}
			int totalScore = playerTotal[key].totalScore;
			if (totalScore <= num)
			{
				if (totalScore != num)
				{
					num2 = num;
				}
				num = totalScore;
			}
			else if (totalScore < num2)
			{
				num2 = totalScore;
			}
		}
		if (num2 - num <= GameSettings.GetInstance().PointTypeValue(PointBlock.pointBlockType.win))
		{
			return false;
		}
		foreach (GamePlayer key2 in playerTotal.Keys)
		{
			if (!(key2 == null) && playerTotal.ContainsKey(key2))
			{
				int totalScore2 = playerTotal[key2].totalScore;
				if (key2.networkNumber == playerNumber && totalScore2 == num && playerTotal[key2].loseStreak >= GameSettings.GetInstance().ComebackStreak)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetPlayerLoseStreak(int playerNumber)
	{
		foreach (GamePlayer key in playerTotal.Keys)
		{
			if (!(key == null) && playerTotal.ContainsKey(key) && key.networkNumber == playerNumber)
			{
				return playerTotal[key].loseStreak;
			}
		}
		return 0;
	}

	public int GetPlayerWinStreak(int playerNumber)
	{
		foreach (GamePlayer key in playerTotal.Keys)
		{
			if (!(key == null) && playerTotal.ContainsKey(key) && key.networkNumber == playerNumber)
			{
				return playerTotal[key].winStreak;
			}
		}
		return 0;
	}

	public void ClearScore()
	{
		instance = new ScoreKeeper();
		newPointBlocks.Clear();
		historyPointBlocks.Clear();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PointAwarded)
			{
				MsgPointAwarded msgPointAwarded = networkMessageReceivedEvent.ReadMessage as MsgPointAwarded;
				PointBlock pointBlock = new PointBlock(msgPointAwarded.PointType, msgPointAwarded.PlayerNumber);
				addPointBlock(pointBlock);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ReadyToTallyPoints)
			{
				TallyPointBlockAllPlayers(checkStreaks: false);
			}
		}
	}

	private void OnPointEarnedForPlayer(Player p, SaveFileData saveFileData, string statName, PointBlock.pointBlockType pointType)
	{
		saveFileData?.IncrementStat(statName);
	}

	private void addPointBlock(PointBlock pointBlock)
	{
		if (pointBlock.playerNumber <= 0)
		{
			return;
		}
		foreach (GamePlayer key in playerTotal.Keys)
		{
			if (key == null || !playerTotal.ContainsKey(key) || key.networkNumber != pointBlock.playerNumber)
			{
				continue;
			}
			if (playerTotal[key].disconnected)
			{
				break;
			}
			newPointBlocks.Add(pointBlock);
			if (key.IsLocalPlayer)
			{
				SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(key.localNumber, fallback: true);
				switch (pointBlock.type)
				{
				case PointBlock.pointBlockType.coin:
					OnPointEarnedForPlayer(key.LocalPlayer, saveFileDataForLocalPlayer, "CoinsCollected", pointBlock.type);
					break;
				case PointBlock.pointBlockType.comeback:
					OnPointEarnedForPlayer(key.LocalPlayer, saveFileDataForLocalPlayer, "ComebackPointsEarned", pointBlock.type);
					break;
				case PointBlock.pointBlockType.soloWin:
					OnPointEarnedForPlayer(key.LocalPlayer, saveFileDataForLocalPlayer, "SoloPointsEarned", pointBlock.type);
					break;
				case PointBlock.pointBlockType.trap:
					OnPointEarnedForPlayer(key.LocalPlayer, saveFileDataForLocalPlayer, "TrapPointsEarned", pointBlock.type);
					break;
				case PointBlock.pointBlockType.winDead:
					OnPointEarnedForPlayer(key.LocalPlayer, saveFileDataForLocalPlayer, "PostmortemVictories", pointBlock.type);
					break;
				}
			}
		}
	}

	public bool SpecialPointCheck(GamePlayer player)
	{
		bool result = false;
		foreach (PointBlock historyPointBlock in historyPointBlocks)
		{
			if (historyPointBlock.playerNumber == player.networkNumber && historyPointBlock.type != PointBlock.pointBlockType.win)
			{
				result = true;
			}
		}
		return result;
	}
}
