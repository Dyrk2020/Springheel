using I2.Loc;

public class PickableInterfaceButton : PickableButton
{
	public enum InterfaceButtonJobs
	{
		MainMenuAnnoucement = 1
	}

	public InterfaceButtonJobs buttonJob;

	protected override void Start()
	{
		Enable(onOff: true);
		initialized = true;
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (buttonJob == InterfaceButtonJobs.MainMenuAnnoucement)
		{
			OpenURLWrapper.Open(LocalizationManager.GetTranslation(MainMenuAnnouncement.locKey_LinkURL));
		}
	}

	public void Show()
	{
	}

	public void Hide()
	{
	}
}
