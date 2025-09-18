public class NetworkTerminal : GameRuleProp, InputReceiver
{
	public NetworkUI NetworkMenu;

	private Controller useController;

	public Controller UseController
	{
		get
		{
			return useController;
		}
		set
		{
			if (useController != null)
			{
				useController.RemoveReceiver(this);
			}
			useController = value;
			if (useController != null)
			{
				useController.AddReceiver(this);
			}
		}
	}

	public override bool Use(LobbyPlayer lobbyPlayer, InputEvent.InputKey usedInputKey)
	{
		if (!base.Use(lobbyPlayer, usedInputKey))
		{
			return false;
		}
		UseController = lobbyPlayer.LocalPlayer.UseController;
		NetworkMenu.Show();
		return true;
	}

	public override void Release(bool unFreeze = true)
	{
		base.Release(unFreeze);
		UseController = null;
		NetworkMenu.Hide();
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.Key == InputEvent.InputKey.Back || e.Key == InputEvent.InputKey.Esc || e.Key == InputEvent.InputKey.Start)
		{
			Release();
		}
	}
}
