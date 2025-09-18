using System.Collections;
using UnityEngine;

public class quickstarttreehouse : MonoBehaviour
{
	public bool startNow;

	private IEnumerator perform;

	private void Start()
	{
	}

	private void Update()
	{
		if (perform != null)
		{
			if (!perform.MoveNext())
			{
				perform = null;
			}
		}
		else if (startNow)
		{
			startNow = false;
			perform = Perform();
		}
	}

	private IEnumerator Perform()
	{
		Debug.Log("Finding rule pick cursor...");
		RulePickCursor rulePickCursor;
		do
		{
			rulePickCursor = Object.FindObjectOfType<RulePickCursor>();
			if (rulePickCursor == null)
			{
				yield return null;
			}
		}
		while (rulePickCursor == null);
		PickableMainMenuButton startButton = null;
		do
		{
			PickableMainMenuButton[] array = Object.FindObjectsOfType<PickableMainMenuButton>();
			foreach (PickableMainMenuButton pickableMainMenuButton in array)
			{
				if (pickableMainMenuButton.job == PickableMainMenuButton.MenuButtonJobs.StartGame)
				{
					startButton = pickableMainMenuButton;
					break;
				}
			}
			if (startButton == null)
			{
				yield return null;
			}
		}
		while (startButton == null);
		Debug.Log("Pressing start button...");
		startButton.OnAccept(rulePickCursor);
	}
}
