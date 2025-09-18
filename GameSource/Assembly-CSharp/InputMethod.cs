public interface InputMethod
{
	void AddPlayer(int player);

	void RemovePlayer(int player);

	void ClearPlayers();

	bool ControlsPlayer(int player);

	int GetControlMask();

	void AddReceiver(InputReceiver r);

	void RemoveReceiver(InputReceiver r);

	void ClearReceivers();

	void Notify(InputEvent e);
}
