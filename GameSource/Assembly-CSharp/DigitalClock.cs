using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DigitalClock : UIGraphic
{
	public Canvas TimeCanvas;

	public Text MainText;

	private Coroutine fadeCoroutine;

	private static StringBuilder stringBuilder = new StringBuilder(256);

	private void Start()
	{
		Reset();
	}

	public void Reset()
	{
		MainText.text = "0:00";
	}

	public void ShowSecondsAsTime(float timeInSeconds)
	{
		string timerString = GetTimerString(timeInSeconds);
		MainText.text = timerString;
	}

	public override void Show()
	{
		base.Show();
		MainText.enabled = true;
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		MainText.enabled = false;
	}

	public static string GetTimerString(float timeInSeconds)
	{
		stringBuilder.Length = 0;
		int num = Mathf.FloorToInt(timeInSeconds / 60f);
		float num2 = timeInSeconds - (float)(num * 60);
		stringBuilder.Append(num);
		stringBuilder.Append(":");
		stringBuilder.Append((num2 < 10f) ? "0" : "");
		stringBuilder.Append(num2.ToString("F2"));
		return stringBuilder.ToString();
	}

	public void SetColor(Color c)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		MainText.color = c;
	}

	public void FadeToColor(Color c, float time)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(fadeColor(c, time));
	}

	private IEnumerator fadeColor(Color c, float time)
	{
		Color startColor = MainText.color;
		float elapsed = 0f;
		while (elapsed < time)
		{
			elapsed += Time.unscaledDeltaTime;
			MainText.color = Color.Lerp(startColor, c, elapsed / time);
			yield return null;
		}
		MainText.color = c;
	}

	private void OnDestroy()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
	}
}
