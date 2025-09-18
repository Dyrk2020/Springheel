using System;
using System.Collections;
using GameEvent;
using UnityEngine;

public class CanvasGroupFader : MonoBehaviour, IGameEventListener
{
	public CanvasGroup[] canvasGroups;

	public CanvasGroup[] canvasGroupsBlankLevelOnly;

	public CanvasGroup[] InvertedCanvasGroups;

	protected float currentAlpha;

	protected Coroutine animationCoroutine;

	private void Awake()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<PrepareBlankLevelForScreenShot>(this, adding);
	}

	private IEnumerator FadeCanvasGroups(float from, float to, float time)
	{
		SetAllCanvasGroupAlphas(from);
		float timer = 0f;
		do
		{
			yield return null;
			timer += Time.unscaledDeltaTime;
			currentAlpha = Mathf.Lerp(from, to, timer / time);
			SetAllCanvasGroupAlphas(currentAlpha);
		}
		while (timer < time);
		SetAllCanvasGroupAlphas(to);
	}

	private void SetAllCanvasGroupAlphas(float value)
	{
		CanvasGroup[] array = canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = value;
		}
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
		{
			array = InvertedCanvasGroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].alpha = 1f - value;
			}
			array = canvasGroupsBlankLevelOnly;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].alpha = value;
			}
		}
		else
		{
			array = InvertedCanvasGroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].alpha = 1f;
			}
			array = canvasGroupsBlankLevelOnly;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].alpha = 0f;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				if (animationCoroutine != null)
				{
					StopCoroutine(animationCoroutine);
				}
				animationCoroutine = StartCoroutine(FadeCanvasGroups(currentAlpha, 0f, 1f));
			}
			if (obj.Phase == GameControl.GamePhase.PLACE && GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE)
			{
				if (animationCoroutine != null)
				{
					StopCoroutine(animationCoroutine);
				}
				animationCoroutine = StartCoroutine(FadeCanvasGroups(currentAlpha, 1f, 1f));
			}
		}
		if (!(type == typeof(PrepareBlankLevelForScreenShot)))
		{
			return;
		}
		if ((e as PrepareBlankLevelForScreenShot).Hidden)
		{
			CanvasGroup[] invertedCanvasGroups = InvertedCanvasGroups;
			for (int i = 0; i < invertedCanvasGroups.Length; i++)
			{
				invertedCanvasGroups[i].alpha = 1f;
			}
			invertedCanvasGroups = canvasGroups;
			for (int i = 0; i < invertedCanvasGroups.Length; i++)
			{
				invertedCanvasGroups[i].alpha = 0f;
			}
			invertedCanvasGroups = canvasGroupsBlankLevelOnly;
			for (int i = 0; i < invertedCanvasGroups.Length; i++)
			{
				invertedCanvasGroups[i].alpha = 0f;
			}
		}
		else
		{
			SetAllCanvasGroupAlphas(currentAlpha);
		}
	}
}
