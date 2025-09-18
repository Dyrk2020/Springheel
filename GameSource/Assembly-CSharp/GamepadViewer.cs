using System.Collections.Generic;
using UnityEngine;

public class GamepadViewer : MonoBehaviour
{
	public GameObject[] gamepadHolders;

	public List<GameObject> allowedGamePads = new List<GameObject>();

	public int currentGamePad;

	private void Start()
	{
		GameObject[] array = gamepadHolders;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(value: false);
			AllowedOnPlatform component = gameObject.GetComponent<AllowedOnPlatform>();
			if (component != null && component.GetAllowed)
			{
				allowedGamePads.Add(gameObject);
			}
		}
		allowedGamePads[currentGamePad].SetActive(value: true);
	}

	public void ShowNext()
	{
		allowedGamePads[currentGamePad].SetActive(value: false);
		currentGamePad++;
		if (currentGamePad >= allowedGamePads.Count)
		{
			currentGamePad = 0;
		}
		allowedGamePads[currentGamePad].SetActive(value: true);
	}

	public void ShowPrevious()
	{
		allowedGamePads[currentGamePad].SetActive(value: false);
		currentGamePad--;
		if (currentGamePad < 0)
		{
			currentGamePad = allowedGamePads.Count - 1;
		}
		allowedGamePads[currentGamePad].SetActive(value: true);
	}
}
