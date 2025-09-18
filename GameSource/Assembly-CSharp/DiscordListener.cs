using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Discord;
using I2.Loc;
using UCHServices;
using UnityEngine;

public class DiscordListener : MonoBehaviour
{
	private static DiscordListener instance;

	private static byte[] bytes = new byte[16]
	{
		200, 145, 53, 255, 49, 219, 239, 163, 44, 13,
		14, 218, 60, 140, 108, 165
	};

	private global::Discord.Discord discord;

	private bool discordRunning;

	private Activity currentActivity;

	public static void SetDefaultPresenceString(Player player = null)
	{
		if (instance != null)
		{
			instance.setDefaultPresenceString(player);
		}
	}

	public static void SetGamePresenceString(GameState.LevelName levelName, string levelCode, GameState.GameMode gameMode, bool online)
	{
		if (instance != null)
		{
			instance.setGamePresenceString(levelName, levelCode, gameMode, online);
		}
	}

	public static void SetLobbyPresenceString(GameState.GameMode gameMode, bool online)
	{
		if (instance != null)
		{
			instance.setLobbyPresenceString(gameMode, online);
		}
	}

	public static void UpdatePresencePlayers(int players)
	{
		if (instance != null)
		{
			instance.updatePresencePlayers(players);
		}
	}

	public static void UpdateLobbyTag(LobbyTags tag)
	{
		if (instance != null)
		{
			instance.updateLobbyTag(tag);
		}
	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			try
			{
				Debug.Log("Trying to start Discord");
				discord = new global::Discord.Discord(854100476920201216L, 1uL);
				discord.GetActivityManager().RegisterSteam(386940u);
				discordRunning = true;
				discord.GetActivityManager().ClearActivity(delegate(Result result)
				{
					Debug.Log($"Discord activity cleared with result {result}");
				});
				discord.GetActivityManager().OnActivityInvite += DiscordListener_OnActivityInvite;
				discord.GetActivityManager().OnActivityJoin += DiscordListener_OnActivityJoin;
				discord.GetActivityManager().OnActivityJoinRequest += DiscordListener_OnActivityJoinRequest;
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Debug.LogWarning("Couldn't start Discord API. Is Discord running?");
				return;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void DiscordListener_OnActivityJoinRequest(ref User user)
	{
		Debug.Log("Received join request from " + user.Username + "#" + user.Discriminator);
		discord.GetActivityManager().SendRequestReply(user.Id, ActivityJoinRequestReply.Ignore, delegate(Result result)
		{
			Debug.Log($"Discord invitation request acknowledged with result {result}");
		});
	}

	private void DiscordListener_OnActivityJoin(string secret)
	{
		GameSettings.GetInstance().StartAsHost = false;
		GameSettings.GetInstance().StartLocal = false;
		try
		{
			byte[] array = Convert.FromBase64String(secret);
			Aes aes = Aes.Create();
			byte[] array2 = new byte[16];
			byte[] array3 = new byte[array.Length - 16];
			Array.Copy(array, array2, 16);
			Array.Copy(array, 16, array3, 0, array3.Length);
			aes.IV = array2;
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(bytes, array2), CryptoStreamMode.Write);
			cryptoStream.Write(array3, 0, array3.Length);
			cryptoStream.FlushFinalBlock();
			memoryStream.Position = 0L;
			byte[] array4 = new byte[memoryStream.Length];
			memoryStream.Read(array4, 0, array4.Length);
			cryptoStream.Close();
			memoryStream.Close();
			string code = Encoding.UTF8.GetString(array4);
			Debug.Log("Join lobby with code " + code);
			Matchmaker matchmaker = Matchmaker.Instance;
			if (matchmaker.IsInLobby() || LobbyManager.instance != null)
			{
				if (LobbyManager.instance.IsInOnlineGame && matchmaker.CurrentLobby != null && matchmaker.CurrentLobby.GetLobbyCode() == code)
				{
					Debug.Log("[Net] Discord: Client was invited to the same lobby they are in.");
					return;
				}
				GameState.GetInstance().PreservePlayers = true;
				for (int i = 1; i < 5; i++)
				{
					PlayerManager.GetInstance().GetPlayer(i)?.Reset(full: false);
				}
				LobbyManagerManager.AbortGameInProgressGracefully();
				matchmaker.LeaveLobby();
			}
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.JoiningRequestNoName, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			LobbyManagerManager.WaitForMainMenu(delegate
			{
				if (matchmaker.CurrentLobby == null)
				{
					matchmaker.JoinLobby(code, useCode: true, delegate(bool success)
					{
						if (success)
						{
							if (Matchmaker.CurrentMatchmakingLobby != null)
							{
								AnalyticEvent.JoinMatchEvent(Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid(), AnalyticEvent.JoinMethod.DISCORD, Matchmaker.CurrentMatchmakingLobby.LobbyIsCrossplay(Application.platform));
							}
							else
							{
								Debug.LogError("ERROR: Matchmaker.CurrentMatchmakingLobby is null - JoinMatchEvent could not be sent");
							}
						}
					});
				}
			});
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Problem_joining_the_lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
		}
	}

	private void DiscordListener_OnActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
	{
		Debug.Log($"{type} Invitation sent to {user.Username}#{user.Discriminator}");
	}

	private void Update()
	{
		if (discord != null)
		{
			discord.RunCallbacks();
		}
	}

	private void updateDiscordActivity(Activity activityData)
	{
		if (discord == null)
		{
			return;
		}
		discord.GetActivityManager().UpdateActivity(activityData, delegate(Result result)
		{
			if (result != Result.Ok)
			{
				Debug.LogError($"Problem updating Discord Rich Presence: {result}");
			}
		});
	}

	private void setDefaultPresenceString(Player player = null)
	{
		if (discordRunning)
		{
			currentActivity = new Activity
			{
				Type = ActivityType.Playing,
				State = ScriptLocalization.Network_Rich_Presence.Default,
				Assets = 
				{
					LargeImage = "defaulticon"
				}
			};
			updateDiscordActivity(currentActivity);
		}
	}

	private void setGamePresenceString(GameState.LevelName levelName, string levelCode, GameState.GameMode gameMode, bool online)
	{
		if (discordRunning)
		{
			string text = GameState.GetLocalizedGameModeName(gameMode) + " " + ScriptLocalization.InLobby.Mode;
			string localizedLevelName = LevelSelectController.GetLocalizedLevelName(levelName);
			string state = text;
			string largeText = ((!levelCode.NullOrEmpty()) ? $"{localizedLevelName} - {levelCode}" : localizedLevelName);
			currentActivity = new Activity
			{
				Type = ActivityType.Playing,
				State = state,
				Details = localizedLevelName,
				Assets = 
				{
					LargeImage = levelName.ToString().ToLower(),
					SmallImage = "defaulticon",
					LargeText = largeText
				}
			};
			if (online)
			{
				setPartyOnline(ref currentActivity, joinable: false);
			}
			updateDiscordActivity(currentActivity);
		}
	}

	private void setLobbyPresenceString(GameState.GameMode gameMode, bool online)
	{
		if (discordRunning)
		{
			string state = GameState.GetLocalizedGameModeName(gameMode) + " " + ScriptLocalization.InLobby.Mode;
			string details = "";
			if (online)
			{
				details = ScriptLocalization.Network.LobbyOptions;
			}
			currentActivity = new Activity
			{
				Type = ActivityType.Playing,
				State = state,
				Details = details,
				Assets = 
				{
					LargeImage = "defaulticon"
				}
			};
			if (online)
			{
				setPartyOnline(ref currentActivity, joinable: true);
			}
			updateDiscordActivity(currentActivity);
		}
	}

	private void setPartyOnline(ref Activity activity, bool joinable)
	{
		int numPlayers = LobbyManager.instance.numPlayers;
		activity.Party = new ActivityParty
		{
			Id = Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid().ToString(),
			Size = 
			{
				CurrentSize = numPlayers,
				MaxSize = 4
			}
		};
		Matchmaker matchmaker = Matchmaker.Instance;
		if (matchmaker.CurrentLobby == null)
		{
			return;
		}
		try
		{
			string text = "";
			switch (matchmaker.CurrentLobby.GetLobbyTag())
			{
			case LobbyTags.Fun:
				text = ScriptLocalization.Network_Tag.Fun;
				break;
			case LobbyTags.Beginner:
				text = ScriptLocalization.Network_Tag.Beginner;
				break;
			case LobbyTags.Competitive:
				text = ScriptLocalization.Network_Tag.Competitive;
				break;
			case LobbyTags.CustomLevels:
				text = ScriptLocalization.Network_Tag.CustomLevels;
				break;
			}
			AvailableRegion lobbyRegion = matchmaker.CurrentLobby.GetLobbyRegion();
			text = text + " (" + lobbyRegion.LocalizedShortName + ")";
			ref string details = ref activity.Details;
			details = details + ", " + text;
			currentActivity.Instance = true;
			if (joinable)
			{
				byte[] array = Encoding.UTF8.GetBytes(matchmaker.CurrentLobby.GetLobbyCode());
				Aes aes = Aes.Create();
				Convert.ToBase64String(aes.IV);
				MemoryStream memoryStream = new MemoryStream();
				CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(bytes, aes.IV), CryptoStreamMode.Write);
				cryptoStream.Write(array, 0, array.Length);
				cryptoStream.FlushFinalBlock();
				memoryStream.Position = 0L;
				byte[] array2 = new byte[memoryStream.Length];
				memoryStream.Read(array2, 0, array2.Length);
				cryptoStream.Close();
				memoryStream.Close();
				byte[] array3 = new byte[16 + array2.Length];
				aes.IV.CopyTo(array3, 0);
				array2.CopyTo(array3, 16);
				string text2 = Convert.ToBase64String(array3);
				activity.Secrets = new ActivitySecrets
				{
					Join = text2
				};
			}
			else
			{
				activity.Secrets = default(ActivitySecrets);
			}
			ref string state = ref activity.State;
			state = state + ", " + ScriptLocalization.Network.Players + ": ";
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void updatePresencePlayers(int players)
	{
		currentActivity.Party.Size.CurrentSize = players;
		updateDiscordActivity(currentActivity);
	}

	private async void updateLobbyTag(LobbyTags tag)
	{
		Matchmaker matchmaker = Matchmaker.Instance;
		if (matchmaker.CurrentLobby != null)
		{
			string details = "";
			switch (tag)
			{
			case LobbyTags.Fun:
				details = ScriptLocalization.Network_Tag.Fun;
				break;
			case LobbyTags.Beginner:
				details = ScriptLocalization.Network_Tag.Beginner;
				break;
			case LobbyTags.Competitive:
				details = ScriptLocalization.Network_Tag.Competitive;
				break;
			case LobbyTags.CustomLevels:
				details = ScriptLocalization.Network_Tag.CustomLevels;
				break;
			}
			await UniTask.WaitUntil(() => matchmaker.CurrentLobby.GetLobbyRegion() != null);
			AvailableRegion lobbyRegion = matchmaker.CurrentLobby.GetLobbyRegion();
			details = details + " (" + lobbyRegion.LocalizedShortName + ")";
			currentActivity.Details = currentActivity.Details + " " + details;
			updateDiscordActivity(currentActivity);
		}
	}

	private void OnDestroy()
	{
		if (discordRunning)
		{
			discord.GetActivityManager().ClearActivity(delegate(Result result)
			{
				Debug.Log($"Discord activity cleared with result {result}");
			});
			discord.Dispose();
		}
	}
}
