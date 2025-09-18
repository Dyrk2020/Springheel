using System;
using GameEvent;
using UnityEngine;

public class WaveMoveV2 : ActiveBlock
{
	public Animator animator;

	private bool waving;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Activate()
	{
		base.Activate();
		animator.SetTrigger("Start");
		waving = true;
	}

	protected override void Act(float deltaTime)
	{
		if (!paused)
		{
			_ = scoreboard;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (waving)
		{
			Debug.Log("Resetting wave");
			animator.SetTrigger("Reset");
			waving = false;
		}
	}

	public void InstantReset()
	{
		if (waving)
		{
			animator.SetTrigger("InstantReset");
			waving = false;
		}
	}

	public override void Pause()
	{
		base.Pause();
		animator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
	}

	protected override void ToSuddenDeath()
	{
		base.ToSuddenDeath();
		animator.SetBool("OneWave", value: true);
	}

	public override void SpriteAssignmentRule(SpriteRenderer sr)
	{
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(FreePlayCharacterRespawnEvent) || type == typeof(LevelResetEvent))
		{
			InstantReset();
		}
		else if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.PLAY && GameState.GetInstance().UsingHotSeat)
		{
			InstantReset();
		}
		base.handleEvent(e);
	}
}
