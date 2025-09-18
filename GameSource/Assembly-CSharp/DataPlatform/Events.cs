using System;
using System.Diagnostics;

namespace DataPlatform;

public class Events
{
	[Conditional("UNITY_XBOXONE")]
	public static void SendAchievementUpdate()
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCharacterUnlocked(string UserId, ref Guid PlayerSessionId, string Character)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCoinDropped(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendContraptionBuilt(string UserId, ref Guid PlayerSessionId, int Pieces)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendGameProgress(string UserId, ref Guid PlayerSessionId, float CompletionPercent)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendLevelUnlocked(string UserId, ref Guid PlayerSessionId, string LevelName)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMatchComplete(string UserId, ref Guid PlayerSessionId, string GameMode, string LevelName, bool WasOnline, bool WithLocalFriend)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMatchStarted(string UserId, ref Guid PlayerSessionId, string Level, string LevelCode)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMatchWon(string UserId, ref Guid PlayerSessionId, bool Online, int Players, bool WithSpecialPoints)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMediaUsage(string AppSessionId, string AppSessionStartDateTime, uint UserIdType, string UserId, string SubscriptionTierType, string SubscriptionTier, string MediaType, string ProviderId, string ProviderMediaId, string ProviderMediaInstanceId, ref Guid BingId, ulong MediaLengthMs, uint MediaControlAction, float PlaybackSpeed, ulong MediaPositionMs, ulong PlaybackDurationMs, string AcquisitionType, string AcquisitionContext, string AcquisitionContextType, string AcquisitionContextId, int PlaybackIsStream, int PlaybackIsTethered, string MarketplaceLocation, string ContentLocale, float TimeZoneOffset, uint ScreenState)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendModePicked(string UserId, ref Guid PlayerSessionId, string GameMode)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMultiplayerRoundEnd(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId, float TimeInSeconds, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMultiplayerRoundStart(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendObjectiveEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendObjectiveStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendOutfitUnlocked(string UserId, ref Guid PlayerSessionId, string Character, int OutfitNumber)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPageAction(string UserId, ref Guid PlayerSessionId, int ActionTypeId, int ActionInputMethodId, string Page, string TemplateId, string DestinationPage, string Content)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPageView(string UserId, ref Guid PlayerSessionId, string Page, string RefererPage, int PageTypeId, string PageTags, string TemplateId, string Content)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPiecePlaced(string UserId, ref Guid PlayerSessionId, string PieceType)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerDefeated(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, ref Guid RoundId, int PlayerRoleId, int PlayerWeaponId, int EnemyRoleId, int EnemyWeaponId, float LocationX, float LocationY, float LocationZ)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerGetsKill(string UserId, ref Guid PlayerSessionId, string KillWith)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerJumped(string UserId, ref Guid PlayerSessionId, bool WallJump)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerKilled(string UserId, ref Guid PlayerSessionId, string KilledBy)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionEnd(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionPause(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionResume(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionStart(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSpawned(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, ref Guid RoundId, int PlayerRoleId, float LocationX, float LocationY, float LocationZ)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerTeleported(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPointEarned(string UserId, ref Guid PlayerSessionId, string PointType)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendRoundStarted(string UserId, ref Guid PlayerSessionId, bool SuddenDeath)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSectionEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSectionStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendViewOffer(string UserId, ref Guid PlayerSessionId, ref Guid OfferGuid, ref Guid ProductGuid)
	{
	}
}
