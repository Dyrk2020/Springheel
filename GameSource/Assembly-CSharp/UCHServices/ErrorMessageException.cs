using System;
using Relay;

namespace UCHServices;

public class ErrorMessageException : Exception
{
	private ServiceError mErrorMessage;

	public ServiceError ErrorMessage => mErrorMessage;

	public override string Message => $"Error code = {mErrorMessage.ErrorCode}";

	public ErrorMessageException(ServiceError aError)
	{
		mErrorMessage = aError;
	}

	public override string ToString()
	{
		return mErrorMessage.ToString();
	}
}
