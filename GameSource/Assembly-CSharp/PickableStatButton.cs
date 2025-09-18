using UnityEngine;

public class PickableStatButton : PickableButton
{
	public enum StatButtonJobs
	{
		ResetAllStats,
		ResetAllStatsConfirmMessage,
		ResetAllStatsConfirmMessageYes,
		ResetAllStatsConfirmMessageNo,
		ResetAllStatsConfirmMessageSlash,
		ResetAllStatsConfirmMessageConfirmAgain,
		ResetAllStatsConfirmMessageConfirmAgainYes,
		ResetAllStatsConfirmMessageConfirmAgainNo,
		ResetAllStatsConfirmMessageConfirmAgainSlash
	}

	public StatButtonJobs job;

	public static bool showShowClearGameMessage;

	public static bool showShowClearGameMessageConfirm;

	protected override void Start()
	{
		base.Start();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		showShowClearGameMessage = false;
		showShowClearGameMessageConfirm = false;
		switch (job)
		{
		case StatButtonJobs.ResetAllStats:
			showShowClearGameMessage = true;
			break;
		case StatButtonJobs.ResetAllStatsConfirmMessageYes:
			showShowClearGameMessage = true;
			showShowClearGameMessageConfirm = true;
			break;
		case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainYes:
			StatTracker.Instance.ClearStatsAndUnlocks();
			break;
		case StatButtonJobs.ResetAllStatsConfirmMessage:
		case StatButtonJobs.ResetAllStatsConfirmMessageNo:
		case StatButtonJobs.ResetAllStatsConfirmMessageSlash:
		case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgain:
		case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainNo:
		case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainSlash:
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Visible && initialized)
		{
			switch (job)
			{
			case StatButtonJobs.ResetAllStatsConfirmMessage:
				Show(showShowClearGameMessage);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageYes:
				Show(showShowClearGameMessage);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageNo:
				Show(showShowClearGameMessage);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageSlash:
				Show(showShowClearGameMessage);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgain:
				Show(showShowClearGameMessageConfirm);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainYes:
				Show(showShowClearGameMessageConfirm);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainNo:
				Show(showShowClearGameMessageConfirm);
				break;
			case StatButtonJobs.ResetAllStatsConfirmMessageConfirmAgainSlash:
				Show(showShowClearGameMessageConfirm);
				break;
			case StatButtonJobs.ResetAllStats:
				break;
			}
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
		showShowClearGameMessage = false;
		showShowClearGameMessageConfirm = false;
		switch (job)
		{
		}
	}

	protected void Show(bool show)
	{
		buttonText.enabled = show;
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
	}
}
