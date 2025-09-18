using System.Collections.Generic;

public class UserReports
{
	public class ReportInformation
	{
		public string reporterUsername;

		public string reporterGSID;

		public string reporterPlatformID;

		public LobbyPlayer.SocialPlatform reporterPlatform;

		public string reportedUsername;

		public string reportedGSID;

		public string reportedPlatformID;

		public LobbyPlayer.SocialPlatform reportedPlatform;

		public ReportReason reportReason;

		public string reportComments;

		public string reportChatlog;

		public string reportLevelCode;
	}

	public enum ReportReason
	{
		OffensiveMessage,
		OffensiveUsername,
		DisruptiveBehavior,
		Cheating,
		Other
	}

	private static HashSet<string> alreadyReportedGSIDs = new HashSet<string>();

	public static void NotifyReportedUser(string GSID)
	{
		alreadyReportedGSIDs.Add(GSID);
	}

	public static bool PlayerReportedThisSession(LobbyPlayer lobbyPl)
	{
		return alreadyReportedGSIDs.Contains(lobbyPl.GSID);
	}

	public static void ClearReportedUserLog()
	{
		alreadyReportedGSIDs.Clear();
	}
}
