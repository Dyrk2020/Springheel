using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdminPanelDialog : MonoBehaviour
{
	public enum SubDialog
	{
		None,
		IndexPage,
		ListBatches,
		Modal,
		UserReports,
		AdminActionLogs
	}

	public class CurrentState
	{
		public SubDialog subDialog;

		public CurrentState Clone()
		{
			return new CurrentState
			{
				subDialog = subDialog
			};
		}
	}

	public UndergroundComputer undergroundComputer;

	public Transform mainContentRect;

	public Transform indexPageRect;

	public Transform listBatchesRect;

	public Transform modalRect;

	public Transform userReportsDialogRect;

	public Transform adminActionLogsRect;

	public Text modalPrompt;

	public InputField modalInputField;

	public GenericButton modalOkButton;

	public GenericButton modalCancelButton;

	public CurrentState currentState;

	private List<CurrentState> stateStack = new List<CurrentState>();

	public AdminBatchManagementDialog batchManagementDialog;

	public AdminUserReportsDialog userReportsDialog;

	private void PushCurrentState()
	{
		stateStack.Add(currentState.Clone());
	}

	private void PopCurrentState()
	{
		currentState = stateStack[stateStack.Count - 1];
		stateStack.RemoveAt(stateStack.Count - 1);
	}

	private void Awake()
	{
	}

	public void OnBack()
	{
		if (currentState.subDialog != SubDialog.UserReports || !userReportsDialog.OnBack())
		{
			currentState = new CurrentState();
			currentState.subDialog = SubDialog.IndexPage;
			UpdateVisibility();
		}
	}

	public void Initialize(UndergroundComputer undergroundComputer)
	{
		ResetDialog();
		this.undergroundComputer = undergroundComputer;
		stateStack.Clear();
		currentState.subDialog = SubDialog.IndexPage;
		UpdateVisibility();
	}

	public void OnClose()
	{
		currentState.subDialog = SubDialog.None;
		UpdateVisibility();
	}

	public void OnClickBatchManagement(PickCursor pickCursor)
	{
		currentState.subDialog = SubDialog.ListBatches;
		UpdateVisibility();
		batchManagementDialog.Initialize();
	}

	public void ResetDialog()
	{
		currentState = new CurrentState();
		currentState.subDialog = SubDialog.None;
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		mainContentRect.gameObject.SetActive(value: true);
		indexPageRect.gameObject.SetActive(value: false);
		listBatchesRect.gameObject.SetActive(value: false);
		modalRect.gameObject.SetActive(value: false);
		userReportsDialogRect.gameObject.SetActive(value: false);
		adminActionLogsRect.gameObject.SetActive(value: false);
		switch (currentState.subDialog)
		{
		case SubDialog.None:
			mainContentRect.gameObject.SetActive(value: false);
			break;
		case SubDialog.IndexPage:
			indexPageRect.gameObject.SetActive(value: true);
			break;
		case SubDialog.ListBatches:
			listBatchesRect.gameObject.SetActive(value: true);
			break;
		case SubDialog.Modal:
			modalRect.gameObject.SetActive(value: true);
			break;
		case SubDialog.UserReports:
			userReportsDialogRect.gameObject.SetActive(value: true);
			break;
		case SubDialog.AdminActionLogs:
			adminActionLogsRect.gameObject.SetActive(value: true);
			break;
		}
	}

	public void ShowModalDialog(int playerNumber, string promptText, bool showInputField, UnityAction OnClickOk, UnityAction OnClickCancel)
	{
		PushCurrentState();
		currentState.subDialog = SubDialog.Modal;
		UpdateVisibility();
		modalPrompt.text = promptText;
		if (showInputField)
		{
			modalInputField.gameObject.SetActive(value: true);
			ActivateModalInputField(promptText, playerNumber);
		}
		else
		{
			modalInputField.gameObject.SetActive(value: false);
		}
		modalOkButton.OnClick.RemoveAllListeners();
		modalCancelButton.OnClick.RemoveAllListeners();
		if (OnClickOk != null)
		{
			modalOkButton.gameObject.SetActive(value: true);
			modalOkButton.OnClick.AddListener(PopStateAndExecute(OnClickOk));
		}
		else
		{
			modalOkButton.gameObject.SetActive(value: false);
		}
		if (OnClickCancel != null)
		{
			modalCancelButton.gameObject.SetActive(value: true);
			modalCancelButton.OnClick.AddListener(PopStateAndExecute(OnClickCancel));
		}
		else
		{
			modalCancelButton.gameObject.SetActive(value: false);
		}
	}

	private UnityAction PopStateAndExecute(UnityAction action)
	{
		return delegate
		{
			PickableButton.ResetMasks();
			PopCurrentState();
			UpdateVisibility();
			if (action != null)
			{
				action();
			}
		};
	}

	public void OnClickModalInputField(PickCursor pickCursor)
	{
		ActivateModalInputField(modalPrompt.text, pickCursor.localNumber);
	}

	private void OnModalInputFieldEndEditWithKeyboard()
	{
		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && modalOkButton.gameObject.activeInHierarchy)
		{
			modalOkButton.OnAccept(null);
		}
	}

	public void ActivateModalInputField(string promptText, int playerNumber)
	{
		Controller.LockInputField(modalInputField, delegate
		{
			OnModalInputFieldEndEditWithKeyboard();
		});
		PickableButton.AllowOnlyButtons(modalOkButton, modalCancelButton, modalInputField.GetComponent<GenericButton>());
	}

	public void OnClickViewUserReports()
	{
		currentState.subDialog = SubDialog.UserReports;
		UpdateVisibility();
		userReportsDialog.Initialize(this);
	}

	public void OnClickViewAdminActionLogs()
	{
		currentState.subDialog = SubDialog.AdminActionLogs;
		UpdateVisibility();
	}

	public static string DateToStr(DateTime utcDate)
	{
		return utcDate.ToString("yyyy/MM/dd HH:mm", Parsing.invariantCulture);
	}

	public static bool StrToDate(string str, out DateTime result)
	{
		try
		{
			result = DateTime.ParseExact(str, "yyyy/MM/dd HH:mm", Parsing.invariantCulture);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Date parse error: " + ex.Message);
			result = new DateTime(0L);
			return false;
		}
	}

	public void OnScrollPlus(PickCursor pickCursor)
	{
		switch (currentState.subDialog)
		{
		case SubDialog.ListBatches:
			batchManagementDialog.OnScrollPlus(pickCursor);
			break;
		case SubDialog.UserReports:
			userReportsDialog.OnScrollPlus(pickCursor);
			break;
		}
	}

	public void OnScrollMinus(PickCursor pickCursor)
	{
		switch (currentState.subDialog)
		{
		case SubDialog.ListBatches:
			batchManagementDialog.OnScrollMinus(pickCursor);
			break;
		case SubDialog.UserReports:
			userReportsDialog.OnScrollMinus(pickCursor);
			break;
		}
	}

	public void PopupModalDialog_Input(int playerNumber, string promptMessage, string initialValue, string placeholderText, UnityAction onClickOk, UnityAction onClickCancel)
	{
		modalInputField.text = initialValue;
		((Text)modalInputField.placeholder).text = placeholderText;
		ShowModalDialog(playerNumber, promptMessage, showInputField: true, onClickOk, onClickCancel);
	}

	public static string DurationToStringEnglish(long totalSeconds)
	{
		if (totalSeconds < 60)
		{
			return totalSeconds + " s";
		}
		if (totalSeconds < 3600)
		{
			long num = totalSeconds / 60;
			long num2 = totalSeconds - num * 60;
			string text = num + " min";
			if (num2 != 0L)
			{
				text = text + ", " + num2 + " s";
			}
			return text;
		}
		if (totalSeconds < 86400)
		{
			long num3 = totalSeconds / 3600;
			long num4 = (totalSeconds - num3 * 3600) / 60;
			string text2 = num3 + " h";
			if (num4 != 0L)
			{
				text2 = text2 + ", " + num4 + " min";
			}
			return text2;
		}
		long num5 = totalSeconds / 86400;
		long num6 = (totalSeconds - num5 * 86400) / 3600;
		string text3 = num5 + " d";
		if (num6 != 0L)
		{
			text3 = text3 + ", " + num6 + " h";
		}
		return text3;
	}
}
