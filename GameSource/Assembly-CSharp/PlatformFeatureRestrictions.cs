public static class PlatformFeatureRestrictions
{
	public static bool HideOnlineContent
	{
		get
		{
			if (!IsNotConnected)
			{
				return IsUGCRestricted;
			}
			return true;
		}
	}

	public static bool IsUGCRestricted => false;

	public static bool IsChatRestricted => false;

	public static bool IsNotConnected => !GameSparksManager.Instance.Connected;

	public static bool MustHideAllUGC => false;
}
