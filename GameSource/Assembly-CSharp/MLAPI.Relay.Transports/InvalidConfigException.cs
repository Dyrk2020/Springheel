using System;

namespace MLAPI.Relay.Transports;

public class InvalidConfigException : SystemException
{
	public InvalidConfigException()
	{
	}

	public InvalidConfigException(string issue)
		: base(issue)
	{
	}
}
