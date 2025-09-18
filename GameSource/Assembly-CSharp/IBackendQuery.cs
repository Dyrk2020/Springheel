using System;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using GameSparks.Core;

public interface IBackendQuery
{
	string Error { get; }

	bool HasError { get; }

	bool IsDone { get; }

	bool IsRunning { get; }

	void AddChallengeAttempt(string code, List<string> playerIds, bool successful, int secondsInLevel);

	void CastLevelVote(string code, int vote);

	void CreateMatch(Action<LogEventResponse> callback);

	void ForcePurge();

	void GetChallengeTimes(string code, int numPlayers, int startIndex, int maxRecords);

	void GetFeaturedLevelList(int fromIndex, int numDisplay, FeaturedQuickFilter.SortingFilter sortingFilter);

	void GetLevelInfo(List<string> codeList);

	void GetLevelPublishStatus(string code);

	void GetLevelReports(string code);

	void GetLobbyData(string matchID, bool useCode, Action<LogEventResponse> callback, bool reserveSlot = false);

	void GetLobbyList(string version, int regionFilterIndex, LobbyPlayer.SocialPlatform platform, bool disallowCrossplay, Action<LogEventResponse> callback);

	void GetMyLevelRating(string code);

	void GetMyLevelReport(string code);

	bool GetResultDataBool(string key, bool defaultValue);

	float GetResultDataFloat(string key, float defaultValue);

	GSData GetResultDataGSData(string key);

	int GetResultDataInt(string key, int defaultValue);

	void GetThumbnailForSnapshotCode(string snapshotCode);

	void GetXmlStringFromSnapshotCode(string snapshotCode, bool incrementGetCount = true);

	void NotifySnapshotPlayed(string inputCode);

	void SendSimpleRequest(string eventKey, Dictionary<string, object> eventData, bool returnScriptData);

	void SetLevelApprovalStatus(string code, int approvalStatus);

	void SetLevelPublishStatus(string code, bool published, FeaturedQuickFilter.LevelTypes levelType);

	void SetLobbyData(string matchID, GSRequestData matchData, Action<LogEventResponse> callback);

	void SetLobbyHeartbeat();

	void SubmitChallengeTime(string code, List<string> playerIds, float time, bool allCoins, bool noUpdate, List<string> ghostUploadIDs);

	void SubmitLevelReport(string code, int reportReason, string reportComment, bool delete);

	void SubmitUserReport(UserReports.ReportInformation reportInformation);

	void Update();

	void UploadGhostData(byte[] bytes);

	void UploadLevelThumbnail(string code, byte[] bytes);

	void UploadStringAsFile(string fileContents, string uploadName, bool published, FeaturedQuickFilter.LevelTypes levelType, bool hasMods);

	void WakeUp();
}
