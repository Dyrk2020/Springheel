using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class DanceLightSystem : MonoBehaviour, IGameEventListener
{
	public DanceLightGroup[] danceLightGroups;

	private DanceLightGroup currentDanceLightGroup;

	public Queue<DanceLightGroup> danceLights = new Queue<DanceLightGroup>();

	private void Awake()
	{
		DanceLightGroup[] array = danceLightGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}

	private void Start()
	{
		DanceLightGroup[] array = danceLightGroups;
		foreach (DanceLightGroup danceLightGroup in array)
		{
			danceLightGroup.Deactivate();
			danceLightGroup.gameObject.SetActive(value: false);
			danceLights.Enqueue(danceLightGroup);
		}
		currentDanceLightGroup = danceLights.Peek();
		ChangeListener(adding: true);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	private void StartNewLightSequence()
	{
		currentDanceLightGroup.gameObject.SetActive(value: false);
		currentDanceLightGroup = danceLights.Dequeue();
		currentDanceLightGroup.gameObject.SetActive(value: true);
		currentDanceLightGroup.Activate();
		danceLights.Enqueue(currentDanceLightGroup);
	}

	private void EnterBuildMode()
	{
		currentDanceLightGroup.Deactivate();
	}

	private void Pause()
	{
		currentDanceLightGroup.Pause();
	}

	private void Unpause()
	{
		currentDanceLightGroup.Unpause();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				StartNewLightSequence();
			}
			if (obj.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				StartNewLightSequence();
			}
		}
		if (type == typeof(EndPhaseEvent) && (e as EndPhaseEvent).Phase == GameControl.GamePhase.PLAY)
		{
			EnterBuildMode();
		}
		if (type == typeof(ScoreboardEvent))
		{
			_ = (e as ScoreboardEvent).Showing;
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Pause();
			}
			else
			{
				Unpause();
			}
		}
	}
}
