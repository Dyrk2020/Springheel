using System;
using GameEvent;
using UnityEngine;

public class SuicideNote : UIGraphic, IGameEventListener
{
	public Canvas NoteCanvas;

	public float MaxRunTime;

	private float runTime;

	private bool running;

	private void Start()
	{
		ChangeListener(adding: true);
		Hide();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<PlayersDoneRunning>(this, adding);
	}

	public override void Update()
	{
		base.Update();
		if (!base.Visible && running)
		{
			runTime += Time.unscaledDeltaTime;
			if (runTime >= MaxRunTime)
			{
				Show();
			}
		}
	}

	public override void Show()
	{
		base.Show();
		NoteCanvas.enabled = true;
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		NoteCanvas.enabled = false;
		runTime = 0f;
	}

	public void Pause()
	{
		running = false;
	}

	public void Unpause()
	{
		running = true;
	}

	public void Reset()
	{
		runTime = 0f;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY || startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				running = true;
			}
			else
			{
				running = false;
			}
			Hide();
		}
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			running = !pauseEvent.Paused;
		}
		if (type == typeof(PlayersDoneRunning))
		{
			running = false;
			Hide();
		}
	}
}
