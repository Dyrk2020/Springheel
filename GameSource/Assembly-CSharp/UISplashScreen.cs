using UnityEngine;

public abstract class UISplashScreen : MonoBehaviour
{
	public enum STATE
	{
		FADING_IN,
		FADING_OUT,
		SHOW,
		HIDE
	}

	public float FadeInTime;

	public float FadeOutTime;

	public float ShowTime;

	public float HideTime;

	public bool FadeOutAutomatically = true;

	public bool FadeInAutomatically;

	public UISplashScreen NextScreen;

	public STATE State = STATE.HIDE;

	public bool StartShowing;

	public UIMenu FadeToMenu;

	public string FadeInSoundEvent;

	public string FadeOutSoundEvent;

	public string FadeInSoundEventStarter;

	protected float stateTime;

	protected bool visible;

	protected bool SkipBool;

	protected virtual void Start()
	{
		if (!StartShowing)
		{
			visible = false;
			Hide();
		}
		else
		{
			Show();
		}
		Setup();
		SkipBool = false;
	}

	protected virtual void Update()
	{
		stateTime += Time.unscaledDeltaTime;
		if (SkipBool)
		{
			stateTime = 50f;
		}
		switch (State)
		{
		case STATE.FADING_IN:
			if (stateTime >= FadeInTime)
			{
				Show();
			}
			else
			{
				Fade(stateTime / FadeInTime);
			}
			break;
		case STATE.SHOW:
			if (stateTime >= ShowTime && FadeOutAutomatically)
			{
				FadeOut();
			}
			else
			{
				Fade(1f);
			}
			break;
		case STATE.FADING_OUT:
			if (stateTime >= FadeOutTime)
			{
				Hide();
			}
			else
			{
				Fade(1f - stateTime / FadeOutTime);
			}
			break;
		case STATE.HIDE:
			if (stateTime >= HideTime && FadeInAutomatically)
			{
				FadeIn();
			}
			Fade(0f);
			break;
		}
	}

	public abstract void Setup();

	public virtual void FadeIn()
	{
		stateTime = 0f;
		State = STATE.FADING_IN;
		if (FadeInSoundEventStarter != "")
		{
			AkSoundEngine.PostEvent(FadeInSoundEventStarter, base.gameObject);
		}
		if (FadeInSoundEvent != "")
		{
			AkSoundEngine.PostEvent(FadeInSoundEvent, base.gameObject);
		}
	}

	public virtual void FadeOut()
	{
		stateTime = 0f;
		State = STATE.FADING_OUT;
		if (FadeOutSoundEvent != "")
		{
			AkSoundEngine.PostEvent(FadeOutSoundEvent, base.gameObject);
		}
		if (FadeToMenu != null)
		{
			FadeToMenu.Show();
		}
	}

	public virtual void Show()
	{
		stateTime = 0f;
		State = STATE.SHOW;
		visible = true;
	}

	public virtual void Hide()
	{
		stateTime = 0f;
		State = STATE.HIDE;
		if (visible && NextScreen != null)
		{
			visible = false;
			NextScreen.FadeIn();
			NextScreen.stateTime = 0f;
		}
	}

	public virtual void Fade(float alpha)
	{
	}

	public void Skip()
	{
		SkipBool = true;
	}

	public void SkipOff()
	{
		stateTime = 0f;
		SkipBool = false;
	}
}
