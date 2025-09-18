using System;
using System.Collections;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class countDownStart : UIGraphic, IGameEventListener
{
	public enum TimerMessage
	{
		STARTING,
		VOTING,
		HOSTFORCE
	}

	public Text number;

	public Text word;

	public Text modeText;

	private int num;

	public bool Done;

	public float FadeInTime;

	public string threeSound;

	public string twoSound;

	public string oneSound;

	public string zeroSound;

	protected Animator animator;

	private IEnumerator coro;

	public int CurrentNumber => num;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
		Hide();
		if (modeText != null)
		{
			modeText.gameObject.SetActive(value: false);
		}
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public void RpcStopTimer()
	{
		Hide();
	}

	private IEnumerator fadeIn()
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		if (FadeInTime <= 0f)
		{
			canvasGroup.alpha = 1f;
			yield break;
		}
		float fadeInTimer = 0f;
		while (base.Visible && fadeInTimer < FadeInTime)
		{
			canvasGroup.alpha = fadeInTimer / FadeInTime;
			fadeInTimer += Time.deltaTime;
			yield return null;
		}
		if (base.Visible)
		{
			canvasGroup.alpha = 1f;
		}
		else
		{
			canvasGroup.alpha = 0f;
		}
	}

	public void StartCountDown(int countFrom, TimerMessage message, float timePerCount = 1f)
	{
		num = countFrom;
		number.text = num.ToString();
		Show();
		coro = fadeIn();
		coro.MoveNext();
		Done = false;
		animator.speed = 1f / timePerCount;
		animator.SetTrigger("Reset");
		animator.SetBool("CountDown", value: true);
		switch (message)
		{
		case TimerMessage.STARTING:
			word.text = ScriptLocalization.InLobby.Starting;
			break;
		case TimerMessage.VOTING:
			word.text = ScriptLocalization.InLobby.Voting;
			break;
		case TimerMessage.HOSTFORCE:
			word.text = ScriptLocalization.InLobby.autoStart;
			break;
		}
		UpdateModeText(GameSettings.GetInstance().GameMode);
	}

	private void UpdateModeText(GameState.GameMode mode)
	{
		if (modeText != null)
		{
			modeText.gameObject.SetActive(value: true);
			switch (mode)
			{
			case GameState.GameMode.FREEPLAY:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/FreePlayText");
				break;
			case GameState.GameMode.CREATIVE:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/CreativeModeText");
				break;
			case GameState.GameMode.PARTY:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/PartyModeText");
				break;
			case GameState.GameMode.CHALLENGE:
				modeText.text = LocalizationManager.GetTranslation("RuleBook/ChallengeModeText");
				break;
			}
		}
	}

	public void DecreaseNumber()
	{
		num--;
		SetNumber(num);
	}

	public void PlayCountDownSound()
	{
		if (num == 3)
		{
			AkSoundEngine.PostEvent(threeSound, base.gameObject);
		}
		else if (num == 2)
		{
			AkSoundEngine.PostEvent(twoSound, base.gameObject);
		}
		else if (num == 1)
		{
			AkSoundEngine.PostEvent(oneSound, base.gameObject);
		}
	}

	public void SetNumber(int num)
	{
		this.num = num;
		if (num > 0)
		{
			number.text = num.ToString();
		}
		else if (num == 0)
		{
			number.text = ScriptLocalization.InLobby.Countdown_Go;
			animator.SetBool("CountDown", value: false);
			AkSoundEngine.PostEvent(zeroSound, base.gameObject);
		}
	}

	public void done()
	{
		base.Hide(forceQuickHide: false);
		Done = true;
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		num = 30;
		animator.SetBool("CountDown", value: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = (MsgSwitchToMode)networkMessageReceivedEvent.ReadMessage;
				UpdateModeText(msgSwitchToMode.toMode);
			}
		}
		if (type == typeof(LanguageChangeEvent))
		{
			UpdateModeText(GameSettings.GetInstance().GameMode);
		}
	}

	public override void Update()
	{
		base.Update();
		if (coro != null && !coro.MoveNext())
		{
			coro = null;
		}
	}
}
