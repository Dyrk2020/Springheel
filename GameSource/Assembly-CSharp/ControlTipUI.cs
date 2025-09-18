using System;
using System.Collections;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ControlTipUI : UIGraphic, IGameEventListener
{
	public Text TipText;

	public Text TipTextShadow;

	protected Animator animator;

	private GameState.GameMode currentMode;

	private GameControl.GamePhase currentPhase;

	public bool ShowDebugMessages;

	private bool phaseEnded;

	protected bool coroutineLock;

	public ControlTipData controlTipData;

	public MultiControllerButton multiControllerButton1;

	public MultiControllerButton multiControllerButton2;

	public void Start()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("On", value: false);
		Hide(forceQuickHide: true);
		ChangeListener(adding: true);
		controlTipData = GameState.GetInstance().controlTips;
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<GameStartEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<PartyBoxEvent>(this, adding);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public bool checkForTips()
	{
		bool result = false;
		if (currentPhase == GameControl.GamePhase.PLAY)
		{
			if (controlTipData.roundsTillNextRunRound > 0)
			{
				controlTipData.roundsTillNextRunRound--;
			}
			else if (controlTipData.askForHint(ControlTipData.KnowledgeType.SPRINT, ShowDebugMessages))
			{
				result = true;
				StartCoroutine(DisplayMessage(ScriptLocalization.InGameText.Tip_Hold_X_to_Run, ControlTipData.KnowledgeType.SPRINT, currentPhase));
				controlTipData.roundsTillNextRunRound = GameSettings.GetInstance().roundsBetweenHints;
			}
		}
		else if (currentPhase == GameControl.GamePhase.PLACE)
		{
			if (controlTipData.roundsTillNextBuildRound > 0)
			{
				controlTipData.roundsTillNextBuildRound--;
			}
			else if (controlTipData.askForHint(ControlTipData.KnowledgeType.ROTATE, ShowDebugMessages))
			{
				result = true;
				StartCoroutine(DisplayMessage(ScriptLocalization.InGameText.Tip_ShoulderButtonRotate, ControlTipData.KnowledgeType.ROTATE, currentPhase));
				controlTipData.roundsTillNextBuildRound = GameSettings.GetInstance().roundsBetweenHints;
			}
		}
		return result;
	}

	private IEnumerator DisplayMessage(string txt, ControlTipData.KnowledgeType type, GameControl.GamePhase currentPhase)
	{
		if (!coroutineLock)
		{
			coroutineLock = true;
			float timer = 0f;
			TipText.text = txt;
			TipTextShadow.text = txt;
			switch (type)
			{
			case ControlTipData.KnowledgeType.ROTATE:
				multiControllerButton1.inputKey = InputEvent.InputKey.RotateLeft;
				multiControllerButton2.inputKey = InputEvent.InputKey.RotateRight;
				multiControllerButton2.gameObject.SetActive(value: true);
				break;
			case ControlTipData.KnowledgeType.SPRINT:
				multiControllerButton1.inputKey = InputEvent.InputKey.Sprint;
				multiControllerButton2.gameObject.SetActive(value: false);
				break;
			default:
				multiControllerButton1.gameObject.SetActive(value: false);
				multiControllerButton2.gameObject.SetActive(value: false);
				break;
			}
			Show();
			animator.SetBool("On", value: true);
			while (timer < GameSettings.GetInstance().hintMinDisplayTime || (controlTipData.askForHint(type, ShowDebugMessages) && timer < GameSettings.GetInstance().hintDisplayTime && !phaseEnded))
			{
				timer += Time.deltaTime;
				yield return null;
			}
			phaseEnded = false;
			animator.SetBool("On", value: false);
			coroutineLock = false;
		}
	}

	public void HideMe()
	{
		Hide();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(GameStartEvent))
		{
			GameStartEvent gameStartEvent = e as GameStartEvent;
			currentMode = gameStartEvent.GameMode;
		}
		if (type == typeof(PartyBoxEvent) && !(e as PartyBoxEvent).Opened)
		{
			checkForTips();
		}
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (currentPhase != startPhaseEvent.Phase)
			{
				phaseEnded = true;
			}
			currentPhase = startPhaseEvent.Phase;
			if (currentPhase == GameControl.GamePhase.PLACE && currentMode == GameState.GameMode.CREATIVE)
			{
				checkForTips();
			}
			if (currentPhase == GameControl.GamePhase.PLAY)
			{
				checkForTips();
			}
		}
	}
}
