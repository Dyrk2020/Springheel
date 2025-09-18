using System;
using GameEvent;
using UnityEngine;

public class DancePartyBackgroundColor : MonoBehaviour, IGameEventListener
{
	public SpriteRenderer targetSprite;

	protected Animator animator;

	public int blendTreeStateCount;

	private void Start()
	{
		ChangeListener(adding: true);
		animator = GetComponent<Animator>();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				float value = 1f / (float)UnityEngine.Random.Range(0, blendTreeStateCount);
				animator.SetFloat("BuildBlend", value);
				animator.SetBool("RunMode", value: false);
			}
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				float value2 = 1f / (float)UnityEngine.Random.Range(0, blendTreeStateCount);
				animator.SetFloat("RunBlend", value2);
				animator.SetBool("RunMode", value: true);
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				animator.speed = 0f;
			}
			else
			{
				animator.speed = 1f;
			}
		}
	}
}
