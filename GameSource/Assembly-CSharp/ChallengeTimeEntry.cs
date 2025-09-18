using UnityEngine;
using UnityEngine.UI;

public class ChallengeTimeEntry : MonoBehaviour
{
	public Text numberText;

	public Text timeText;

	public Text playerNamesText;

	public void Initialize(int number, float time, string playerNames, Color TextColor)
	{
		numberText.text = number + ".";
		timeText.text = HighscoreDisplayEntry.GetTimeString(time);
		playerNamesText.text = playerNames;
		numberText.color = TextColor;
		timeText.color = TextColor;
		playerNamesText.color = TextColor;
	}
}
