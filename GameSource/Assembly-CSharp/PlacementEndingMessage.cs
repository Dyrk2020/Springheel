using System;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class PlacementEndingMessage : UIGraphic, IGameEventListener
{
	public Canvas MessageCanvas;

	public Text MessageText;

	public Text MessageShadow;

	public Text TimeText;

	public Text TimeShadow;

	private float time;

	private bool paused;

	private bool scoreboard;

	private int lastInt;

	private void Start()
	{
		ChangeListener(adding: true);
	}

	public override void Update()
	{
		if (!(time > 0f) || paused || scoreboard)
		{
			return;
		}
		time -= Time.unscaledDeltaTime;
		if (time < 0f)
		{
			time = 0f;
		}
		int num = Mathf.CeilToInt(time);
		if (lastInt != num && base.Visible)
		{
			lastInt = num;
			switch (num)
			{
			case 5:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_05", base.gameObject);
				break;
			case 4:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_04", base.gameObject);
				break;
			case 3:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_03", base.gameObject);
				break;
			case 2:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_02", base.gameObject);
				break;
			case 1:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_01", base.gameObject);
				break;
			case 0:
				AkSoundEngine.PostEvent("UI_Placement_Countdown_00", base.gameObject);
				break;
			}
		}
		TimeText.text = num.ToString();
		TimeShadow.text = num.ToString();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
	}

	public void Show(float timeLeft, bool showText)
	{
		Show();
		time = timeLeft;
		int num = Mathf.CeilToInt(time);
		TimeText.text = num.ToString();
		TimeShadow.text = num.ToString();
		TimeText.enabled = true;
		TimeShadow.enabled = true;
		if (!showText)
		{
			MessageText.enabled = false;
			MessageShadow.enabled = false;
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		TimeText.enabled = false;
		TimeShadow.enabled = false;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			paused = pauseEvent.Paused;
		}
		if (type == typeof(ScoreboardEvent))
		{
			ScoreboardEvent scoreboardEvent = e as ScoreboardEvent;
			scoreboard = scoreboardEvent.Showing;
		}
	}
}
