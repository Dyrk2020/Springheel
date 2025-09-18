using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabletMainMenuOnlineIndicator : MonoBehaviour, IGameEventListener
{
	public TabletDisableGroup playOnlineDisableGroup;

	public TabletButton playOnlineButton;

	public Image playOnlineSpinner;

	public CanvasGroup banMessageCanvasGroup;

	public RectTransform banTextContainer;

	public Image banTipSpinner;

	public TabletTextLabel bannedMessage;

	public TabletTextLabel banTipText;

	public TabletTextLabel banTipPermanentText;

	public TabletTextLabel banReasonText;

	private bool banMessageEnabled;

	private bool recheckingPermissions;

	private static bool UserOnlinePermissionsValid => true;

	private void Awake()
	{
		ChangeListeners(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListeners(adding: false);
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PlayerInGameRuleEvent) && !(e as PlayerInGameRuleEvent).Entered)
		{
			HideBan();
			GameSparksManager.Instance.MainUserBanned = false;
		}
	}

	private void Start()
	{
		playOnlineDisableGroup.SetDisabled(disabled: true);
		HideBan();
	}

	private void Update()
	{
		if (!recheckingPermissions)
		{
			if (UserOnlinePermissionsValid)
			{
				if (GameSparksManager.Instance.Connected)
				{
					SetPlayOnlineButtonState(spinnerActive: false, !banMessageEnabled);
				}
				else
				{
					if (GameSparksManager.Instance.Connecting)
					{
						SetPlayOnlineButtonState(spinnerActive: true, buttonActive: false);
					}
					else if (!GameSparksManager.Instance.AllowAutoRetry)
					{
						SetPlayOnlineButtonState(spinnerActive: false, buttonActive: false);
					}
					GameSparksManager.Instance.EnableRetry(3f);
				}
			}
			else
			{
				SetPlayOnlineButtonState(spinnerActive: false, !banMessageEnabled);
			}
		}
		if (GameSparksManager.Instance.MainUserBanned && !banMessageEnabled)
		{
			EnableBanMessage();
		}
		if (banMessageEnabled)
		{
			banMessageCanvasGroup.alpha = (playOnlineButton.HasTrackedCursors ? 1f : 0f);
		}
	}

	public void ForceSetOnlineButtonState(bool spinnerActive, bool buttonActive)
	{
		recheckingPermissions = true;
		SetPlayOnlineButtonState(spinnerActive, buttonActive);
	}

	public void ResetForcedSet()
	{
		recheckingPermissions = false;
	}

	protected void SetPlayOnlineButtonState(bool spinnerActive, bool buttonActive)
	{
		playOnlineSpinner.gameObject.SetActive(spinnerActive);
		if (playOnlineDisableGroup.Disabled != !buttonActive)
		{
			playOnlineDisableGroup.SetDisabled(!buttonActive);
		}
	}

	public static void PerformMultiplayerChecks(UnityAction<bool> OnChecksFinished)
	{
		OnChecksFinished(arg0: true);
	}

	public void OnClickPlayOnlineButton(UnityAction<bool> OnResponse)
	{
		bool force = false;
		EnsureMainUserOnlinePermissionsValid(OnResponse, delegate
		{
			recheckingPermissions = true;
			SetPlayOnlineButtonState(spinnerActive: true, buttonActive: false);
		}, delegate(bool result)
		{
			recheckingPermissions = false;
			if (!result)
			{
				SetPlayOnlineButtonState(spinnerActive: false, !banMessageEnabled);
			}
		}, this, force);
	}

	public static void EnsureMainUserOnlinePermissionsValid(UnityAction<bool> OnResponse, UnityAction BeforeChecksCallback, UnityAction<bool> AfterChecksCallback, MonoBehaviour coroutineRunner, bool force = false)
	{
		BeforeChecksCallback();
		Debug.Log("Is UGC restricted: " + PlatformFeatureRestrictions.IsUGCRestricted);
		if (UserOnlinePermissionsValid && !force)
		{
			Debug.Log("Online permissions valid");
			if (GameSparksManager.Instance.Connected)
			{
				if (GameSparksManager.Instance.MainUserPermissionLevel >= 0)
				{
					AfterChecksCallback(arg0: true);
					OnResponse(arg0: true);
				}
				else
				{
					GameSparksManager.Instance.MainUserBanned = true;
					AfterChecksCallback(arg0: false);
					OnResponse(arg0: false);
				}
			}
			else
			{
				AfterChecksCallback(arg0: false);
			}
			return;
		}
		if (!force || !UserOnlinePermissionsValid)
		{
			Debug.Log("Online permissions invalid");
		}
		else
		{
			Debug.Log("Forcing permission check");
		}
		PerformMultiplayerChecks(delegate(bool success)
		{
			if (success)
			{
				coroutineRunner.StartCoroutine(WaitForGameSparks(delegate(bool result)
				{
					if (result)
					{
						if (GameSparksManager.Instance.MainUserPermissionLevel >= 0)
						{
							AfterChecksCallback(arg0: true);
							OnResponse(arg0: true);
						}
						else
						{
							GameSparksManager.Instance.MainUserBanned = true;
							AfterChecksCallback(arg0: false);
							OnResponse(arg0: false);
						}
					}
					else
					{
						AfterChecksCallback(arg0: false);
						OnResponse(arg0: false);
					}
				}));
			}
			else
			{
				Debug.Log("Multiplayer checks failed... ");
				AfterChecksCallback(arg0: false);
				OnResponse(arg0: false);
			}
		});
	}

	private static IEnumerator WaitForGameSparks(UnityAction<bool> OnResponse)
	{
		Debug.Log("Waiting for backend");
		_ = !GameSparksManager.Instance.Connected;
		while (!GameSparksManager.Instance.Connected)
		{
			if (GameSparksManager.Instance.Connecting ? true : false)
			{
				if (!GameSparksManager.Instance.Connecting)
				{
					Debug.Log("Tried connecting to backend. Connected: " + GameSparksManager.Instance.Connected);
					break;
				}
				if (!GameSparksManager.Instance.Connecting)
				{
				}
				yield return null;
				continue;
			}
			Debug.Log("Backend is not trying to connect. Status: " + GameSparksManager.Instance.Connected);
			break;
		}
		bool flag = !GameSparksManager.Instance.Connected;
		PickableButton.ResetMasks();
		Debug.Log("Done waiting for backend: " + !flag);
		OnResponse(!flag);
	}

	private void EnableBanMessage()
	{
		playOnlineDisableGroup.SetDisabled(disabled: true);
		banMessageEnabled = true;
		banTextContainer.gameObject.SetActive(value: false);
		banTipSpinner.enabled = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("GetBanInfo", new Dictionary<string, object>(), returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (!query.HasError)
			{
				if (query.ResultData.TryGetValue("scriptData", out var value) && value is GSData gSData)
				{
					string banReason = gSData.GetString("reason");
					long num = gSData.GetLong("hoursToGo").GetValueOrDefault();
					if ((gSData.GetLong("permanent") ?? 0) == 1)
					{
						num = -1L;
					}
					ShowBan((int)num, banReason);
				}
			}
			else
			{
				Debug.LogError("Ban info not found... " + query.Error);
			}
		});
	}

	private void ShowBan(int hoursToGo, string banReason)
	{
		banTextContainer.gameObject.SetActive(value: true);
		banTipSpinner.enabled = false;
		if (hoursToGo >= 0)
		{
			banTipPermanentText.gameObject.SetActive(value: false);
			banTipText.gameObject.SetActive(value: true);
			int num = hoursToGo / 24;
			int num2 = hoursToGo - num * 24;
			if (num == 0 && num2 < 1)
			{
				num2 = 1;
			}
			banTipText.text = string.Format(LocalizationManager.GetTranslation("Options/Start/BannedTipText"), num, num2);
		}
		else
		{
			banTipPermanentText.gameObject.SetActive(value: true);
			banTipText.gameObject.SetActive(value: false);
		}
		banReasonText.text = banReason;
		SetPlayOnlineButtonState(spinnerActive: false, !banMessageEnabled);
	}

	private void HideBan()
	{
		banMessageEnabled = false;
		banTextContainer.gameObject.SetActive(value: false);
		banTipSpinner.enabled = false;
	}
}
