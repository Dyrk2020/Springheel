namespace BrainCloud;

public struct RelayConnectOptions
{
	public bool ssl;

	public string host;

	public int port;

	public string passcode;

	public string lobbyId;

	public RelayConnectOptions(bool in_ssl, string in_host, int in_port, string in_passcode, string in_lobbyId)
	{
		ssl = in_ssl;
		host = in_host;
		port = in_port;
		passcode = in_passcode;
		lobbyId = in_lobbyId;
	}
}
