public class WwiseSuspender
{
	private static bool muted;

	public static bool Muted => muted;

	public static void MuteAudio()
	{
		if (ControllerMonitor.Instance.IsMainControllerSet && PlayerManager.GetInstance().FirstUserLoggedIn && StatTracker.Instance.GetSaveFileDataForMainUser().BackgroundAudio)
		{
			muted = false;
			return;
		}
		muted = true;
		AkSoundEngine.SetRTPCValue("MUS_volume", 0f);
		AkSoundEngine.SetRTPCValue("SFX_volume", 0f);
	}

	public static void UnmuteAudio()
	{
		muted = false;
		AkSoundEngine.SetRTPCValue("MUS_volume", GameSettings.Music * 100f);
		AkSoundEngine.SetRTPCValue("SFX_volume", GameSettings.Sound * 100f);
	}
}
