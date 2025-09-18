using System;
using GameEvent;
using UnityEngine;

public class RotateLights : MonoBehaviour, IGameEventListener
{
	public float constantRotateSpeed;

	protected bool rotating;

	private void Start()
	{
		ChangeListener(adding: true);
		rotating = true;
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void Update()
	{
		if (rotating)
		{
			base.transform.Rotate(new Vector3(0f, 0f, constantRotateSpeed * Time.deltaTime));
		}
	}

	private void Pause()
	{
		rotating = false;
	}

	private void Unpause()
	{
		rotating = true;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
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
