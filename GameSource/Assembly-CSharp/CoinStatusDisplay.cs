using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class CoinStatusDisplay : MonoBehaviour, IGameEventListener
{
	public Image Coin;

	public Image Checkmark;

	public Canvas IconCanvas;

	public CanvasGroup CanvasGroup;

	public float FadeTime = 0.1f;

	private int totalCoins;

	private int collectedCoins;

	private bool showing;

	private void Start()
	{
		totalCoins = Object.FindObjectsOfType<Coin>().Length;
		collectedCoins = 0;
		GameEventManager.ChangeListener<CoinPickupEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<CoinPickupEvent>(this, adding: false);
	}

	public void Show()
	{
		Coin.enabled = true;
		Checkmark.enabled = true;
		SetAlpha(0f);
		FadeToAlpha(1f);
		showing = true;
	}

	public void Reset()
	{
		Coin.enabled = false;
		Checkmark.enabled = false;
		showing = false;
		collectedCoins = 0;
		SetAlpha(0f);
	}

	public void SetAlpha(float alpha)
	{
		CanvasGroup.alpha = alpha;
	}

	public void FadeToAlpha(float targetAlpha)
	{
		FadeToAlpha(targetAlpha, FadeTime);
	}

	public void FadeToAlpha(float targetAlpha, float fadeTime)
	{
		StartCoroutine(FadeAlphaToCourotine(targetAlpha, fadeTime));
	}

	private IEnumerator FadeAlphaToCourotine(float targetAlpha, float fadeTime)
	{
		while (!Mathf.Approximately(CanvasGroup.alpha, targetAlpha))
		{
			CanvasGroup.alpha = Mathf.MoveTowards(CanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime / fadeTime);
			yield return null;
		}
		CanvasGroup.alpha = targetAlpha;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e is CoinPickupEvent coinPickupEvent))
		{
			return;
		}
		if (coinPickupEvent.PickedUp)
		{
			collectedCoins++;
			if (collectedCoins == totalCoins)
			{
				Show();
			}
			return;
		}
		collectedCoins--;
		if (showing)
		{
			showing = false;
			FadeToAlpha(0f, 1f);
		}
	}
}
