using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class ScorecardPopupMessage : UIGraphic
{
	public Canvas Message;

	public Text NoWinners;

	public Text NoWinnersShadow;

	public Text AllWinners;

	public Text AllWinnersShadow;

	public Text NoPoints;

	public Text NoPointsShadow;

	public Text ExceptCoin;

	public Text ExceptCoinShadow;

	public Text Exceptions;

	public Text ExceptionsShadow;

	public bool AllWin;

	public bool NoWin;

	public bool PointsAwarded;

	public bool coinPoints;

	public bool racePoints;

	public bool MatchHasWinnerPoints = true;

	public Animator animator;

	public float scoreboardDelay = 5f;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
		animator.SetBool("Enabled", value: false);
	}

	public override void Show()
	{
		base.Show();
		Message.enabled = false;
		AllWinners.enabled = false;
		AllWinnersShadow.enabled = false;
		NoWinners.enabled = false;
		NoWinnersShadow.enabled = false;
		NoPoints.enabled = false;
		NoPointsShadow.enabled = false;
		ExceptCoin.enabled = false;
		ExceptCoinShadow.enabled = false;
		Exceptions.enabled = false;
		ExceptionsShadow.enabled = false;
		bool flag = false;
		if (MatchHasWinnerPoints)
		{
			if (!racePoints)
			{
				if (AllWin)
				{
					AllWinners.enabled = true;
					AllWinnersShadow.enabled = true;
					flag = true;
				}
				else if (NoWin)
				{
					NoWinners.enabled = true;
					NoWinnersShadow.enabled = true;
					flag = true;
				}
				else if (!PointsAwarded && !coinPoints)
				{
					NoPoints.enabled = true;
					NoPointsShadow.enabled = true;
					Message.enabled = true;
				}
			}
		}
		else if (!PointsAwarded && !coinPoints)
		{
			NoPoints.enabled = true;
			NoPointsShadow.enabled = true;
			Message.enabled = true;
		}
		if (flag)
		{
			Message.enabled = true;
			if (PointsAwarded)
			{
				Exceptions.enabled = true;
				ExceptionsShadow.enabled = true;
			}
			else if (coinPoints)
			{
				ExceptCoin.enabled = true;
				ExceptCoinShadow.enabled = true;
			}
		}
		animator.SetBool("Enabled", value: true);
	}

	public override void Hide(bool forceQuickHide = false)
	{
		animator.SetBool("Enabled", value: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(SpecialUIEvent) && (e as SpecialUIEvent).SpecialUIType == SpecialUIEvent.SpecialUI.SCOREBOARDDELAY)
		{
			StartCoroutine(DelayScoreboard());
		}
	}

	private IEnumerator DelayScoreboard()
	{
		animator.SetBool("DelayScoreboard", value: true);
		float delayTimer = 0f;
		do
		{
			delayTimer += Time.unscaledDeltaTime;
			yield return null;
		}
		while (delayTimer < scoreboardDelay);
		animator.SetBool("DelayScoreboard", value: false);
	}
}
