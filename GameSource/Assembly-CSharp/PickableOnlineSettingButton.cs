using UnityEngine;

public class PickableOnlineSettingButton : PickableButton
{
	public enum OnlineSettingsButtonJobs
	{
		NameVisibilityButton,
		ChatAndEmotesButton,
		PlayerMute,
		PlayerKick,
		AreYouSureYes,
		AreYouSureNo,
		AreYouSureSlash,
		AreYouSureText,
		PlayerName,
		PlayerReport,
		AFKKicker,
		AFKKickerLabel,
		CameraFollow
	}

	public OnlineSettingsButtonJobs job;

	public OnlinePlayerUI relatedOnlinePlayerUI;

	public OnlinePlayerUISystem relatedOnlinePlayerUISystem;

	protected void Show(bool show)
	{
		if ((bool)buttonText)
		{
			buttonText.enabled = show;
		}
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if ((bool)image)
		{
			image.enabled = show;
		}
		if ((bool)sprite)
		{
			sprite.enabled = show;
		}
	}
}
