using System;
using UnityEngine;
using UnityEngine.UI;

public class HoldBToGiveUp : MonoBehaviour
{
	public Image buttonImage;

	public Image buttonbackground;

	public Text text;

	public CanvasGroup canvasGroup;

	public float FadeSpeedMultiplier = 2f;

	protected bool FadeActive;

	protected bool pulsing;

	protected float fadeT;

	public AnimationCurve fadeInCurve;

	public AnimationCurve fadeInCurveOff;

	public AnimationCurve fadeInCurveFast;

	public MultiControllerButton multiControllerButton;

	private float lastScale = 1f;

	public float maxOpacity = 1f;

	private float target;

	public bool Visible { get; protected set; }

	private void Awake()
	{
		buttonImage.type = Image.Type.Filled;
		buttonImage.fillMethod = Image.FillMethod.Radial360;
		buttonImage.fillOrigin = 0;
		buttonImage.fillAmount = 1f;
		buttonImage.fillClockwise = true;
		InstantHide();
	}

	public void Hide(bool instanthide = false)
	{
		Visible = false;
		pulsing = false;
	}

	public void InstantHide()
	{
		Visible = false;
		FadeActive = false;
		pulsing = false;
		text.enabled = false;
		buttonImage.enabled = false;
		buttonbackground.enabled = false;
		multiControllerButton.Hidden = true;
	}

	public void Show()
	{
		text.enabled = true;
		buttonImage.enabled = true;
		buttonbackground.enabled = true;
		multiControllerButton.Hidden = false;
		Visible = true;
		FadeActive = true;
	}

	public void SetPulse(bool pulse)
	{
		pulsing = pulse;
	}

	public void SetFillAmount(float filledAmount)
	{
		buttonImage.fillAmount = filledAmount;
	}

	private void Update()
	{
		if (pulsing)
		{
			target = Mathf.Sin(Time.unscaledTime * MathF.PI) / 4f + 0.75f;
		}
		else if (Visible)
		{
			target = 1f;
		}
		else
		{
			target = 0f;
		}
		if (FadeActive)
		{
			float num = 1f;
			fadeT = Mathf.MoveTowards(fadeT, target, Time.unscaledDeltaTime);
			if (fadeT < target)
			{
				AnimationCurve animationCurve = ((GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE && GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY) ? fadeInCurve : fadeInCurveFast);
				num = animationCurve.Evaluate(fadeT);
			}
			else
			{
				num = fadeInCurveOff.Evaluate(fadeT);
			}
			canvasGroup.alpha = Mathf.Min(num, maxOpacity);
		}
		else
		{
			canvasGroup.alpha = maxOpacity;
		}
		if (!Visible && FadeActive && canvasGroup.alpha <= 0f)
		{
			text.enabled = false;
			buttonImage.enabled = false;
			buttonbackground.enabled = false;
			multiControllerButton.Hidden = true;
			FadeActive = false;
		}
	}

	public void SetLocalController(Controller controller)
	{
		multiControllerButton.ForceController(controller);
	}

	private void LateUpdate()
	{
		float num = 1f / base.transform.parent.localScale.x;
		if (lastScale != num)
		{
			lastScale = num;
			base.transform.localScale = new Vector3(num, num, num);
		}
	}
}
