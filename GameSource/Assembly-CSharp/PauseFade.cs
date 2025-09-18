using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class PauseFade : MonoBehaviour, IGameEventListener
{
	public Image SolidImage;

	public Text PauseText;

	public Color PauseColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public Color FadeColor = new Color(0f, 0f, 0f, 0f);

	public Color PauseTextColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public Color FadeTextColor = new Color(0f, 0f, 0f, 0f);

	protected bool fading;

	private float fadeAmt;

	public virtual void Start()
	{
		ChangeListener(addRemove: true);
		SolidImage.color = FadeColor;
		SolidImage.enabled = false;
		PauseText.color = FadeTextColor;
		PauseText.enabled = false;
	}

	public void OnDestroy()
	{
		ChangeListener(addRemove: false);
	}

	public virtual void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<PauseEvent>(this, addRemove);
	}

	private IEnumerator Fade(float time, Color endColor, Color gridendColor, bool fadingIn)
	{
		SolidImage.enabled = true;
		PauseText.enabled = true;
		Color startColor = SolidImage.color;
		Color gridstartColor = PauseText.color;
		float timer = fadeAmt * time;
		if (!fading)
		{
			timer = time - timer;
		}
		_ = timer / time;
		while (fading == fadingIn && timer < time)
		{
			float t = (fadeAmt = timer / time);
			if (!fading)
			{
				fadeAmt = 1f - fadeAmt;
			}
			SolidImage.color = Color.Lerp(startColor, endColor, t);
			PauseText.color = Color.Lerp(gridstartColor, gridendColor, t);
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		if (fading == fadingIn)
		{
			SolidImage.color = endColor;
			SolidImage.enabled = fadingIn;
			PauseText.color = gridendColor;
			PauseText.enabled = fadingIn;
			fadeAmt = (fadingIn ? 1 : 0);
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				fading = true;
				StartCoroutine(Fade(1f, PauseColor, PauseTextColor, fadingIn: true));
			}
			else
			{
				fading = false;
				StartCoroutine(Fade(0.4f, FadeColor, FadeTextColor, fadingIn: false));
			}
		}
	}
}
