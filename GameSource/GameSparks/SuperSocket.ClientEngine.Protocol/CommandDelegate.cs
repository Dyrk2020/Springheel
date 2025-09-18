namespace SuperSocket.ClientEngine.Protocol;

public delegate void CommandDelegate<TClientSession, TCommandInfo>(TClientSession session, TCommandInfo commandInfo);
