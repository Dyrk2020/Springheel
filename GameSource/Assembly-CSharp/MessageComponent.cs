using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MessageComponent : MonoBehaviour
{
	protected CanvasGroup canvasGroup;

	public Text mainText;

	protected float displayMessageTimer;

	private string associatedScene;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
	}

	private void Update()
	{
		if (displayMessageTimer > 0f)
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, GameSettings.GetInstance().messageComponentFadeSpeed * Time.unscaledDeltaTime);
			displayMessageTimer -= Time.unscaledDeltaTime;
		}
		else
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, GameSettings.GetInstance().messageComponentFadeSpeed * Time.unscaledDeltaTime);
		}
		if (associatedScene != null && SceneManager.GetActiveScene().name != associatedScene)
		{
			canvasGroup.alpha = 0f;
			displayMessageTimer = 0f;
		}
	}

	public void DisplayMessage(string textToDisplay, float duration, bool tieToCurrentScene)
	{
		if (duration > displayMessageTimer)
		{
			displayMessageTimer = duration;
		}
		if (mainText != null)
		{
			mainText.text = textToDisplay;
		}
		associatedScene = (tieToCurrentScene ? SceneManager.GetActiveScene().name : null);
	}
}
