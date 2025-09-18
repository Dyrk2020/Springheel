using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeleteDialog : MonoBehaviour
{
	public UndergroundComputer undergroundComputer;

	public ThumbnailDisplaySlot thumbnailDisplaySlot;

	public Text levelNameText;

	public Text loadingIndicatorText;

	public Transform deleteArea;

	private string code;

	private void Awake()
	{
		HideAllPanes();
	}

	private void HideAllPanes()
	{
		deleteArea.gameObject.SetActive(value: false);
		loadingIndicatorText.gameObject.SetActive(value: false);
	}

	public void OnClose()
	{
		HideAllPanes();
	}

	public void DisplayLevelInfo(string code, string levelName)
	{
		this.code = code;
		levelNameText.text = levelName;
		thumbnailDisplaySlot.LoadThumbnail(code, levelName);
		deleteArea.gameObject.SetActive(value: false);
		loadingIndicatorText.gameObject.SetActive(value: true);
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.GetMyLevelReport(code);
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.DeleteDialog)
			{
				loadingIndicatorText.gameObject.SetActive(value: false);
				deleteArea.gameObject.SetActive(value: true);
			}
		});
	}

	public void OnClickSubmit()
	{
		PickableButton.maskAll = true;
		undergroundComputer.AddLoadingOperation();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SetLevelApprovalStatus(code, 6);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			undergroundComputer.RemoveLoadingOperation();
			if (!query.HasError)
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/DeleteConfirm"), 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				undergroundComputer.OnClickDeleteConfirm();
			}
			else
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/DeleteFailed"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			}
		});
	}
}
