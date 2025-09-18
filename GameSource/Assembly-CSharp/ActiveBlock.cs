using System;
using System.Collections;
using GameEvent;
using UnityEngine;

public class ActiveBlock : Placeable
{
	public enum ActMode
	{
		UPDATE,
		FIXED,
		NEVER
	}

	[Header("----ActiveBlock----")]
	public bool MovingPlatform;

	public ActMode ActUpdateMode = ActMode.FIXED;

	protected bool isActive;

	protected bool paused;

	protected bool scoreboard;

	public bool Active
	{
		get
		{
			return isActive;
		}
		set
		{
			if (!isActive && value)
			{
				Activate();
			}
			isActive = value;
		}
	}

	public bool Paused
	{
		get
		{
			if (!paused)
			{
				return scoreboard;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (ActUpdateMode == ActMode.FIXED)
		{
			StartCoroutine(actOnFixed());
		}
		else if (ActUpdateMode == ActMode.UPDATE)
		{
			StartCoroutine(actOnUpdate());
		}
	}

	private IEnumerator actOnFixed()
	{
		while (!markedForDestruction)
		{
			if (isActive && !Paused)
			{
				Act(Time.fixedDeltaTime);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator actOnUpdate()
	{
		while (!markedForDestruction)
		{
			if (isActive && !Paused)
			{
				Act(Time.deltaTime);
			}
			yield return null;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected virtual void Act(float deltaTime)
	{
	}

	public override void Disable()
	{
		base.Disable();
		isActive = false;
	}

	protected virtual void Activate()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void Pause()
	{
	}

	public virtual void Unpause()
	{
	}

	public override void ToPlayMode()
	{
		base.ToPlayMode();
		if (!base.MarkedForDestruction)
		{
			Reset();
			Active = false;
		}
	}

	protected override void ToPlaceMode(bool enableSelection)
	{
		base.ToPlaceMode(enableSelection);
		if (!base.MarkedForDestruction)
		{
			Reset();
			Active = false;
		}
	}

	protected override void ToSuddenDeath()
	{
		base.ToSuddenDeath();
		if (!base.MarkedForDestruction)
		{
			Reset();
			Active = false;
		}
	}

	private IEnumerator CompleteReset()
	{
		yield return new WaitForEndOfFrame();
		Reset();
		Active = false;
		yield return TimeToWaitForResetReactivation();
		Enable();
		Active = true;
	}

	protected virtual YieldInstruction TimeToWaitForResetReactivation()
	{
		return new WaitForFixedUpdate();
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(LevelResetEvent))
		{
			StartCoroutine(CompleteReset());
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
			}
			else
			{
				scoreboard = false;
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				paused = true;
				Pause();
			}
			else
			{
				paused = false;
				Unpause();
			}
		}
	}
}
