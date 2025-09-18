using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public abstract class GameSparksQuery
{
	public enum UniqueQueryTag
	{
		None,
		SetHeartbeat,
		SetLobbyData,
		WakeUp
	}

	protected delegate string uploadURLDelegate(GetUploadUrlResponse response);

	protected delegate void uploadDataDelegate(Dictionary<string, object> uploadData);

	protected delegate string uploadErrorDelegate(GSTypedResponse response);

	protected delegate void resultDataDelegate(Dictionary<string, object> resultData);

	protected delegate void uploadCompleteDelegate(GSTypedResponse response, string uploadId);

	protected const float requestTimeOut = 30f;

	public string name = "Undefined Query";

	protected bool debugOutput;

	protected IEnumerator op;

	private bool isDone;

	protected string error = "";

	public Dictionary<string, object> ResultData;

	public UnityAction<GameSparksQuery> FinishListeners;

	public UniqueQueryTag uniqueTag;

	public bool IsRunning => op != null;

	public bool IsDone => isDone;

	public bool HasError => !error.NullOrEmpty();

	public string Error => error;

	public GameSparksQuery(bool debugOutput = false)
	{
		this.debugOutput = debugOutput;
	}

	public void Update()
	{
		bool isRunning = IsRunning;
		if (op != null && !op.MoveNext())
		{
			op = null;
		}
		if (isRunning && isRunning != IsRunning)
		{
			isDone = true;
			if (FinishListeners != null)
			{
				FinishListeners(this);
			}
		}
	}

	public float GetResultDataFloat(string key, float defaultValue)
	{
		if (ResultData == null)
		{
			return defaultValue;
		}
		if (ResultData.TryGetValue(key, out var value))
		{
			return (float)value;
		}
		return defaultValue;
	}

	public bool GetResultDataBool(string key, bool defaultValue)
	{
		if (ResultData == null)
		{
			return defaultValue;
		}
		if (ResultData.TryGetValue(key, out var value))
		{
			return (bool)value;
		}
		return defaultValue;
	}

	public int GetResultDataInt(string key, int defaultValue)
	{
		if (ResultData == null)
		{
			return defaultValue;
		}
		if (ResultData.TryGetValue(key, out var value))
		{
			return (int)value;
		}
		return defaultValue;
	}

	public GSData GetResultDataGSData(string key)
	{
		if (ResultData == null)
		{
			return null;
		}
		if (ResultData.TryGetValue(key, out var value))
		{
			return value as GSData;
		}
		return null;
	}

	public void UploadStringAsFile(string fileContents, string uploadName, bool published, FeaturedQuickFilter.LevelTypes levelType, bool hasMods)
	{
		byte[] compressedBytesFromXmlString = QuickSaver.GetCompressedBytesFromXmlString(fileContents);
		string fileHash = QuickSaver.GetHashForFile(compressedBytesFromXmlString);
		string uploadCode = null;
		uploadURLDelegate setUploadURL = delegate(GetUploadUrlResponse response)
		{
			if (response.ScriptData != null && response.ScriptData.ContainsKey("code"))
			{
				string value = response.ScriptData.GetString("code");
				ResultData = new Dictionary<string, object>();
				ResultData.Add("code", value);
				ResultData.Add("name", uploadName);
				return (string)null;
			}
			return response.Url;
		};
		uploadDataDelegate setUploadData = delegate(Dictionary<string, object> uploadData)
		{
			uploadData.Add("uploadType", "snapshot");
			uploadData.Add("fileHash", fileHash);
			uploadData.Add("name", uploadName);
			uploadData.Add("published", published ? 1 : 0);
			uploadData.Add("levelType", levelType.ToString());
			uploadData.Add("levelVersion", GameSettings.GetInstance().UploadLevelVersion);
			uploadData.Add("hasMods", hasMods ? 1 : 0);
		};
		uploadErrorDelegate setUploadError = delegate(GSTypedResponse response)
		{
			if (!response.JSONData.ContainsKey("scriptData"))
			{
				return "No scriptData in response: " + response.JSONString;
			}
			if (!(response.JSONData["scriptData"] is GSData gSData))
			{
				return "Could not read scriptData: " + response.JSONString;
			}
			uploadCode = gSData.GetString("code");
			return uploadCode.NullOrEmpty() ? ("No upload code found in response: " + response.JSONString) : null;
		};
		resultDataDelegate populateResultData = delegate(Dictionary<string, object> resultData)
		{
			resultData.Add("code", uploadCode);
			resultData.Add("name", uploadName);
		};
		op = DoUploadFileFromBytes(compressedBytesFromXmlString, fileHash + "-" + QuickSaver.SanitizePath(uploadName), "application/octet-stream", setUploadURL, setUploadData, setUploadError, populateResultData);
		name = "Upload snapshot (" + uploadName + ")";
	}

	public void GetXmlStringFromSnapshotCode(string snapshotCode, bool incrementGetCount = true)
	{
		op = DoGetXmlStringFromSnapshotCode(snapshotCode, incrementGetCount);
		name = "Grab snapshot from code (" + GetFormattedSnapshotCode(snapshotCode) + ")";
	}

	public static bool ValidateSnapshotCode(string input)
	{
		if (input.NullOrEmpty())
		{
			return false;
		}
		if (!Regex.IsMatch(input, "[^A-Za-z0-9\\-]"))
		{
			return SanitizeSnapshotCode(input) != null;
		}
		return false;
	}

	public static string SanitizeSnapshotCode(string input)
	{
		if (input.NullOrEmpty())
		{
			return null;
		}
		string text = Regex.Replace(input.ToUpper(), "[^A-Za-z0-9]", "");
		if (text.Length != 8)
		{
			return null;
		}
		return text;
	}

	public static string GetFormattedSnapshotCode(string input)
	{
		if (input.NullOrEmpty())
		{
			return "";
		}
		string text = SanitizeSnapshotCode(input);
		if (text == null)
		{
			return "";
		}
		return text.Substring(0, 4) + "-" + text.Substring(4, 4);
	}

	private IEnumerator DoGetXmlStringFromSnapshotCode(string inputCode, bool incrementGetCount)
	{
		string text = SanitizeSnapshotCode(inputCode);
		if (text != null)
		{
			bool waitingForUploadURL = true;
			string snapshotURL = null;
			string snapshotName = "Unnamed";
			string levelTypeStr = null;
			bool levelIsArchived = false;
			GSData authorInfo = null;
			Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
			{
				waitingForUploadURL = false;
				if (response.HasErrors)
				{
					error = "Error in GetLevelUploadURL response: " + response.JSONString;
				}
				else if (response.ScriptData != null)
				{
					if (response.ScriptData.GetBoolean("archived") ?? false)
					{
						levelIsArchived = true;
						snapshotURL = response.ScriptData.GetString("url");
						snapshotName = response.ScriptData.GetString("name");
						levelTypeStr = null;
					}
					else
					{
						string text3 = response.ScriptData.GetString("url");
						levelTypeStr = response.ScriptData.GetString("levelType");
						snapshotName = response.ScriptData.GetString("name");
						authorInfo = response.ScriptData.GetGSData("authorInfo");
						if (text3 != null)
						{
							snapshotURL = text3;
						}
						else
						{
							error = "Could not parse URL and size from GetLevelUploadURL response: " + response.JSONString;
						}
					}
				}
				else
				{
					error = "Response had no script data";
				}
			};
			sendGetLevelUploadUrlRequest(text, incrementGetCount, responseHandler);
			float waitTime = 30f;
			while (waitingForUploadURL)
			{
				yield return null;
				waitTime -= Time.unscaledDeltaTime;
				if (waitTime <= 0f)
				{
					error = "Timeout while asking for snapshot URL (" + 30f + " s)";
					yield break;
				}
			}
			if (!error.NullOrEmpty())
			{
				yield break;
			}
			if (snapshotURL != null)
			{
				while (!GameSparksManager.Instance.SecureWebRequestLock(this))
				{
					yield return null;
				}
				int num = snapshotURL.IndexOf("uploadedLevels") + 15;
				string text2 = snapshotURL.Substring(0, num);
				string s = snapshotURL.Substring(num);
				s = UnityWebRequest.EscapeURL(s);
				s = s.Replace("+", "%20");
				UnityWebRequest www = UnityWebRequest.Get(text2 + s);
				www.SendWebRequest();
				waitTime = 30f;
				while (!www.isDone)
				{
					yield return null;
					waitTime -= Time.unscaledDeltaTime;
					if (waitTime <= 0f)
					{
						error = "Timeout while trying to download snapshot (" + 30f + " s)";
						GameSparksManager.Instance.ReleaseWebRequestLock(this);
						yield break;
					}
				}
				GameSparksManager.Instance.ReleaseWebRequestLock(this);
				if (www.error.NullOrEmpty())
				{
					if (debugOutput)
					{
						Debug.Log("[GS query] Successfully grabbed snapshot data:\n" + www.downloadHandler.text);
					}
					StatTracker.Instance.GetSaveFileDataForMainUser()?.AddRecentSnapshotCode(SaveFileData.RecentSnapshotEntry.SnapshotType.Downloaded, GetFormattedSnapshotCode(inputCode), snapshotName);
					ResultData = new Dictionary<string, object>();
					ResultData.Add("bytes", www.downloadHandler.data);
					ResultData.Add("name", snapshotName);
					ResultData.Add("archived", levelIsArchived);
					ResultData.Add("authorInfo", authorInfo);
					FeaturedQuickFilter.LevelTypes levelTypes = FeaturedQuickFilter.LevelTypes.Any;
					try
					{
						levelTypes = (FeaturedQuickFilter.LevelTypes)Enum.Parse(typeof(FeaturedQuickFilter.LevelTypes), levelTypeStr);
					}
					catch (Exception)
					{
					}
					ResultData.Add("levelType", levelTypes);
				}
				else
				{
					error = "Error downloading snapshot from URL: " + www.error;
				}
			}
			else
			{
				error = "No Snapshot URL was supplied.";
			}
		}
		else
		{
			error = "Snapshot code input is invalid: " + inputCode;
		}
	}

	protected abstract void sendGetLevelUploadUrlRequest(string snapshotCode, bool incrementGetCount, Action<LogEventResponse> responseHandler);

	public void NotifySnapshotPlayed(string inputCode)
	{
		op = DoNotifySnapshotPlayed(inputCode);
		name = "Notify snapshot played (" + GetFormattedSnapshotCode(inputCode) + ")";
	}

	private IEnumerator DoNotifySnapshotPlayed(string inputCode)
	{
		string snapshotCode = SanitizeSnapshotCode(inputCode);
		if (snapshotCode != null)
		{
			bool responseReceived = false;
			Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
			{
				responseReceived = true;
				if (response.HasErrors)
				{
					error = "Error in NotifySnapshotPlayed response: " + response.JSONString;
				}
				else if (debugOutput)
				{
					Debug.Log("[GS query] NotifySnapshotPlayed for code " + GetFormattedSnapshotCode(snapshotCode) + " acknowledged successfully.");
				}
			};
			sendNotifySnapshotPlayedRequest(snapshotCode, responseHandler);
			while (!responseReceived)
			{
				yield return null;
			}
		}
		else
		{
			error = "Snapshot code input is invalid: " + inputCode;
		}
	}

	protected abstract void sendNotifySnapshotPlayedRequest(string snapshotCode, Action<LogEventResponse> responseHandler);

	public void ForcePurge()
	{
		op = null;
		error = "The query \"" + name + "\" was force-purged by the Query Manager.";
		ResultData = null;
		isDone = true;
		if (FinishListeners != null)
		{
			FinishListeners(this);
		}
	}

	public void UploadLevelThumbnail(string code, byte[] bytes)
	{
		uploadDataDelegate setUploadData = delegate(Dictionary<string, object> uploadData)
		{
			uploadData.Add("uploadType", "thumbnail");
			uploadData.Add("code", code);
		};
		op = DoUploadFileFromBytes(bytes, code + "-screenshot" + QuickSaver.PreferredThumbnailFormatExtension, QuickSaver.PreferredThumbnailFormatMimeType, null, setUploadData, null, null);
		name = "Upload Level Thumbnail (" + code + ")";
	}

	protected virtual IEnumerator DoUploadFileFromBytes(byte[] bytes, string filename, string contentType, uploadURLDelegate setUploadURL, uploadDataDelegate setUploadData, uploadErrorDelegate setUploadError, resultDataDelegate populateResultData)
	{
		string uploadURL = null;
		bool waitingForUploadURL = true;
		Action<GetUploadUrlResponse> responseHandler = delegate(GetUploadUrlResponse response)
		{
			if (!response.HasErrors)
			{
				if (debugOutput)
				{
					Debug.Log("[GS query] Received Upload URL: " + response.Url);
				}
				if (setUploadURL != null)
				{
					uploadURL = setUploadURL(response);
				}
				else
				{
					uploadURL = response.Url;
				}
			}
			else
			{
				error = "Error while requesting Upload URL: " + response.JSONString;
			}
			waitingForUploadURL = false;
		};
		Dictionary<string, object> uploadData = new Dictionary<string, object>();
		setUploadData?.Invoke(uploadData);
		sendGetUploadUrlRequest(uploadData, responseHandler);
		float waitTime = 30f;
		while (waitingForUploadURL)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while requesting Upload URL (" + 30f + " s)";
				yield break;
			}
		}
		if (uploadURL == null)
		{
			yield break;
		}
		string uploadID = null;
		string uploadJSONString = null;
		bool waitingForUploadID = true;
		Action<GSTypedResponse> uploadCompletedResponse = delegate(GSTypedResponse response)
		{
			if (!response.HasErrors)
			{
				string uploadIdFromResponse = getUploadIdFromResponse(response);
				if (debugOutput)
				{
					Debug.Log("[GS query] Received upload complete message: " + uploadIdFromResponse);
				}
				waitingForUploadID = false;
				if (setUploadError != null)
				{
					error = setUploadError(response);
				}
				if (error.NullOrEmpty())
				{
					uploadID = uploadIdFromResponse;
					uploadJSONString = response.JSONString;
				}
			}
			else
			{
				error = "Error in upload complete message: " + response.JSONString;
			}
		};
		startWaitingForUpload(uploadCompletedResponse);
		while (!GameSparksManager.Instance.SecureWebRequestLock(this))
		{
			yield return null;
		}
		yield return doFileUpload(bytes, filename, contentType, uploadURL);
		GameSparksManager.Instance.ReleaseWebRequestLock(this);
		if (!error.NullOrEmpty())
		{
			error = "Error uploading file: " + error;
			yield break;
		}
		if (debugOutput)
		{
			Debug.Log("[GS query] Upload successful");
		}
		waitTime = 30f;
		while (waitingForUploadID)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for Upload Complete Message (" + 30f + " s)";
				yield break;
			}
		}
		if (uploadID != null)
		{
			if (debugOutput)
			{
				Debug.Log("[GS query] Got upload ID successfully: " + uploadJSONString);
			}
			ResultData = new Dictionary<string, object>();
			ResultData.Add("uploadID", uploadID);
			populateResultData?.Invoke(ResultData);
		}
		else
		{
			error = "Error with upload ID: " + error;
		}
		doneWaitingForUpload(uploadCompletedResponse);
	}

	protected abstract IEnumerator doFileUpload(byte[] bytes, string filename, string contentType, string uploadURL);

	protected abstract string getUploadIdFromResponse(GSTypedResponse response);

	protected abstract void startWaitingForUpload(Action<GSTypedResponse> onComplete);

	protected abstract void doneWaitingForUpload(Action<GSTypedResponse> onComplete);

	protected abstract void sendGetUploadUrlRequest(Dictionary<string, object> uploadData, Action<GetUploadUrlResponse> responseHandler);

	public void GetThumbnailForSnapshotCode(string snapshotCode)
	{
		op = DoGetThumbnailForSnapshotCode(snapshotCode);
		name = "Grab thumbnail from code (" + GetFormattedSnapshotCode(snapshotCode) + ")";
	}

	private IEnumerator DoGetThumbnailForSnapshotCode(string inputCode)
	{
		string text = SanitizeSnapshotCode(inputCode);
		if (text != null)
		{
			bool waitingForUploadURL = true;
			string thumbnailURL = null;
			Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
			{
				waitingForUploadURL = false;
				if (response.HasErrors)
				{
					error = "Error in GetLevelThumbnailUrl response: " + response.JSONString;
				}
				else if (response.ScriptData != null)
				{
					string text3 = response.ScriptData.GetString("error");
					if (!text3.NullOrEmpty())
					{
						error = "GetLevelThumbnailUrl returned this error : " + text3;
					}
					else if (response.ScriptData.GetBoolean("archived") ?? false)
					{
						error = "This level is archived -- no thumbnail!";
					}
					else
					{
						string text4 = response.ScriptData.GetString("url");
						if (text4 != null)
						{
							thumbnailURL = text4;
						}
						else
						{
							error = "Could not parse URL and size from GetLevelThumbnailUrl response: " + response.JSONString;
						}
					}
				}
			};
			sendGetLevelThumbnailUrlRequest(text, responseHandler);
			float waitTime = 30f;
			while (waitingForUploadURL)
			{
				yield return null;
				waitTime -= Time.unscaledDeltaTime;
				if (waitTime <= 0f)
				{
					error = "Timeout while asking for thumbnail URL (" + 30f + " s)";
					yield break;
				}
			}
			if (!error.NullOrEmpty())
			{
				yield break;
			}
			if (thumbnailURL != null)
			{
				while (!GameSparksManager.Instance.SecureWebRequestLock(this))
				{
					yield return null;
				}
				int num = thumbnailURL.IndexOf("thumbnails") + 15;
				string text2 = thumbnailURL.Substring(0, num);
				string s = thumbnailURL.Substring(num);
				s = UnityWebRequest.EscapeURL(s);
				UnityWebRequest www = UnityWebRequest.Get(text2 + s);
				www.SendWebRequest();
				waitTime = 30f;
				while (!www.isDone)
				{
					yield return null;
					waitTime -= Time.unscaledDeltaTime;
					if (waitTime <= 0f)
					{
						error = "Timeout while trying to download thumbnail (" + 30f + " s)";
					}
					else if (!string.IsNullOrEmpty(www.error))
					{
						error = www.error;
					}
					if (!string.IsNullOrEmpty(error))
					{
						GameSparksManager.Instance.ReleaseWebRequestLock(this);
						www.Dispose();
						yield break;
					}
				}
				GameSparksManager.Instance.ReleaseWebRequestLock(this);
				if (www.error.NullOrEmpty())
				{
					if (debugOutput)
					{
						Debug.Log("[GS query] Successfully grabbed thumbnail data");
					}
					ResultData = new Dictionary<string, object>();
					ResultData.Add("bytes", www.downloadHandler.data);
					www.Dispose();
				}
				else
				{
					error = "Error downloading thumbnail from URL: " + www.error;
				}
			}
			else
			{
				error = "No thumbnail URL was supplied.";
			}
		}
		else
		{
			error = "Snapshot code input is invalid: " + inputCode;
		}
	}

	protected abstract void sendGetLevelThumbnailUrlRequest(string snapshotCode, Action<LogEventResponse> responseHandler);

	public void GetFeaturedLevelList(int fromIndex, int numDisplay, FeaturedQuickFilter.SortingFilter sortingFilter)
	{
		op = DoGetFeaturedLevelList(fromIndex, numDisplay, sortingFilter);
		name = "Get Featured Level List (" + numDisplay + " starting from #" + fromIndex + ", filter: " + sortingFilter.filterType;
		if (sortingFilter.filterType == FeaturedQuickFilter.FilterTypes.Sorted)
		{
			name = name + " by " + sortingFilter.sortBy + (sortingFilter.descending ? " Desc." : " Asc.");
		}
		name += ")";
	}

	private IEnumerator DoGetFeaturedLevelList(int fromIndex, int numDisplay, FeaturedQuickFilter.SortingFilter sortingFilter)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetLevelList response: " + response.JSONString;
			}
			else
			{
				ResultData = new Dictionary<string, object>();
				int num = response.ScriptData.GetInt("totalEntries") ?? 0;
				ResultData.Add("totalEntries", num);
				int num2 = response.ScriptData.GetInt("returnedEntries") ?? 0;
				ResultData.Add("returnedEntries", num2);
				int num3 = response.ScriptData.GetInt("firstEntryIndex") ?? 0;
				ResultData.Add("firstEntryIndex", num3);
				long num4 = response.ScriptData.GetLong("date") ?? 0;
				ResultData.Add("date", num4);
				List<GSData> gSDataList = response.ScriptData.GetGSDataList("records");
				List<string> list = null;
				if (response.ScriptData.ContainsKey("orderedCodeList"))
				{
					list = response.ScriptData.GetStringList("orderedCodeList");
					if (list.Count == 0)
					{
						list = null;
					}
				}
				List<UndergroundComputer.FeaturedLevelData> list2 = new List<UndergroundComputer.FeaturedLevelData>();
				Dictionary<string, UndergroundComputer.FeaturedLevelData> dictionary = new Dictionary<string, UndergroundComputer.FeaturedLevelData>();
				if (list == null && sortingFilter.codeList != null && sortingFilter.codeList.Count > 0)
				{
					list = sortingFilter.codeList;
				}
				foreach (GSData item in gSDataList)
				{
					UndergroundComputer.FeaturedLevelData featuredLevelData = new UndergroundComputer.FeaturedLevelData(item);
					if (list != null)
					{
						dictionary.Add(featuredLevelData.code, featuredLevelData);
					}
					else
					{
						list2.Add(featuredLevelData);
					}
				}
				if (list != null)
				{
					foreach (string item2 in list)
					{
						if (dictionary.TryGetValue(item2, out var value))
						{
							list2.Add(value);
						}
						else
						{
							Debug.Log("GetFeaturedLevelList: No result returned for code: " + item2);
						}
					}
				}
				ResultData.Add("records", list2);
			}
		};
		GSRequestData gSRequestData = new GSRequestData();
		gSRequestData.AddStringList("list", sortingFilter.codeList);
		sendGetLevelListRequest(sortingFilter, fromIndex, numDisplay, gSRequestData, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while grabbing level list (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetLevelListRequest(FeaturedQuickFilter.SortingFilter sortingFilter, int fromIndex, int numDisplay, GSRequestData codeListObject, Action<LogEventResponse> responseHandler);

	public void SetLevelPublishStatus(string code, bool published, FeaturedQuickFilter.LevelTypes levelType)
	{
		SendSimpleRequest("setLevelPublishStatus", new Dictionary<string, object>
		{
			{ "code", code },
			{
				"published",
				published ? 1 : 0
			},
			{
				"category",
				levelType.ToString()
			}
		}, returnScriptData: false);
	}

	public void GetLevelPublishStatus(string code)
	{
		op = DoGetLevelPublishStatus(code);
		name = "Get Level Publish Status (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoGetLevelPublishStatus(string code)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetLevelPublishStatus response: " + response.JSONString;
			}
			else
			{
				ResultData = new Dictionary<string, object>();
				ResultData.Add("published", response.ScriptData.GetInt("published") ?? 0);
				ResultData.Add("isOwner", response.ScriptData.GetInt("isOwner") ?? 0);
				ResultData.Add("isAnonymous", response.ScriptData.GetInt("isAnonymous") ?? 0);
				ResultData.Add("category", response.ScriptData.GetString("category"));
			}
		};
		sendGetLevelPublishStatusRequest(text, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while getting level publish status (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetLevelPublishStatusRequest(string snapshotCode, Action<LogEventResponse> responseHandler);

	public void CastLevelVote(string code, int vote)
	{
		op = DoCastLevelVote(code, vote);
		string text = "";
		switch (vote)
		{
		case -1:
			text = "Downvote";
			break;
		case 0:
			text = "Cancel vote for";
			break;
		case 1:
			text = "Upvote";
			break;
		}
		name = text + " level (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoCastLevelVote(string code, int vote)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in CastLevelVote response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				string text2 = response.ScriptData.GetString("error");
				if (text2.NullOrEmpty())
				{
					ResultData = new Dictionary<string, object>();
					ResultData.Add("myVote", response.ScriptData.GetInt("myVote") ?? 0);
					ResultData.Add("newRating", response.ScriptData.GetInt("newRating") ?? 1);
				}
				else
				{
					error = "Response had the following error: " + text2;
				}
			}
			else
			{
				error = "Response had no script data";
			}
		};
		sendCastLevelVoteRequest(text, vote, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while casting level vote (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendCastLevelVoteRequest(string snapshotCode, int vote, Action<LogEventResponse> responseHandler);

	public void GetMyLevelReport(string code)
	{
		op = DoGetMyLevelReport(code);
		name = "Get my level report (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoGetMyLevelReport(string code)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetMyLevelReport response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				string text2 = response.ScriptData.GetString("error");
				if (text2.NullOrEmpty())
				{
					ResultData = new Dictionary<string, object>();
					ResultData.Add("reportReason", response.ScriptData.GetInt("reportReason") ?? 0);
					ResultData.Add("reportComment", response.ScriptData.GetString("reportComment"));
				}
				else
				{
					error = "Response had the following error: " + text2;
				}
			}
		};
		sendGetMyLevelReportRequest(text, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while retrieving my level report (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetMyLevelReportRequest(string snapshotCode, Action<LogEventResponse> responseHandler);

	public void SubmitLevelReport(string code, int reportReason, string reportComment, bool delete)
	{
		SendSimpleRequest("submitLevelReport", new Dictionary<string, object>
		{
			{ "code", code },
			{ "reason", reportReason },
			{ "comment", reportComment },
			{
				"deleteReport",
				delete ? 1 : 0
			}
		}, returnScriptData: false);
	}

	public void SubmitChallengeTime(string code, List<string> playerIds, float time, bool allCoins, bool noUpdate)
	{
		op = DoSubmitChallengeTime(code, playerIds, time, allCoins, noUpdate);
		name = "Submit level time (" + GetFormattedSnapshotCode(code) + ", " + playerIds.Count + " players, " + time + " s)";
	}

	private IEnumerator DoSubmitChallengeTime(string code, List<string> playerIds, float time, bool allCoins, bool noUpdate)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in SubmitChallengeTime response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				if (!response.ScriptData.ContainsKey("error"))
				{
					try
					{
						ResultData = new Dictionary<string, object>();
						List<ChallengeScoreboard.ChallengeTimeData> value = ChallengeScoreboard.ChallengeTimeData.CreateListFromGSDataRecordList(response.ScriptData.GetGSDataList("recordsNoCoins"));
						ResultData.Add("recordsNoCoins", value);
						List<ChallengeScoreboard.ChallengeTimeData> value2 = ChallengeScoreboard.ChallengeTimeData.CreateListFromGSDataRecordList(response.ScriptData.GetGSDataList("recordsAllCoins"));
						ResultData.Add("recordsAllCoins", value2);
						ResultData.Add("newBestNoCoins", response.ScriptData.GetBoolean("newBestNoCoins") ?? false);
						ResultData.Add("newBestAllCoins", response.ScriptData.GetBoolean("newBestAllCoins") ?? false);
						ResultData.Add("bestNoCoins", response.ScriptData.GetFloat("bestNoCoins") ?? 0f);
						ResultData.Add("bestAllCoins", response.ScriptData.GetFloat("bestAllCoins") ?? 0f);
						return;
					}
					catch (Exception ex)
					{
						error = "Exception while populating ResultData: " + ex.Message + "\n" + ex.StackTrace;
						return;
					}
				}
				error = "Error submitting times: " + response.ScriptData.GetString("error");
			}
			else
			{
				error = "SubmitChallengeTime had no scriptData";
			}
		};
		sendSubmitChallengeTimeRequest(text, playerIds, time.ToString(), allCoins, noUpdate, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while submitting challenge time (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendSubmitChallengeTimeRequest(string snapshotCode, List<string> playerIds, string time, bool allCoins, bool noUpdate, Action<LogEventResponse> responseHandler);

	public void GetMyLevelRating(string code)
	{
		op = DoGetMyLevelRating(code);
		name = "Get my level rating (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoGetMyLevelRating(string code)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetMyLevelRating response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				string text2 = response.ScriptData.GetString("error");
				if (text2.NullOrEmpty())
				{
					ResultData = new Dictionary<string, object>();
					ResultData.Add("levelRating", response.ScriptData.GetInt("levelRating") ?? 0);
					ResultData.Add("myVote", response.ScriptData.GetInt("myVote") ?? 0);
				}
				else
				{
					error = "Response had the following error: " + text2;
				}
			}
		};
		sendGetMyLevelRatingRequest(text, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while retrieving my level rating (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetMyLevelRatingRequest(string snapshotCode, Action<LogEventResponse> responseHandler);

	public void GetChallengeTimes(string code, int numPlayers, int startIndex, int maxRecords)
	{
		op = DoGetChallengeTimes(code, numPlayers, startIndex, maxRecords);
		name = "Get challenge times (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoGetChallengeTimes(string code, int numPlayers, int startIndex, int maxRecords)
	{
		string text = SanitizeSnapshotCode(code);
		if (text.NullOrEmpty())
		{
			error = "Supplied snapshot code seems invalid (" + code + ")";
			yield break;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetChallengeTimes response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				string text2 = response.ScriptData.GetString("error");
				if (text2.NullOrEmpty())
				{
					ResultData = new Dictionary<string, object>();
					List<ChallengeScoreboard.ChallengeTimeData> value = ChallengeScoreboard.ChallengeTimeData.CreateListFromGSDataRecordList(response.ScriptData.GetGSDataList("recordsNoCoins"));
					ResultData.Add("recordsNoCoins", value);
					List<ChallengeScoreboard.ChallengeTimeData> value2 = ChallengeScoreboard.ChallengeTimeData.CreateListFromGSDataRecordList(response.ScriptData.GetGSDataList("recordsAllCoins"));
					ResultData.Add("recordsAllCoins", value2);
					GSData gSData = response.ScriptData.GetGSData("personalRecordNoCoins");
					if (gSData != null)
					{
						ChallengeScoreboard.ChallengeTimeData value3 = ChallengeScoreboard.ChallengeTimeData.CreateFromGSDataRecord(gSData);
						ResultData.Add("personalRecordNoCoins", value3);
					}
					else
					{
						ResultData.Add("personalRecordNoCoins", null);
					}
					GSData gSData2 = response.ScriptData.GetGSData("personalRecordAllCoins");
					if (gSData2 != null)
					{
						ChallengeScoreboard.ChallengeTimeData value4 = ChallengeScoreboard.ChallengeTimeData.CreateFromGSDataRecord(gSData2);
						ResultData.Add("personalRecordAllCoins", value4);
					}
					else
					{
						ResultData.Add("personalRecordAllCoins", null);
					}
					List<string> stringList = response.ScriptData.GetStringList("firstClearPlayerNames");
					ResultData.Add("firstClearNameList", stringList);
					List<string> stringList2 = response.ScriptData.GetStringList("firstClearPlayerIds");
					ResultData.Add("firstClearIdList", stringList2);
					List<GSData> gSDataList = response.ScriptData.GetGSDataList("firstClearPlayerPlatformIds");
					ResultData.Add("firstClearPlatformIdList", gSDataList);
				}
				else
				{
					error = "Response had the following error: " + text2;
				}
			}
		};
		sendGetChallengeTimesRequest(text, numPlayers, startIndex, maxRecords, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while retrieving challenge times (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetChallengeTimesRequest(string snapshotCode, int numPlayers, int startIndex, int maxRecords, Action<LogEventResponse> responseHandler);

	public void AddChallengeAttempt(string code, List<string> playerIds, bool successful, int secondsInLevel)
	{
		SendSimpleRequest("addAttempt", new Dictionary<string, object>
		{
			{ "code", code },
			{ "playerIds", playerIds },
			{
				"successful",
				successful ? 1 : 0
			},
			{ "secondsInLevel", secondsInLevel }
		}, returnScriptData: false);
	}

	public void GetLevelInfo(List<string> codeList)
	{
		op = DoGetLevelInfo(codeList);
		name = "Get level info (" + codeList.Count + " codes)";
	}

	private IEnumerator DoGetLevelInfo(List<string> codeList)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetLevelInfo response: " + response.JSONString;
			}
			else
			{
				ResultData = new Dictionary<string, object>();
				long num = response.ScriptData.GetLong("date") ?? 0;
				ResultData.Add("date", num);
				List<GSData> gSDataList = response.ScriptData.GetGSDataList("records");
				List<UndergroundComputer.FeaturedLevelData> list = new List<UndergroundComputer.FeaturedLevelData>();
				Dictionary<string, UndergroundComputer.FeaturedLevelData> dictionary = new Dictionary<string, UndergroundComputer.FeaturedLevelData>();
				foreach (GSData item in gSDataList)
				{
					UndergroundComputer.FeaturedLevelData featuredLevelData = new UndergroundComputer.FeaturedLevelData(item);
					dictionary.Add(featuredLevelData.code, featuredLevelData);
				}
				foreach (string code in codeList)
				{
					if (dictionary.TryGetValue(code, out var value))
					{
						list.Add(value);
					}
					else
					{
						Debug.Log("GetLevelInfo: No result returned for code: " + code);
					}
				}
				ResultData.Add("records", list);
			}
		};
		GSRequestData gSRequestData = new GSRequestData();
		gSRequestData.AddStringList("list", codeList);
		sendGetLevelInfoRequest(gSRequestData, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while grabbing level info (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetLevelInfoRequest(GSRequestData codeListObject, Action<LogEventResponse> responseHandler);

	public void SetLevelApprovalStatus(string code, int approvalStatus)
	{
		SendSimpleRequest("adminSetLevelApprovalStatus", new Dictionary<string, object>
		{
			{ "code", code },
			{ "approvalStatus", approvalStatus }
		}, returnScriptData: false);
	}

	public void GetLevelReports(string code)
	{
		op = DoGetLevelReports(code);
		name = "Get Level Reports (" + GetFormattedSnapshotCode(code) + ")";
	}

	private IEnumerator DoGetLevelReports(string code)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in GetLevelReports response: " + response.JSONString;
			}
			else if (response.ScriptData != null)
			{
				ResultData = new Dictionary<string, object>();
				long num = response.ScriptData.GetLong("date") ?? 0;
				ResultData.Add("date", num);
				List<GSData> gSDataList = response.ScriptData.GetGSDataList("records");
				if (gSDataList != null)
				{
					List<ViewReportsDialog.ReportData> list = new List<ViewReportsDialog.ReportData>();
					foreach (GSData item in gSDataList)
					{
						list.Add(new ViewReportsDialog.ReportData
						{
							reportReason = (item.GetInt("reason") ?? 0),
							reportComment = item.GetString("comment"),
							timestamp = (item.GetLong("date") ?? 0)
						});
					}
					ResultData.Add("reports", list);
				}
				else
				{
					error = "Could not grab records from GS data list.";
				}
			}
			else
			{
				error = "No scriptdata in response.";
			}
		};
		sendAdminGetLevelReportsRequest(code, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while grabbing level reports (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendAdminGetLevelReportsRequest(string code, Action<LogEventResponse> responseHandler);

	public void SendSimpleRequest(string eventKey, Dictionary<string, object> eventData, bool returnScriptData)
	{
		op = DoSendSimpleRequest(eventKey, eventData, returnScriptData);
		name = "Simple request (" + eventKey + ")";
	}

	private IEnumerator DoSendSimpleRequest(string eventKey, Dictionary<string, object> eventData, bool returnScriptData)
	{
		if (eventData.ContainsKey("code"))
		{
			string text = SanitizeSnapshotCode(eventData["code"] as string);
			if (text == null)
			{
				error = "Invalid code was supplied";
				yield break;
			}
			eventData["code"] = text;
		}
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				error = "Error in response: " + response.JSONString;
			}
			else if (returnScriptData)
			{
				if (response.ScriptData != null)
				{
					if (response.ScriptData.ContainsKey("error"))
					{
						error = response.ScriptData.GetString("error");
					}
					else
					{
						ResultData = new Dictionary<string, object>();
						ResultData.Add("scriptData", response.ScriptData);
					}
				}
				else
				{
					error = "No scriptdata in response.";
				}
			}
		};
		sendSimpleRequest(eventKey, eventData, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendSimpleRequest(string eventKey, Dictionary<string, object> eventData, Action<LogEventResponse> responseHandler);

	public void GetLobbyList(string version, int regionFilterIndex, LobbyPlayer.SocialPlatform platform, bool disallowCrossplay, Action<LogEventResponse> callback)
	{
		op = DoGetLobbyList(version, regionFilterIndex, platform, disallowCrossplay, callback);
		name = "Get Lobby List (" + regionFilterIndex + ")";
	}

	private IEnumerator DoGetLobbyList(string version, int regionFilterIndex, LobbyPlayer.SocialPlatform platform, bool disallowCrossplay, Action<LogEventResponse> callback)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			callback(response);
		};
		GSRequestData gSRequestData = new GSRequestData();
		if (regionFilterIndex != -1)
		{
			AvailableRegion availableRegion = RelayConstants.AVAILABLE_REGIONS[regionFilterIndex];
			gSRequestData.AddString(MatchmakingLobby.data_unityLobbyRegion, availableRegion.id);
		}
		gSRequestData.AddString(MatchmakingLobby.data_lobbyPlatform, GamesparksMatchmaker.GetLobbyPlatformString(platform));
		gSRequestData.AddString("useNewCrossplayRestrictions", "1");
		gSRequestData.AddString(MatchmakingLobby.data_disallowCrossplay, disallowCrossplay ? "1" : "0");
		sendGetLobbyListRequest(version, gSRequestData, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetLobbyListRequest(string version, GSRequestData filters, Action<LogEventResponse> responseHandler);

	public void GetLobbyData(string matchID, bool useCode, Action<LogEventResponse> callback, bool reserveSlot = false)
	{
		op = DoGetLobbyData(matchID, useCode, callback, reserveSlot);
		if (useCode)
		{
			name = "Get Lobby Data (Lobby Code: " + matchID + ")";
		}
		else
		{
			name = "Get Lobby Data (Match ID: " + matchID + ")";
		}
	}

	private IEnumerator DoGetLobbyData(string matchID, bool useCode, Action<LogEventResponse> callback, bool reserveSlot = false)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			callback(response);
		};
		sendGetLobbyDataRequest(matchID, useCode, reserveSlot, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendGetLobbyDataRequest(string matchID, bool useCode, bool reserveSlot, Action<LogEventResponse> responseHandler);

	public void CreateMatch(Action<LogEventResponse> callback)
	{
		op = DoSendRequest(sendCreateMatchEvent, callback);
		name = "Create Match";
	}

	public void GetFrozenLobbyCode(Action<LogEventResponse> callback)
	{
		op = DoSendRequest(sendGetLobbyCodeEvent, callback);
		name = "Get Frozen Lobby Code";
	}

	public void FreezeLobbyCode(Action<LogEventResponse> callback, string lobbyCode)
	{
		op = DoSendFreezeLobbyRequest(callback, lobbyCode);
		name = "Freeze Lobby Code";
	}

	public void UnfreezeLobbyCode(Action<LogEventResponse> callback)
	{
		op = DoSendRequest(sendUnfreezeLobbyEvent, callback);
		name = "Unfreeze Lobby Code";
	}

	private IEnumerator DoSendRequest(Action<Action<LogEventResponse>> request, Action<LogEventResponse> callback)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> obj = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			callback(response);
		};
		request(obj);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	private IEnumerator DoSendFreezeLobbyRequest(Action<LogEventResponse> callback, string lobbyCode)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			callback(response);
		};
		sendFreezeLobbyEvent(responseHandler, lobbyCode);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendCreateMatchEvent(Action<LogEventResponse> responseHandler);

	protected abstract void sendFreezeLobbyEvent(Action<LogEventResponse> responseHandler, string lobbyCode);

	protected abstract void sendUnfreezeLobbyEvent(Action<LogEventResponse> responseHandler);

	protected abstract void sendGetLobbyCodeEvent(Action<LogEventResponse> responseHandler);

	public void SetLobbyData(string matchID, GSRequestData matchData, Action<LogEventResponse> callback)
	{
		GameSparksManager.Instance.InvalidateExistingQueries(UniqueQueryTag.SetLobbyData);
		op = DoSetLobbyData(matchID, matchData, callback);
		name = "Set Lobby Data for lobby " + matchID;
		uniqueTag = UniqueQueryTag.SetLobbyData;
	}

	private IEnumerator DoSetLobbyData(string matchID, GSRequestData matchData, Action<LogEventResponse> callback)
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			callback(response);
		};
		sendSetLobbyDataRequest(matchID, matchData, responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendSetLobbyDataRequest(string matchID, GSRequestData matchData, Action<LogEventResponse> responseHandler);

	public void SetLobbyHeartbeat()
	{
		GameSparksManager.Instance.InvalidateExistingQueries(UniqueQueryTag.SetHeartbeat);
		op = DoSetLobbyHeartbeat();
		name = "Set Lobby Heartbeat";
		uniqueTag = UniqueQueryTag.SetHeartbeat;
	}

	private IEnumerator DoSetLobbyHeartbeat()
	{
		bool waitingForResponse = true;
		Action<LogEventResponse> responseHandler = delegate(LogEventResponse response)
		{
			waitingForResponse = false;
			if (response.HasErrors)
			{
				Debug.LogError("Problem setting lobby heartbeat: " + response.Errors.JSON);
			}
		};
		sendSetLobbyHeartbeatRequest(responseHandler);
		float waitTime = 30f;
		while (waitingForResponse)
		{
			yield return null;
			waitTime -= Time.unscaledDeltaTime;
			if (waitTime <= 0f)
			{
				error = "Timeout while waiting for response (" + 30f + " s)";
				break;
			}
		}
	}

	protected abstract void sendSetLobbyHeartbeatRequest(Action<LogEventResponse> responseHandler);

	public void WakeUp()
	{
		GameSparksManager.Instance.InvalidateExistingQueries(UniqueQueryTag.WakeUp);
		op = NoOp();
		name = "Wake Up GameSparks";
		uniqueTag = UniqueQueryTag.WakeUp;
	}

	private IEnumerator NoOp()
	{
		yield break;
	}

	public void SubmitUserReport(UserReports.ReportInformation reportInformation)
	{
		SendSimpleRequest("SubmitUserReport", new Dictionary<string, object>
		{
			{ "reporterID", reportInformation.reporterGSID },
			{ "reportedID", reportInformation.reportedGSID },
			{
				"reportReason",
				(int)reportInformation.reportReason
			},
			{ "reportComments", reportInformation.reportComments },
			{
				"reportChatLog",
				(reportInformation.reportChatlog != null) ? reportInformation.reportChatlog : ""
			},
			{
				"reportLevelCode",
				(reportInformation.reportLevelCode != null) ? reportInformation.reportLevelCode : ""
			}
		}, returnScriptData: false);
		name = "Submit report for user " + reportInformation.reportedUsername;
	}

	public static int ParseValueToInt(GSData data, string key, int defaultReturnValue = 0)
	{
		if (data.BaseData.TryGetValue(key, out var value))
		{
			try
			{
				return Convert.ToInt32(value);
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not cast value with key " + key + " (" + value.ToString() + ") to an integer: " + ex.Message + "\n" + ex.StackTrace);
			}
		}
		return defaultReturnValue;
	}
}
