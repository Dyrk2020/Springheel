using System;
using GameEvent;
using UnityEngine;

public class SpinObject : MonoBehaviour, IGameEventListener
{
	public float Speed;

	public bool Clockwise;

	public bool randomSpeed;

	public float randomMin;

	public float randomMax;

	protected bool paused;

	protected bool scoreboard;

	private void Start()
	{
		ChangeListener(adding: true);
		if (randomSpeed)
		{
			Speed = UnityEngine.Random.Range(randomMin, randomMax);
		}
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
	}

	private void Update()
	{
		if (!paused && !scoreboard)
		{
			base.transform.Rotate(0f, 0f, Speed * Time.deltaTime * 60f * (float)((!Clockwise) ? 1 : (-1)));
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
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
			}
			else
			{
				paused = false;
			}
		}
		if (type == typeof(LevelResetEvent))
		{
			base.transform.localRotation = Quaternion.identity;
		}
	}
}
