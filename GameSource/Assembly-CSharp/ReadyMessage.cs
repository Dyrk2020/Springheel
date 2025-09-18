using System.Collections;
using GameEvent;
using UnityEngine;

public class ReadyMessage : UIGraphic, IGameEventListener
{
	public bool WaitForPlayer;

	public GameObject PressAObject;

	public GameObject HoldBGiveUpObject;

	public Canvas canvas;

	public GameObject LocalChallengeTimeWarning;

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		if (forceQuickHide)
		{
			canvasGroup.alpha = 0f;
		}
		else
		{
			StartCoroutine(HideObjectsAtAlphaZero());
		}
		LocalChallengeTimeWarning.SetActive(value: false);
	}

	public override void Show()
	{
		Show(showChallengeWarning: false);
	}

	public void Show(bool showChallengeWarning)
	{
		base.Show();
		_ = WaitForPlayer;
		LocalChallengeTimeWarning.SetActive(showChallengeWarning);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(StartPhaseEvent))
		{
			Hide();
		}
	}

	private IEnumerator HideObjectsAtAlphaZero()
	{
		while (canvasGroup.alpha != 0f)
		{
			yield return null;
		}
	}

	public void SetupForChallengeMode()
	{
		PressAObject.SetActive(value: true);
		HoldBGiveUpObject.SetActive(value: true);
	}

	public void SetupForVersusMode()
	{
		PressAObject.SetActive(WaitForPlayer);
		HoldBGiveUpObject.SetActive(value: false);
	}
}
