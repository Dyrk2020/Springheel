using System;
using nn.account;

namespace nn.friends;

public static class Friends
{
	public const int FriendCountMax = 300;

	public const int BlockedUserCountMax = 100;

	public const long PresenceAppFieldSize = 192L;

	public const int InAppScreenNameLengthMax = 20;

	public const int GetProfileCountMax = 100;

	public const long ProfileImageSizeMax = 131072L;

	public const int NotificationCountMax = 100;

	public const int FriendInvitationInviteeCountMax = 16;

	public const long FriendInvitationApplicationParameterSizeMax = 1024L;

	public static ErrorRange ResultNotInitialized => new ErrorRange(121, 1, 2);

	public static ErrorRange ResultInvalidArgument => new ErrorRange(121, 2, 3);

	public static ErrorRange ResultUserNotOpened => new ErrorRange(121, 3, 4);

	public static ErrorRange ResultNetworkServiceAccountNotLinked => new ErrorRange(121, 4, 5);

	public static ErrorRange ResultOwnNetworkServiceAccountSpecified => new ErrorRange(121, 5, 6);

	public static ErrorRange ResultInternetRequestNotAccepted => new ErrorRange(121, 6, 7);

	public static ErrorRange ResultNotCalled => new ErrorRange(121, 7, 8);

	public static ErrorRange ResultCallInProgress => new ErrorRange(121, 8, 9);

	public static ErrorRange ResultCanceled => new ErrorRange(121, 9, 10);

	public static ErrorRange ResultProfileImageCacheNotFound => new ErrorRange(121, 10, 11);

	public static ErrorRange ResultOutOfMemory => new ErrorRange(121, 11, 12);

	public static ErrorRange ResultOutOfResource => new ErrorRange(121, 12, 13);

	public static ErrorRange ResultReservedKey => new ErrorRange(121, 13, 14);

	public static ErrorRange ResultDuplicatedKey => new ErrorRange(121, 14, 15);

	public static ErrorRange ResultNotificationNotFound => new ErrorRange(121, 15, 16);

	public static ErrorRange ResultPlayHistoryRegistrationKeyBroken => new ErrorRange(121, 21, 22);

	public static ErrorRange ResultOwnPlayHistoryRegistrationKey => new ErrorRange(121, 22, 23);

	public static ErrorRange ResultAppletCanceled => new ErrorRange(121, 30, 31);

	public static ErrorRange ResultApplicationInfoNotRegistered => new ErrorRange(121, 40, 41);

	public static ErrorRange ResultNotPermitted => new ErrorRange(121, 90, 91);

	public static ErrorRange ResultInvalidOperation => new ErrorRange(121, 92, 93);

	public static ErrorRange ResultNotImplemented => new ErrorRange(121, 99, 100);

	public static ErrorRange ResultResponseFormatError => new ErrorRange(121, 100, 200);

	public static ErrorRange ResultHttpError => new ErrorRange(121, 1000, 2000);

	public static ErrorRange ResultServerError => new ErrorRange(121, 2000, 4000);

	public static void Initialize()
	{
	}

	public static Result GetFriendList(ref int outCount, NetworkServiceAccountId[] outAccountIds, Uid uid, int offset, FriendFilter filter)
	{
		return default(Result);
	}

	public static Result GetFriendList(ref int outCount, NetworkServiceAccountId[] outAccountIds, int offset, FriendFilter filter)
	{
		return default(Result);
	}

	public static Result GetFriendList(ref int outCount, Friend[] outFriends, Uid uid, int offset, FriendFilter filter)
	{
		return default(Result);
	}

	public static Result GetFriendList(ref int outCount, Friend[] outFriends, int offset, FriendFilter filter)
	{
		return default(Result);
	}

	public static Result UpdateFriendInfo(Friend[] outFriends, Uid uid, NetworkServiceAccountId[] accountIds)
	{
		return default(Result);
	}

	public static Result UpdateFriendInfo(Friend[] outFriends, NetworkServiceAccountId[] accountIds)
	{
		return default(Result);
	}

	public static Result CheckFriendListAvailability(ref bool outIsAvailable, Uid uid)
	{
		return default(Result);
	}

	public static Result CheckFriendListAvailability(ref bool outIsAvailable)
	{
		return default(Result);
	}

	public static Result EnsureFriendListAvailable(AsyncContext outAsync, Uid uid)
	{
		return default(Result);
	}

	public static Result EnsureFriendListAvailable(AsyncContext outAsync)
	{
		return default(Result);
	}

	public static Result GetBlockedUserList(ref int outCount, NetworkServiceAccountId[] outAccountIds, Uid uid, int offset)
	{
		return default(Result);
	}

	public static Result GetBlockedUserList(ref int outCount, NetworkServiceAccountId[] outAccountIds, int offset)
	{
		return default(Result);
	}

	public static Result CheckBlockedUserListAvailability(ref bool outIsAvailable, Uid uid)
	{
		return default(Result);
	}

	public static Result CheckBlockedUserListAvailability(ref bool outIsAvailable)
	{
		return default(Result);
	}

	public static Result EnsureBlockedUserListAvailable(AsyncContext outAsync, Uid uid)
	{
		return default(Result);
	}

	public static Result EnsureBlockedUserListAvailable(AsyncContext outAsync)
	{
		return default(Result);
	}

	public static Result GetProfileList(AsyncContext outAsync, Profile[] outProfiles, Uid uid, NetworkServiceAccountId[] accountIds)
	{
		return default(Result);
	}

	public static Result GetProfileList(AsyncContext outAsync, Profile[] outProfiles, NetworkServiceAccountId[] accountIds)
	{
		return default(Result);
	}

	public static Result DeclareOpenOnlinePlaySession(Uid uid)
	{
		return default(Result);
	}

	public static Result DeclareOpenOnlinePlaySession()
	{
		return default(Result);
	}

	public static Result DeclareCloseOnlinePlaySession(Uid uid)
	{
		return default(Result);
	}

	public static Result DeclareCloseOnlinePlaySession()
	{
		return default(Result);
	}

	public static Result GetPlayHistoryRegistrationKey(ref PlayHistoryRegistrationKey outKey, Uid uid, bool isLocalPlay)
	{
		return default(Result);
	}

	public static Result GetPlayHistoryRegistrationKey(ref PlayHistoryRegistrationKey outKey, bool isLocalPlay)
	{
		return default(Result);
	}

	public static Result AddPlayHistory(Uid uid, PlayHistoryRegistrationKey key, InAppScreenName inAppScreenName, InAppScreenName myInAppScreenName)
	{
		return default(Result);
	}

	public static Result AddPlayHistory(PlayHistoryRegistrationKey key, InAppScreenName inAppScreenName, InAppScreenName myInAppScreenName)
	{
		return default(Result);
	}

	public static bool TryPopFriendInvitationNotificationInfo(ref Uid pOutUid, ref long pOutSize, byte[] pOutBuffer)
	{
		pOutUid = default(Uid);
		pOutSize = 0L;
		pOutBuffer[0] = 0;
		return false;
	}

	public static Result ShowFriendList(Uid uid)
	{
		return default(Result);
	}

	public static Result ShowUserDetailInfo(Uid uid, NetworkServiceAccountId accountId, InAppScreenName myInAppScreenName, InAppScreenName inAppScreenName)
	{
		return default(Result);
	}

	public static Result StartSendingFriendRequest(Uid uid, NetworkServiceAccountId accountId, InAppScreenName myInAppScreenName, InAppScreenName inAppScreenName)
	{
		return default(Result);
	}

	[Obsolete("It will be removed in NX Addon 18 series or later")]
	public static Result ShowMethodsOfSendingFriendRequest(Uid uid)
	{
		return default(Result);
	}

	[Obsolete("It will be removed in NX Addon 18 series or later")]
	public static Result StartFacedFriendRequest(Uid uid)
	{
		return default(Result);
	}

	[Obsolete("It will be removed in NX Addon 18 series or later")]
	public static Result ShowReceivedFriendRequestList(Uid uid)
	{
		return default(Result);
	}

	[Obsolete("It will be removed in NX Addon 18 series or later")]
	public static Result ShowBlockedUserList(Uid uid)
	{
		return default(Result);
	}

	public static Result StartFriendInvitation(Uid uid, int maxInviteeCount, FriendInvitationGameModeDescription description, byte[] pAppParameter)
	{
		return default(Result);
	}

	public static Result StartSendingFriendInvitation(Uid uid, NetworkServiceAccountId[] pInvitees, FriendInvitationGameModeDescription description, byte[] pAppParameter)
	{
		return default(Result);
	}
}
