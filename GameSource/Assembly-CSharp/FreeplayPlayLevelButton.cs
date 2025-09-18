using UnityEngine.Events;

public class FreeplayPlayLevelButton : PickableButton
{
	public FreeplayModeSwitchButton ModeButton;

	public UnityEvent OnClick;

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (ModeButton.CurrentMode != GameSettings.GetInstance().GameMode && inventoryBook.ShowingOnHost)
		{
			GameSettings.GetInstance().GameMode = ModeButton.CurrentMode;
			OnClick.Invoke();
		}
	}
}
