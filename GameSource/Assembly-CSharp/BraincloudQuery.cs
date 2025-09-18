using System;
using System.Collections;
using System.Collections.Generic;
using BCGSComponents;
using BCGSComponents.DataModels;
using BrainCloud;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UnityEngine;

public class BraincloudQuery : GameSparksQuery
{
	private Action<GSTypedResponse> onUploadComplete;

	private bool uploading;

	private string lastUploadId;

	public BraincloudQuery(bool debugOutput = false)
		: base(debugOutput)
	{
	}

	protected override void startWaitingForUpload(Action<GSTypedResponse> onComplete)
	{
	}

	protected override void doneWaitingForUpload(Action<GSTypedResponse> onComplete)
	{
	}

	protected override IEnumerator DoUploadFileFromBytes(byte[] bytes, string filename, string contentType, uploadURLDelegate setUploadURL, uploadDataDelegate setUploadData, uploadErrorDelegate setUploadError, resultDataDelegate populateResultData)
	{
		bool done = false;
		bool isLevel = contentType == "application/octet-stream";
		string cloudPath = (isLevel ? "uploadedLevels" : "thumbnails");
		SuccessCallback success = delegate
		{
			Debug.Log("Level has begun uploading");
		};
		FailureCallback failure = delegate(int status, int reasonCode, string jsonError, object cbObject)
		{
			Debug.LogError("Level has failed to start uploading");
			done = true;
			error = string.Format("{0} {1}", reasonCode, reasonCode switch
			{
				40429 => "UPLOAD_FILE_TOO_LARGE", 
				40430 => "FILE_ALREADY_EXISTS", 
				_ => "UNKNOWN", 
			});
		};
		FileUploadSuccessCallback success2 = delegate(string fileUploadId, string jsonResponse)
		{
			BraincloudManager.BC.Client.DeregisterFileUploadCallback();
			error = "";
			string uploadId = fileUploadId;
			SuccessCallback success3 = delegate(string jsonString, object cbObject)
			{
				string value = new BCGSData(jsonString).GetBCGSData("data").GetString("appServerUrl");
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				setUploadData(dictionary);
				if (isLevel)
				{
					dictionary.Add("uploadId", uploadId);
				}
				else
				{
					dictionary.Add("thumbnailId", uploadId);
				}
				dictionary.Add("fileURL", value);
				new BCGSRequestData(dictionary);
				LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("updateLevelDoc");
				foreach (KeyValuePair<string, object> item in dictionary)
				{
					logEventRequest.AddCustomData(item.Key, item.Value);
				}
				BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(delegate(GameSparks.Api.Responses.LogEventResponse response)
				{
					done = true;
					if (setUploadError != null)
					{
						error = setUploadError(response);
					}
					ResultData = new Dictionary<string, object>();
					ResultData.Add("uploadID", uploadId);
					if (populateResultData != null)
					{
						populateResultData(ResultData);
					}
				}));
			};
			FailureCallback failure3 = delegate(int status, int reasonCode, string jsonError, object cbObject)
			{
				Debug.LogError("Couldn't get CDN URL for level");
				BraincloudManager.BC.Client.DeregisterFileUploadCallback();
				done = true;
				error = string.Format("{0} {1}", reasonCode, reasonCode switch
				{
					40431 => "CLOUD_STORAGE_SERVICE_ERROR", 
					40432 => "FILE_DOES_NOT_EXIST", 
					_ => "UNKNOWN", 
				});
			};
			Debug.Log("File upload successful. Updating level doc");
			BraincloudManager.BC.FileService.GetCDNUrl(cloudPath, filename, success3, failure3);
		};
		FileUploadFailedCallback failure2 = delegate(string fileUploadId, int statusCode, int reasonCode, string jsonResponse)
		{
			Debug.LogError("Level has failed to upload");
			new BCGSData(jsonResponse);
			switch (reasonCode)
			{
			case 40429:
			{
				string text = "UPLOAD_FILE_TOO_LARGE";
				break;
			}
			case 40430:
			{
				string text = "FILE_ALREADY_EXISTS";
				break;
			}
			default:
			{
				string text = "UNKNOWN";
				break;
			}
			}
			error = $"{statusCode}-{reasonCode}";
		};
		BraincloudManager.BC.Client.RegisterFileUploadCallback(success2, failure2);
		BraincloudManager.BC.FileService.UploadFileFromMemory(cloudPath, filename, shareable: true, replaceIfExists: true, bytes, success, failure);
		while (!done)
		{
			yield return null;
		}
	}

	protected override void sendGetUploadUrlRequest(Dictionary<string, object> uploadData, Action<GameSparks.Api.Responses.GetUploadUrlResponse> responseHandler)
	{
		new GetUploadUrlRequest().SetUploadData(new BCGSRequestData(uploadData)).Send(delegate(BCGSComponents.GetUploadUrlResponse response)
		{
			responseHandler?.Invoke(new GameSparks.Api.Responses.GetUploadUrlResponse(new GSData(response.JSONData)));
		});
	}

	protected override IEnumerator doFileUpload(byte[] bytes, string filename, string contentType, string uploadURL)
	{
		bool done = false;
		SuccessCallback success = delegate(string jsonResponse, object cbObject)
		{
			done = true;
			error = "";
			BCGSData bCGSData = new BCGSData(jsonResponse);
			lastUploadId = bCGSData.GetBCGSData("data").GetBCGSData("fileDetails").GetString("uploadId");
		};
		FailureCallback failure = delegate(int status, int reasonCode, string jsonError, object cbObject)
		{
			done = true;
			error = string.Format("{0} {1}", reasonCode, reasonCode switch
			{
				40429 => "UPLOAD_FILE_TOO_LARGE", 
				40430 => "FILE_ALREADY_EXISTS", 
				_ => "UNKNOWN", 
			});
		};
		BraincloudManager.BC.FileService.UploadFileFromMemory(uploadURL, filename, shareable: true, replaceIfExists: true, bytes, success, failure);
		while (!done)
		{
			yield return null;
		}
	}

	protected override string getUploadIdFromResponse(GSTypedResponse response)
	{
		return lastUploadId;
	}

	private Action<BCGSComponents.LogEventResponse> convertToLogEventResponse(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		return delegate(BCGSComponents.LogEventResponse response)
		{
			BCGSComponents.LogEventResponse logEventResponse = new BCGSComponents.LogEventResponse(response.BaseData.GetBCGSData("data").GetBCGSData("response"));
			responseHandler?.Invoke(new GameSparks.Api.Responses.LogEventResponse(new GSData(logEventResponse.BaseData.BaseData)));
		};
	}

	protected override void sendAdminGetLevelReportsRequest(string code, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("adminGetLevelReports").SetEventAttribute("code", code), convertToLogEventResponse(responseHandler));
	}

	protected override void sendCastLevelVoteRequest(string snapshotCode, int vote, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("CastLevelVote").SetEventAttribute("levelCode", snapshotCode)
			.SetEventAttribute("vote", vote), convertToLogEventResponse(responseHandler));
	}

	protected override void sendCreateMatchEvent(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("createMatch_JS"), convertToLogEventResponse(responseHandler));
	}

	protected override void sendFreezeLobbyEvent(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler, string lobbyCode)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("freezeLobby").SetEventAttribute("lobbyCode", lobbyCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendUnfreezeLobbyEvent(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("unfreezeLobby"), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLobbyCodeEvent(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getFrozenLobby"), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetChallengeTimesRequest(string snapshotCode, int numPlayers, int startIndex, int maxRecords, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getChallengeTimes").SetEventAttribute("code", snapshotCode)
			.SetEventAttribute("numPlayers", numPlayers)
			.SetEventAttribute("startIndex", startIndex)
			.SetEventAttribute("maxRecords", maxRecords), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLevelInfoRequest(GSRequestData codeListObject, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getLevelInfo");
		BCGSRequestData bCGSRequestData = new BCGSRequestData(codeListObject.BaseData);
		logEventRequest.JSONData.Add("codeList", bCGSRequestData.BaseData);
		BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLevelListRequest(FeaturedQuickFilter.SortingFilter sortingFilter, int fromIndex, int numDisplay, GSRequestData codeListObject, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getLevelListBC").SetEventAttribute("filterType", (int)sortingFilter.filterType)
			.SetEventAttribute("sortBy", sortingFilter.sortBy)
			.SetEventAttribute("descending", sortingFilter.descending ? 1 : 0)
			.SetEventAttribute("startIndex", fromIndex)
			.SetEventAttribute("numRecords", numDisplay)
			.SetEventAttribute("levelType", sortingFilter.levelType.ToString())
			.SetEventAttribute("allowUnpublished", sortingFilter.allowUnpublished ? 1 : 0)
			.SetEventAttribute("cutoffDays", sortingFilter.cutoffDays)
			.SetEventAttribute("lowerDifficultyBound", sortingFilter.lowerDifficultyBound.ToString())
			.SetEventAttribute("upperDifficultyBound", sortingFilter.upperDifficultyBound.ToString())
			.SetEventAttribute("hideAcknowledged", sortingFilter.hideAcknowledged ? 1 : 0)
			.SetEventAttribute("approvalFilter", sortingFilter.approvalStatusFilter)
			.SetEventAttribute("maxLevelVersion", GameSettings.GetInstance().UploadLevelVersion)
			.SetEventAttribute("hasMods", sortingFilter.showMods);
		BCGSRequestData bCGSRequestData = new BCGSRequestData(codeListObject.BaseData);
		logEventRequest.JSONData.Add("codeList", bCGSRequestData.BaseData);
		if (!sortingFilter.searchTerms.NullOrEmpty() && sortingFilter.searchTerms.Length > 1)
		{
			logEventRequest.SetEventAttribute("searchTerms", sortingFilter.searchTerms);
		}
		if (!sortingFilter.restrictToUserId.NullOrEmpty())
		{
			if (sortingFilter.restrictToUserId != "da7c528a-6d03-4c18-a685-43f7fe9d08fa")
			{
				logEventRequest.SetEventAttribute("filterUserId", sortingFilter.restrictToUserId);
				logEventRequest.SetEventAttribute("filterGSID", "");
			}
			else
			{
				logEventRequest.SetEventAttribute("filterUserId", "");
				logEventRequest.SetEventAttribute("filterGSID", sortingFilter.restrictToGSID);
			}
		}
		else
		{
			logEventRequest.SetEventAttribute("filterUserId", "");
			logEventRequest.SetEventAttribute("filterGSID", "");
		}
		BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLevelPublishStatusRequest(string snapshotCode, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getLevelPublishStatus").SetEventAttribute("code", snapshotCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLevelThumbnailUrlRequest(string snapshotCode, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getlevelthumbnailurl").SetEventAttribute("code", snapshotCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLevelUploadUrlRequest(string snapshotCode, bool incrementGetCount, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getleveluploadurl").SetEventAttribute("code", snapshotCode)
			.SetEventAttribute("incrementGetCount", incrementGetCount ? 1 : 0)
			.SetEventAttribute("maxLevelVersion", GameSettings.GetInstance().UploadLevelVersion), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLobbyDataRequest(string matchID, bool useCode, bool reserveSlot, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getLobbyData").SetEventAttribute("matchID", matchID)
			.SetEventAttribute("useCode", useCode ? 1 : 0)
			.SetEventAttribute("reserveSlot", reserveSlot ? 1 : 0), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetLobbyListRequest(string version, GSRequestData filters, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getLobbyList").SetEventAttribute("version", version);
		BCGSRequestData bCGSRequestData = new BCGSRequestData(filters.BaseData);
		logEventRequest.JSONData.Add("filters", bCGSRequestData.BaseData);
		BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetMyLevelRatingRequest(string snapshotCode, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getMyLevelRating").SetEventAttribute("code", snapshotCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendGetMyLevelReportRequest(string snapshotCode, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("getMyLevelReport").SetEventAttribute("code", snapshotCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendNotifySnapshotPlayedRequest(string snapshotCode, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("notifysnapshotplayed").SetEventAttribute("code", snapshotCode), convertToLogEventResponse(responseHandler));
	}

	protected override void sendSetLobbyDataRequest(string matchID, GSRequestData matchData, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("setLobbyData").SetEventAttribute("matchID", matchID);
		BCGSRequestData bCGSRequestData = new BCGSRequestData(matchData.BaseData);
		logEventRequest.JSONData.Add("matchData", bCGSRequestData.BaseData);
		BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(responseHandler));
	}

	protected override void sendSetLobbyHeartbeatRequest(Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("setLobbyHeartbeat"), convertToLogEventResponse(responseHandler));
	}

	protected override void sendSimpleRequest(string eventKey, Dictionary<string, object> eventData, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		LogEventRequest logEventRequest = new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey(eventKey);
		foreach (KeyValuePair<string, object> eventDatum in eventData)
		{
			if (eventDatum.Value == null)
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, "");
				continue;
			}
			Type type = eventDatum.Value.GetType();
			if (type == typeof(int))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (int)eventDatum.Value);
			}
			else if (type == typeof(long))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (long)eventDatum.Value);
			}
			else if (type == typeof(string))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (string)eventDatum.Value);
			}
			else if (type == typeof(List<int>))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (List<int>)eventDatum.Value);
			}
			else if (type == typeof(List<long>))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (List<long>)eventDatum.Value);
			}
			else if (type == typeof(List<string>))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, (List<string>)eventDatum.Value);
			}
			else if (type == typeof(bool))
			{
				logEventRequest.SetEventAttribute(eventDatum.Key, ((bool)eventDatum.Value) ? 1 : 0);
			}
			else
			{
				Debug.LogError("Unknown type fed to Simple Request (" + type.ToString() + ")");
			}
		}
		BraincloudManager.SendLogEventRequest(logEventRequest, convertToLogEventResponse(responseHandler));
	}

	protected override void sendSubmitChallengeTimeRequest(string snapshotCode, List<string> playerIds, string time, bool allCoins, bool noUpdate, Action<GameSparks.Api.Responses.LogEventResponse> responseHandler)
	{
		BraincloudManager.SendLogEventRequest(new LogEventRequest().SetScriptData(new BCGSRequestData()).SetEventKey("submitChallengeTime").SetEventAttribute("code", snapshotCode)
			.SetEventAttribute("playerIds", playerIds)
			.SetEventAttribute("time", time.ToString())
			.SetEventAttribute("allCoins", allCoins ? 1 : 0)
			.SetEventAttribute("noUpdate", noUpdate ? 1 : 0), convertToLogEventResponse(responseHandler));
	}
}
