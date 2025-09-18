public class NetMsgTypes
{
	private static short msgCount = 0;

	public static readonly short NetworkClientConnected = (short)(47 + ++msgCount);

	public static readonly short NetworkClientDisconnected = (short)(47 + ++msgCount);

	public static readonly short GameRuleSet = (short)(47 + ++msgCount);

	public static readonly short CharacterPicked = (short)(47 + ++msgCount);

	public static readonly short SetBlockFrequency = (short)(47 + ++msgCount);

	public static readonly short SetAllBlockFrequencies = (short)(47 + ++msgCount);

	public static readonly short SendAllBlockFrequencies = (short)(47 + ++msgCount);

	public static readonly short PartyBoxOpen = (short)(47 + ++msgCount);

	public static readonly short PartyBoxClosed = (short)(47 + ++msgCount);

	public static readonly short PiecePicked = (short)(47 + ++msgCount);

	public static readonly short PiecePlaced = (short)(47 + ++msgCount);

	public static readonly short PieceDestroyed = (short)(47 + ++msgCount);

	public static readonly short CharacterSuccess = (short)(47 + ++msgCount);

	public static readonly short ProjectileHit = (short)(47 + ++msgCount);

	public static readonly short TrapTriggered = (short)(47 + ++msgCount);

	public static readonly short PointAwarded = (short)(47 + ++msgCount);

	public static readonly short PointsCleared = (short)(47 + ++msgCount);

	public static readonly short ReadyToTallyPoints = (short)(47 + ++msgCount);

	public static readonly short NetworkSurrogateSpawned = (short)(47 + ++msgCount);

	public static readonly short SetNetworkSurrogateVal = (short)(47 + ++msgCount);

	public static readonly short SwitchToMode = (short)(47 + ++msgCount);

	public static readonly short BookPiecePicked = (short)(47 + ++msgCount);

	public static readonly short SetPartyPieceID = (short)(47 + ++msgCount);

	public static readonly short ClientLoadedTreehouse = (short)(47 + ++msgCount);

	public static readonly short ChatSent = (short)(47 + ++msgCount);

	public static readonly short PortalHasUnlock = (short)(47 + ++msgCount);

	public static readonly short UnlockAvailable = (short)(47 + ++msgCount);

	public static readonly short ClientKicked = (short)(47 + ++msgCount);

	public static readonly short VoteToKick = (short)(47 + ++msgCount);

	public static readonly short ConnectionQuality = (short)(47 + ++msgCount);

	public static readonly short LobbyVoting = (short)(47 + ++msgCount);

	public static readonly short PlayerSkillUpdated = (short)(47 + ++msgCount);

	public static readonly short SetGameModeLock = (short)(47 + ++msgCount);

	public static readonly short PlayerHandicapSet = (short)(47 + ++msgCount);

	public static readonly short AFKPlayer = (short)(47 + ++msgCount);

	public static readonly short PiecePickedUp = (short)(47 + ++msgCount);

	public static readonly short SnapshotLoadingDone = (short)(47 + ++msgCount);

	public static readonly short ShowCredits = (short)(47 + ++msgCount);

	public static readonly short CommunicateCharacterOutfits = (short)(47 + ++msgCount);

	public static readonly short SetCustomPortalInfo = (short)(47 + ++msgCount);

	public static readonly short LobbyDataUpdated = (short)(47 + ++msgCount);

	public static readonly short SetCustomBackground = (short)(47 + ++msgCount);

	public static readonly short SetCustomMusic = (short)(47 + ++msgCount);

	public static readonly short UpdateVoteKickCounts = (short)(47 + ++msgCount);

	public static readonly short PlayerWantsToRetry = (short)(47 + ++msgCount);

	public static readonly short PlayerReadyToStart = (short)(47 + ++msgCount);

	public static readonly short PrepareToReloadScene = (short)(47 + ++msgCount);

	public static readonly short SetCustomAmbience = (short)(47 + ++msgCount);

	public static readonly short ProjectileDestroyed = (short)(47 + ++msgCount);

	public static readonly short HostEndedGame = (short)(47 + ++msgCount);

	public static readonly short AFKTimerChanged = (short)(47 + ++msgCount);

	public static readonly short PunchingBlockTriggered = (short)(47 + ++msgCount);

	public static readonly short ApplyRuleset = (short)(47 + ++msgCount);

	public static readonly short RulesetDirty = (short)(47 + ++msgCount);

	public static readonly short ForcedPieceSpawned = (short)(47 + ++msgCount);

	public static readonly short PlatformDancing = (short)(47 + ++msgCount);

	public static readonly short ThwompTriggered = (short)(47 + ++msgCount);

	public static string GetMessageName(short messageType)
	{
		if (messageType == NetworkClientConnected)
		{
			return "NetworkClientConnected";
		}
		if (messageType == NetworkClientDisconnected)
		{
			return "NetworkClientDisconnected";
		}
		if (messageType == GameRuleSet)
		{
			return "GameRuleSet";
		}
		if (messageType == CharacterPicked)
		{
			return "CharacterPicked";
		}
		if (messageType == SetBlockFrequency)
		{
			return "SetBlockFrequency";
		}
		if (messageType == SetAllBlockFrequencies)
		{
			return "SetAllBlockFrequencies";
		}
		if (messageType == SendAllBlockFrequencies)
		{
			return "SendAllBlockFrequencies";
		}
		if (messageType == PartyBoxOpen)
		{
			return "PartyBoxOpen";
		}
		if (messageType == PartyBoxClosed)
		{
			return "PartyBoxClosed";
		}
		if (messageType == PiecePicked)
		{
			return "PiecePicked";
		}
		if (messageType == PiecePlaced)
		{
			return "PiecePlaced";
		}
		if (messageType == PieceDestroyed)
		{
			return "PieceDestroyed";
		}
		if (messageType == CharacterSuccess)
		{
			return "CharacterSuccess";
		}
		if (messageType == ProjectileHit)
		{
			return "ProjectileHit";
		}
		if (messageType == TrapTriggered)
		{
			return "TrapTriggered";
		}
		if (messageType == PointAwarded)
		{
			return "PointAwared";
		}
		if (messageType == PointsCleared)
		{
			return "PointsCleared";
		}
		if (messageType == ReadyToTallyPoints)
		{
			return "ReadyToTallyPoints";
		}
		if (messageType == NetworkSurrogateSpawned)
		{
			return "NetworkSurrogateSpawned";
		}
		if (messageType == SetNetworkSurrogateVal)
		{
			return "SetNetworkSurrogateVal";
		}
		if (messageType == SwitchToMode)
		{
			return "SwitchToMode";
		}
		if (messageType == BookPiecePicked)
		{
			return "BookPiecePicked";
		}
		if (messageType == SetPartyPieceID)
		{
			return "SetPartyPieceID";
		}
		if (messageType == ClientLoadedTreehouse)
		{
			return "ClientLoadedTreehouse";
		}
		if (messageType == ChatSent)
		{
			return "ChatSent";
		}
		if (messageType == PortalHasUnlock)
		{
			return "PortalHasUnlock";
		}
		if (messageType == UnlockAvailable)
		{
			return "UnlockAvailable";
		}
		if (messageType == ClientKicked)
		{
			return "ClientKicked";
		}
		if (messageType == VoteToKick)
		{
			return "VoteToKick";
		}
		if (messageType == ConnectionQuality)
		{
			return " ConnectionQuality";
		}
		if (messageType == PlayerSkillUpdated)
		{
			return " PlayerSkillUpdated";
		}
		if (messageType == SetGameModeLock)
		{
			return " SetGameModeLock";
		}
		if (messageType == PlayerHandicapSet)
		{
			return " PlayerHandicapSet";
		}
		if (messageType == AFKPlayer)
		{
			return "AFKPlayer";
		}
		if (messageType == PiecePickedUp)
		{
			return "PiecePickedUp";
		}
		if (messageType == SnapshotLoadingDone)
		{
			return "SnapshotLoadingDone";
		}
		if (messageType == ShowCredits)
		{
			return "ShowCredits";
		}
		if (messageType == CommunicateCharacterOutfits)
		{
			return "CommunicateCharacterOutfits";
		}
		if (messageType == SetCustomPortalInfo)
		{
			return "SetCustomPortalInfo";
		}
		if (messageType == LobbyDataUpdated)
		{
			return "LobbyDataUpdated";
		}
		if (messageType == SetCustomBackground)
		{
			return "SetCustomBackground";
		}
		if (messageType == SetCustomMusic)
		{
			return "SetCustomMusic";
		}
		if (messageType == UpdateVoteKickCounts)
		{
			return "UpdateVoteKickCounts";
		}
		if (messageType == PlayerWantsToRetry)
		{
			return "PlayerWantsToRetry";
		}
		if (messageType == PlayerReadyToStart)
		{
			return "PlayerReadyToStart";
		}
		if (messageType == PrepareToReloadScene)
		{
			return "PrepareToReloadScene";
		}
		if (messageType == SetCustomAmbience)
		{
			return "SetCustomAmbience";
		}
		if (messageType == ProjectileDestroyed)
		{
			return "ProjectileDestroyed";
		}
		if (messageType == HostEndedGame)
		{
			return "HostendedGame";
		}
		if (messageType == AFKTimerChanged)
		{
			return "AFKTimerChanged";
		}
		if (messageType == PunchingBlockTriggered)
		{
			return "PunchingBlockTriggered";
		}
		if (messageType == ApplyRuleset)
		{
			return "ApplyRuleset";
		}
		if (messageType == RulesetDirty)
		{
			return "RulesetDirty";
		}
		if (messageType == ForcedPieceSpawned)
		{
			return "ForcedPieceSpawned";
		}
		if (messageType == PlatformDancing)
		{
			return "PlatformDancing";
		}
		if (messageType == ThwompTriggered)
		{
			return "ThwompTriggered";
		}
		return "unknown";
	}
}
