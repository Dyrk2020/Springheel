using UnityEngine;
using UnityEngine.UI;

public class RunTimer : UIGraphic
{
	public VersusControl versusController;

	public float roundStartTime;

	private float pauseStartTime;

	private float pauseTime;

	private bool runStarted;

	private bool clockShown;

	private bool clockTripped;

	private bool paused;

	private float maxTime;

	private bool alwaysShowClock;

	public Transform clockContainer;

	public Text clockText;

	public Color StartColor;

	public Color EndColor;

	public bool Paused => paused;

	public bool Tripped => clockTripped;

	public float TimeLeft
	{
		get
		{
			if (runStarted)
			{
				float num = Time.realtimeSinceStartup - roundStartTime - pauseTime;
				return maxTime - num;
			}
			return 0f;
		}
	}

	public void Start()
	{
		Hide();
	}

	public void OnStartRun(float time, bool alwaysShowClock)
	{
		if (time != 0f)
		{
			this.alwaysShowClock = alwaysShowClock;
			maxTime = time;
			roundStartTime = Time.realtimeSinceStartup;
			runStarted = true;
			clockTripped = false;
			paused = false;
			pauseTime = 0f;
		}
	}

	public void PauseRun()
	{
		if (!paused)
		{
			pauseStartTime = Time.realtimeSinceStartup;
			paused = true;
		}
	}

	public void UnpauseRun()
	{
		if (paused)
		{
			pauseTime += Time.realtimeSinceStartup - pauseStartTime;
			paused = false;
		}
	}

	public void OnEndRun()
	{
		runStarted = false;
		clockTripped = false;
		Hide();
	}

	public override void Update()
	{
		base.Update();
		if (runStarted && !paused)
		{
			float num = Time.realtimeSinceStartup - roundStartTime - pauseTime;
			float num2 = (alwaysShowClock ? maxTime : Mathf.Max(maxTime / 4f, 20f));
			float num3 = maxTime - num;
			if (!clockShown && num3 <= num2 && num3 >= -1f)
			{
				Show();
			}
			else if (clockShown && num3 < -1f)
			{
				Hide();
			}
			if (clockShown)
			{
				float num4 = Mathf.Max(0f, num3);
				UpdateClockTime(num4, 1f - num4 / num2);
			}
			if (!clockTripped && num3 < 0f)
			{
				clockTripped = true;
				versusController.OnRunTimerLimitReached();
			}
		}
		else if (clockShown)
		{
			Hide();
		}
	}

	public override void Show()
	{
		base.Show();
		clockShown = true;
	}

	private void UpdateClockTime(float secondsLeft, float colorTime)
	{
		clockText.color = Color.Lerp(StartColor, EndColor, colorTime);
		if (secondsLeft != 0f)
		{
			clockText.text = HighscoreDisplayEntry.GetTimeString(secondsLeft);
		}
		else
		{
			clockText.text = "00:00.00";
		}
	}

	private void Hide()
	{
		Hide(forceQuickHide: false);
		clockShown = false;
	}
}
