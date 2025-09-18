using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdminBatchManagementDialog : MonoBehaviour
{
	public class BatchListResult
	{
		public string batchID;

		public string batchName;

		public long publishAfter;
	}

	public AdminPanelDialog adminPanelDialog;

	public UnityEngine.Object featuredBatchEntryPrefab;

	public UnityEngine.Object publishLinePrefab;

	public GameObject publishLine;

	public Transform featuredBatchCodeListArea;

	public Transform featuredBatchListContainer;

	public Transform featuredBatchCodeContainer;

	public Text featuredBatchNameButtonText;

	public Text featuredBatchPublishButtonText;

	public GenericButton featuredCodeRemoveButton;

	public GenericButton featuredCodeMoveUpButton;

	public GenericButton featuredCodeMoveDownButton;

	[HideInInspector]
	public FeaturedBatchEntry currentBatchEntry;

	[HideInInspector]
	public FeaturedBatchEntry currentBatchCode;

	public Image loadingSpinner;

	public ScrollArrowController batchListScrollController;

	public ScrollArrowController codeListScrollController;

	private bool updatingFeed;

	public void Initialize()
	{
		UpdateFeaturedCodeListButtonBar();
		RefreshBatchList(delegate
		{
			if (featuredBatchListContainer.childCount > 0)
			{
				for (int i = 0; i < featuredBatchListContainer.childCount; i++)
				{
					FeaturedBatchEntry component = featuredBatchListContainer.GetChild(i).GetComponent<FeaturedBatchEntry>();
					if (component != null)
					{
						OnClickBatch(component)(null);
						break;
					}
				}
			}
			else
			{
				UpdateFeaturedCodeListButtonBar();
			}
		});
	}

	public UnityAction<PickCursor> OnClickBatch(FeaturedBatchEntry entry)
	{
		return delegate
		{
			if (!(currentBatchEntry == entry))
			{
				if (currentBatchEntry != null)
				{
					currentBatchEntry.OnDeselect();
				}
				currentBatchEntry = entry;
				if (currentBatchEntry != null)
				{
					PickableButton.maskAll = true;
					loadingSpinner.enabled = true;
					currentBatchEntry.OnSelect();
					UpdateBatchContents(delegate
					{
						PickableButton.ResetMasks();
						loadingSpinner.enabled = false;
					});
				}
				else
				{
					ClearBatchContents();
				}
			}
		};
	}

	public UnityAction<PickCursor> OnClickCode(FeaturedBatchEntry entry)
	{
		return delegate
		{
			if (currentBatchCode != null)
			{
				currentBatchCode.OnDeselect();
			}
			currentBatchCode = entry;
			if (currentBatchCode != null)
			{
				currentBatchCode.OnSelect();
			}
			UpdateFeaturedCodeListButtonBar();
		};
	}

	public void UpdateBatchContents(UnityAction onFinish)
	{
		ClearBatchContents();
		if (currentBatchEntry.batchID != "NULL" && !currentBatchEntry.dirty)
		{
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SendSimpleRequest("adminGetBatchContents", new Dictionary<string, object> { { "batchID", currentBatchEntry.batchID } }, returnScriptData: true);
			GameSparksQuery gameSparksQuery = query;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
			{
				try
				{
					if (query.HasError)
					{
						throw new Exception("ERROR: " + query.Error);
					}
					GSData resultDataGSData = query.GetResultDataGSData("scriptData");
					if (resultDataGSData == null)
					{
						throw new Exception("ERROR: No Script Data");
					}
					GSData obj = resultDataGSData.GetGSData("batch") ?? throw new Exception("ERROR: No batch in returned data");
					string text = obj.GetString("name");
					long valueOrDefault = obj.GetLong("publishAfter").GetValueOrDefault();
					DateTime dateTime = new DateTime(valueOrDefault);
					currentBatchEntry.label.text = text;
					featuredBatchNameButtonText.text = text;
					featuredBatchPublishButtonText.text = AdminPanelDialog.DateToStr(dateTime);
					currentBatchEntry.publishAfterUTC = dateTime;
					List<string> obj2 = obj.GetStringList("codeList") ?? throw new Exception("ERROR: No level code list in batch");
					GSData gSData = resultDataGSData.GetGSData("codeToLevelNames");
					if (gSData == null)
					{
						throw new Exception("ERROR: No code-to-level name list");
					}
					currentBatchEntry.codeList.Clear();
					FeaturedBatchEntry featuredBatchEntry3 = null;
					foreach (string item in obj2)
					{
						FeaturedBatchEntry featuredBatchEntry4 = featuredBatchCodeContainer.gameObject.AddPrefabAsChild<FeaturedBatchEntry>(featuredBatchEntryPrefab);
						string text2 = gSData.GetString(item);
						if (text2.NullOrEmpty())
						{
							text2 = "???";
						}
						featuredBatchEntry4.InitializeCode(item, text2, this);
						currentBatchEntry.codeList.Add(item);
						if (featuredBatchEntry3 == null)
						{
							featuredBatchEntry3 = featuredBatchEntry4;
						}
					}
					if (featuredBatchEntry3 != null)
					{
						OnClickCode(featuredBatchEntry3)(null);
					}
					else
					{
						UpdateFeaturedCodeListButtonBar();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message);
				}
				onFinish();
			});
		}
		else
		{
			featuredBatchNameButtonText.text = currentBatchEntry.batchName;
			featuredBatchPublishButtonText.text = AdminPanelDialog.DateToStr(currentBatchEntry.publishAfterUTC);
			FeaturedBatchEntry featuredBatchEntry = null;
			foreach (string code in currentBatchEntry.codeList)
			{
				FeaturedBatchEntry featuredBatchEntry2 = featuredBatchCodeContainer.gameObject.AddPrefabAsChild<FeaturedBatchEntry>(featuredBatchEntryPrefab);
				featuredBatchEntry2.InitializeCode(code, "<Unknown>", this);
				if (featuredBatchEntry == null)
				{
					featuredBatchEntry = featuredBatchEntry2;
				}
			}
			if (featuredBatchEntry != null)
			{
				OnClickCode(featuredBatchEntry)(null);
			}
			onFinish();
		}
		featuredBatchCodeListArea.gameObject.SetActive(value: true);
	}

	public void ClearBatchContents()
	{
		featuredBatchCodeContainer.DestroyAllChildren();
		featuredBatchCodeListArea.gameObject.SetActive(value: false);
		currentBatchCode = null;
	}

	public void ClearBatchList()
	{
		currentBatchEntry = null;
		featuredBatchListContainer.DestroyAllChildren();
	}

	public void RefreshBatchList(UnityAction onFinish)
	{
		ClearBatchList();
		ClearBatchContents();
		loadingSpinner.enabled = true;
		PickableButton.maskAll = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminGetBatchList", new Dictionary<string, object>(), returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			loadingSpinner.enabled = false;
			PickableButton.ResetMasks();
			adminPanelDialog.undergroundComputer.quickInfoPane.OnGetAdminBatchListResult(query);
			List<BatchListResult> list = ProcessGetBatchListResult(query).ToList();
			list.Sort((BatchListResult a, BatchListResult b) => b.publishAfter.CompareTo(a.publishAfter));
			for (int num = 0; num < list.Count; num++)
			{
				BatchListResult batchListResult = list[num];
				FeaturedBatchEntry featuredBatchEntry = featuredBatchListContainer.gameObject.AddPrefabAsChild<FeaturedBatchEntry>(featuredBatchEntryPrefab);
				featuredBatchEntry.InitializeBatch(batchListResult.batchName, batchListResult.batchID, this);
				featuredBatchEntry.publishAfterUTC = new DateTime(batchListResult.publishAfter);
			}
			UpdatePublishLine();
			onFinish();
		});
	}

	private void UpdatePublishLine()
	{
		if (publishLine != null)
		{
			UnityEngine.Object.DestroyImmediate(publishLine);
			publishLine = null;
		}
		List<FeaturedBatchEntry> list = new List<FeaturedBatchEntry>();
		for (int i = 0; i < featuredBatchListContainer.childCount; i++)
		{
			FeaturedBatchEntry component = featuredBatchListContainer.GetChild(i).GetComponent<FeaturedBatchEntry>();
			if (component != null)
			{
				list.Add(component);
			}
		}
		long ticks = DateTime.UtcNow.Ticks;
		if (list.Count > 0 && list[0].publishAfterUTC.Ticks < ticks)
		{
			publishLine = featuredBatchListContainer.gameObject.AddPrefabAsChild(publishLinePrefab);
			publishLine.transform.SetSiblingIndex(0);
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			FeaturedBatchEntry featuredBatchEntry = list[j];
			if (j < list.Count - 1 && featuredBatchEntry.publishAfterUTC.Ticks >= ticks && list[j + 1].publishAfterUTC.Ticks < ticks)
			{
				publishLine = featuredBatchListContainer.gameObject.AddPrefabAsChild(publishLinePrefab);
				publishLine.transform.SetSiblingIndex(j + 1);
				return;
			}
		}
		publishLine = featuredBatchListContainer.gameObject.AddPrefabAsChild(publishLinePrefab);
	}

	public static IEnumerable<BatchListResult> ProcessGetBatchListResult(GameSparksQuery query)
	{
		if (query.HasError)
		{
			Debug.LogError("ERROR: " + query.Error);
			yield break;
		}
		GSData resultDataGSData = query.GetResultDataGSData("scriptData");
		if (resultDataGSData != null)
		{
			List<GSData> gSDataList = resultDataGSData.GetGSDataList("batches");
			foreach (GSData item in gSDataList)
			{
				string text = null;
				string text2 = null;
				long publishAfter = 0L;
				bool flag = false;
				try
				{
					GSData gSData = item.GetGSData("_id");
					if (gSData == null)
					{
						text = item.GetString("_id");
						if (text == null)
						{
							throw new Exception("No _id in batch record");
						}
					}
					else
					{
						text = gSData.GetString("$oid");
						if (text == null)
						{
							throw new Exception("No $oid in id container");
						}
					}
					text2 = item.GetString("name");
					if (text2 == null)
					{
						throw new Exception("No batch name in record");
					}
					publishAfter = item.GetLong("publishAfter") ?? 0;
				}
				catch (Exception ex)
				{
					flag = true;
					Debug.LogError("Error processing batch: " + ex.Message);
				}
				if (!flag)
				{
					yield return new BatchListResult
					{
						batchID = text,
						batchName = text2,
						publishAfter = publishAfter
					};
				}
			}
		}
		else
		{
			Debug.LogError("Failed to get data...");
		}
	}

	public void OnClickFeaturedBatchName(PickCursor pickCursor)
	{
		adminPanelDialog.PopupModalDialog_Input(pickCursor.localNumber, "Enter a new name for the batch:", currentBatchEntry.batchName, "Enter batch name...", OnSubmitBatchRename, delegate
		{
		});
	}

	public void OnClickAddNewBatch(PickCursor pickCursor)
	{
		adminPanelDialog.PopupModalDialog_Input(pickCursor.localNumber, "Enter name for new batch:", "New Batch", "Enter batch name...", OnConfirmAddNewBatch, delegate
		{
		});
	}

	private void OnConfirmAddNewBatch()
	{
		FeaturedBatchEntry featuredBatchEntry = featuredBatchListContainer.gameObject.AddPrefabAsChild<FeaturedBatchEntry>(featuredBatchEntryPrefab);
		featuredBatchEntry.InitializeBatch(adminPanelDialog.modalInputField.text, "NULL", this);
		featuredBatchEntry.MarkDirty();
		OnClickBatch(featuredBatchEntry)(null);
		UpdatePublishLine();
	}

	public void OnSubmitBatchRename()
	{
		currentBatchEntry.MarkDirty();
		currentBatchEntry.SetBatchName(adminPanelDialog.modalInputField.text);
		featuredBatchNameButtonText.text = adminPanelDialog.modalInputField.text;
	}

	public void OnClickPublishAfter(PickCursor pickCursor)
	{
		adminPanelDialog.modalInputField.text = AdminPanelDialog.DateToStr(currentBatchEntry.publishAfterUTC);
		((Text)adminPanelDialog.modalInputField.placeholder).text = "YYYY/MM/DD HH:MM";
		adminPanelDialog.ShowModalDialog(pickCursor.localNumber, "Enter a publish date <UTC> (YYYY/MM/DD HH:MM)", showInputField: true, OnSubmitPublishDate, delegate
		{
			Debug.Log("Canceled");
		});
	}

	public void OnSubmitPublishDate()
	{
		string text = adminPanelDialog.modalInputField.text;
		if (AdminPanelDialog.StrToDate(text, out var result))
		{
			currentBatchEntry.publishAfterUTC = result;
			featuredBatchPublishButtonText.text = text;
			currentBatchEntry.MarkDirty();
			UpdatePublishLine();
		}
		else
		{
			Debug.LogError("Incorrect date format");
			UserMessageManager.Instance.UserMessage("Incorrect date format", tieToCurrentScene: true);
		}
	}

	public void OnClickAddCode(PickCursor pickCursor)
	{
		adminPanelDialog.modalInputField.text = "";
		((Text)adminPanelDialog.modalInputField.placeholder).text = "Enter code...";
		adminPanelDialog.ShowModalDialog(pickCursor.localNumber, "Enter a code to add to this batch:", showInputField: true, OnSubmitAddCode, delegate
		{
			Debug.Log("Canceled");
		});
	}

	public void OnSubmitAddCode()
	{
		currentBatchEntry.MarkDirty();
		string text = adminPanelDialog.modalInputField.text;
		text = GameSparksQuery.SanitizeSnapshotCode(text);
		if (text != null)
		{
			FeaturedBatchEntry featuredBatchEntry = featuredBatchCodeContainer.gameObject.AddPrefabAsChild<FeaturedBatchEntry>(featuredBatchEntryPrefab);
			featuredBatchEntry.InitializeCode(text, "<Unknown>", this);
			featuredBatchEntry.transform.SetSiblingIndex(0);
			currentBatchEntry.codeList.Insert(0, text);
			OnClickCode(featuredBatchEntry)(null);
		}
		else
		{
			Debug.LogError("Code didn't validate: " + adminPanelDialog.modalInputField.text);
		}
	}

	public void OnClickRemoveCode(PickCursor pickCursor)
	{
		if (currentBatchCode != null)
		{
			int num = currentBatchCode.transform.GetSiblingIndex();
			currentBatchCode.transform.SetParent(null);
			UnityEngine.Object.Destroy(currentBatchCode.gameObject);
			if (featuredBatchCodeContainer.childCount > 0)
			{
				if (num == featuredBatchCodeContainer.childCount)
				{
					num--;
				}
				currentBatchCode = featuredBatchCodeContainer.GetChild(num).GetComponent<FeaturedBatchEntry>();
				currentBatchCode.OnSelect();
			}
			else
			{
				currentBatchCode = null;
			}
			currentBatchEntry.codeList.Clear();
			foreach (Transform item in featuredBatchCodeContainer)
			{
				FeaturedBatchEntry component = item.GetComponent<FeaturedBatchEntry>();
				if (component != null)
				{
					currentBatchEntry.codeList.Add(component.code);
				}
			}
			currentBatchEntry.MarkDirty();
			UpdateFeaturedCodeListButtonBar();
		}
		else
		{
			Debug.LogError("No code selected");
		}
	}

	public void OnClickDeleteBatch(PickCursor pickCursor)
	{
		if (!(currentBatchEntry == null))
		{
			adminPanelDialog.ShowModalDialog(pickCursor.localNumber, "Are you sure you want to delete the batch \"" + currentBatchEntry.batchName + "\"?", showInputField: false, delegate
			{
				OnConfirmDeleteCurrentBatch();
			}, delegate
			{
			});
		}
	}

	private void OnConfirmDeleteCurrentBatch()
	{
		if (currentBatchEntry.batchID != "NULL")
		{
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SendSimpleRequest("adminDeleteFeaturedLevelBatch", new Dictionary<string, object> { { "batchID", currentBatchEntry.batchID } }, returnScriptData: true);
			GameSparksQuery gameSparksQuery = query;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
			{
				if (!query.HasError)
				{
					DestroyCurrentBatch();
				}
				else
				{
					Debug.LogError("There was an error deleting the batch.");
					UserMessageManager.Instance.UserMessage("Error deleting batch", tieToCurrentScene: true);
				}
			});
		}
		else
		{
			DestroyCurrentBatch();
		}
	}

	private void DestroyCurrentBatch()
	{
		foreach (Transform item in featuredBatchListContainer)
		{
			if (item.GetComponent<FeaturedBatchEntry>() == currentBatchEntry)
			{
				UnityEngine.Object.Destroy(item.gameObject);
				break;
			}
		}
		ClearBatchContents();
		currentBatchEntry = null;
		currentBatchCode = null;
	}

	public void OnClickSaveAll(PickCursor pickCursor)
	{
		List<FeaturedBatchEntry> list = new List<FeaturedBatchEntry>();
		foreach (Transform item in featuredBatchListContainer)
		{
			FeaturedBatchEntry component = item.GetComponent<FeaturedBatchEntry>();
			if (component != null && component.dirty)
			{
				list.Add(component);
			}
		}
		if (list.Count > 0)
		{
			StartCoroutine(DoSaveBatches(list));
		}
	}

	private IEnumerator DoSaveBatches(List<FeaturedBatchEntry> batchesToSave)
	{
		Debug.Log("Saving batches...");
		loadingSpinner.enabled = true;
		PickableButton.maskAll = true;
		foreach (FeaturedBatchEntry item in batchesToSave)
		{
			yield return DoSaveBatch(item);
		}
		loadingSpinner.enabled = false;
		PickableButton.ResetMasks();
		adminPanelDialog.undergroundComputer.quickInfoPane.RefreshAdminBatchList();
	}

	private IEnumerator DoSaveBatch(FeaturedBatchEntry batch)
	{
		Debug.Log("Saving batch " + batch.batchName);
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminSubmitLevelBatch", new Dictionary<string, object>
		{
			{ "name", batch.batchName },
			{ "codeList", batch.codeList },
			{
				"publishAfter",
				batch.publishAfterUTC.Ticks
			},
			{ "batchID", batch.batchID }
		}, returnScriptData: true);
		bool haveResponse = false;
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			haveResponse = true;
			if (query.HasError)
			{
				Debug.LogError("There was an error saving batch " + batch.batchName + " (ID = " + batch.batchID + "): " + query.Error);
			}
			else
			{
				GSData resultDataGSData = query.GetResultDataGSData("scriptData");
				if (resultDataGSData != null)
				{
					string text = resultDataGSData.GetString("SetBatchID");
					if (!text.NullOrEmpty())
					{
						Debug.Log("Set batch ID: " + text);
						batch.batchID = text;
					}
				}
				batch.ClearDirtyFlag();
				Debug.Log("Saved " + batch.batchName + " successfully.");
			}
		});
		while (!haveResponse)
		{
			yield return null;
		}
	}

	private void UpdateFeaturedCodeListButtonBar()
	{
		featuredCodeRemoveButton.gameObject.SetActive(currentBatchCode != null);
		featuredCodeMoveUpButton.gameObject.SetActive(currentBatchCode != null);
		featuredCodeMoveDownButton.gameObject.SetActive(currentBatchCode != null);
	}

	public void OnClickUpdateFeedNow(PickCursor pickCursor)
	{
		if (updatingFeed)
		{
			return;
		}
		updatingFeed = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("updateFeaturedLevelFeed", new Dictionary<string, object>(), returnScriptData: false);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			updatingFeed = false;
			if (query.HasError)
			{
				Debug.LogError("Error while updating featured level feed: " + query.Error);
				UserMessageManager.Instance.UserMessage("Error while updating featured level feed");
			}
			else
			{
				UserMessageManager.Instance.UserMessage("Featured level feed updated successfully");
			}
		});
	}

	public void OnClickMoveCodeUp(PickCursor pickCursor)
	{
		if (currentBatchEntry != null && currentBatchCode != null)
		{
			int siblingIndex = currentBatchCode.transform.GetSiblingIndex();
			if (siblingIndex > 0)
			{
				currentBatchCode.transform.SetSiblingIndex(siblingIndex - 1);
				currentBatchEntry.RefreshCodeList(featuredBatchCodeContainer);
				currentBatchEntry.MarkDirty();
			}
		}
	}

	public void OnClickMoveCodeDown(PickCursor pickCursor)
	{
		if (currentBatchEntry != null && currentBatchCode != null)
		{
			int siblingIndex = currentBatchCode.transform.GetSiblingIndex();
			if (siblingIndex < currentBatchCode.transform.parent.childCount - 1)
			{
				currentBatchCode.transform.SetSiblingIndex(siblingIndex + 1);
				currentBatchEntry.RefreshCodeList(featuredBatchCodeContainer);
				currentBatchEntry.MarkDirty();
			}
		}
	}

	public void OnScrollPlus(PickCursor pickCursor)
	{
		if (!codeListScrollController.OnPickCursorScrollPlus(pickCursor))
		{
			batchListScrollController.OnPickCursorScrollPlus(pickCursor);
		}
	}

	public void OnScrollMinus(PickCursor pickCursor)
	{
		if (!codeListScrollController.OnPickCursorScrollMinus(pickCursor))
		{
			batchListScrollController.OnPickCursorScrollMinus(pickCursor);
		}
	}
}
