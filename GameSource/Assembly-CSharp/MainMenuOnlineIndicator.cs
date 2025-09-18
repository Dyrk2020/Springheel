using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuOnlineIndicator : MonoBehaviour, IGameEventListener
{
	public Text playOnlineText;

	public BoxCollider2D playOnlineCollider;

	public SpriteRenderer spinnerSprite;

	public PickableMainMenuButton bannedMessage;

	public Text banTipText;

	public Text banTipPermanentText;

	public Text banReasonText;

	public Transform banTipTextContainer;

	public Transform banTipSpinnerContainer;

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
		playOnlineText.color = Color.grey;
		playOnlineCollider.enabled = false;
		bannedMessage.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (UserOnlinePermissionsValid)
		{
			if (GameSparksManager.Instance.Connected)
			{
				SetPlayOnlineButtonState(spinnerActive: false, buttonActive: true);
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
				playOnlineCollider.enabled = false;
				GameSparksManager.Instance.EnableRetry(3f);
			}
		}
		else
		{
			SetPlayOnlineButtonState(spinnerActive: false, buttonActive: true);
		}
		if (GameSparksManager.Instance.MainUserBanned && !bannedMessage.gameObject.activeSelf)
		{
			EnableBanMessage();
		}
	}

	private void SetPlayOnlineButtonState(bool spinnerActive, bool buttonActive)
	{
		spinnerSprite.gameObject.SetActive(spinnerActive);
		playOnlineText.color = (buttonActive ? Color.white : Color.grey);
		playOnlineCollider.enabled = buttonActive;
	}

	private static void PerformMultiplayerChecks(UnityAction<bool> OnChecksFinished)
	{
		OnChecksFinished(arg0: true);
	}

	public void OnClickPlayOnlineButton(UnityAction<bool> OnResponse)
	{
		EnsureMainUserOnlinePermissionsValid(OnResponse, delegate
		{
			SetPlayOnlineButtonState(spinnerActive: true, buttonActive: false);
			PickableButton.maskAll = true;
		}, delegate(bool result)
		{
			if (!result)
			{
				PickableButton.ResetMasks();
				SetPlayOnlineButtonState(spinnerActive: false, buttonActive: true);
			}
		}, this);
	}

	public static void EnsureMainUserOnlinePermissionsValid(UnityAction<bool> OnResponse, UnityAction BeforeChecksCallback, UnityAction<bool> AfterChecksCallback, MonoBehaviour coroutineRunner)
	{
		BeforeChecksCallback();
		if (UserOnlinePermissionsValid)
		{
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
				AfterChecksCallback(arg0: false);
				OnResponse(arg0: false);
			}
		});
	}

	private static IEnumerator WaitForGameSparks(UnityAction<bool> OnResponse)
	{
		bool triedConnecting = false;
		bool failedToConnect = false;
		while (!GameSparksManager.Instance.Connected && !failedToConnect)
		{
			if (GameSparksManager.Instance.Connecting)
			{
				triedConnecting = true;
			}
			if (triedConnecting && !GameSparksManager.Instance.Connecting)
			{
				failedToConnect = true;
			}
			yield return null;
		}
		PickableButton.ResetMasks();
		OnResponse(!failedToConnect);
	}

	private void EnableBanMessage()
	{
		bannedMessage.gameObject.SetActive(value: true);
		bannedMessage.Enable();
		banTipTextContainer.gameObject.SetActive(value: false);
		banTipSpinnerContainer.gameObject.SetActive(value: true);
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
		banTipTextContainer.gameObject.SetActive(value: true);
		banTipSpinnerContainer.gameObject.SetActive(value: false);
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
		SetPlayOnlineButtonState(spinnerActive: false, buttonActive: false);
	}

	private void HideBan()
	{
		bannedMessage.Disable();
		bannedMessage.gameObject.SetActive(value: false);
	}
}
