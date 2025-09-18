using UnityEngine;

public class UIPauseMenu : UIMenu
{
	private bool firstFrame = true;

	private void Update()
	{
		if (nextMenu != null)
		{
			Hide(useTransition: false, pauseOnHide: false);
			nextMenu.Show();
			nextMenu = null;
		}
		if (base.Visible)
		{
			firstFrame = false;
		}
	}

	public override void ReceiveEvent(InputEvent e)
	{
		if (!base.Visible || firstFrame || (AssociatedPlayer != 0 && (e.PlayerBitMask & (1 << AssociatedPlayer - 1)) == 0))
		{
			return;
		}
		if (e.Key == InputEvent.InputKey.Back && e.Valueb && e.Changed && !firstFrame)
		{
			Debug.Log("BACK");
			if (PreviousMenu != null)
			{
				GoToMenu(PreviousMenu);
			}
			else
			{
				Hide();
			}
		}
		else if (navMap != null)
		{
			navMap.HandleInputEvent(e);
		}
	}

	public override void Hide(bool useTransition = true)
	{
		Hide(useTransition);
	}

	public void Hide(bool useTransition = false, bool pauseOnHide = true)
	{
		base.Hide(useTransition);
		firstFrame = true;
		if (pauseOnHide && nextMenu == null)
		{
			Unpause();
		}
	}

	public override void Show(bool useTransition = false)
	{
		base.Show(useTransition);
		AkSoundEngine.PostEvent("UI_Pause_Game", base.gameObject);
		AkSoundEngine.PostEvent("Menu_Pause", base.gameObject);
	}

	public void Pause()
	{
		GameState.GetInstance().Paused = true;
		Show(useTransition: false);
	}

	public void Unpause()
	{
		GameState.GetInstance().Paused = false;
	}
}
