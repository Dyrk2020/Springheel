using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TabletLoadingScreen : TabletScreen
{
	public CanvasGroup logoCanvasGroup;

	public Image logoImage;

	public Image loadFillImage;

	private IEnumerator anim;

	public override void OnTransitionInBegin()
	{
		base.OnTransitionInBegin();
		AkSoundEngine.PostEvent("UI_UPad_Loading_Start", base.gameObject);
	}

	public override void OnTransitionInEnd()
	{
		base.OnTransitionInEnd();
		anim = AnimateLoading();
	}

	public override void Update()
	{
		base.Update();
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
	}

	private void ResetScreen()
	{
		canvasGroup.alpha = 1f;
		logoCanvasGroup.alpha = 1f;
		loadFillImage.fillAmount = 0f;
	}

	private IEnumerator AnimateLoading()
	{
		float t = 0f;
		float initWait = 0.5f;
		while (t < initWait)
		{
			t += Time.deltaTime;
			yield return null;
		}
		t = 0f;
		float fillTime = 0.3f;
		while (t < fillTime)
		{
			t += Time.deltaTime;
			loadFillImage.fillAmount = Mathf.Clamp01(t / fillTime);
			yield return null;
		}
		loadFillImage.fillAmount = 1f;
		t = 0f;
		float disappearTime = 0.2f;
		while (t < disappearTime)
		{
			t += Time.deltaTime;
			logoCanvasGroup.alpha = Mathf.Clamp01(1f - t / disappearTime);
			yield return null;
		}
		logoCanvasGroup.alpha = 0f;
		Tablet componentInParent = GetComponentInParent<Tablet>();
		componentInParent.TransitionTo(componentInParent.emptyScreen);
		componentInParent.OpenBurgerMenu(null);
	}
}
