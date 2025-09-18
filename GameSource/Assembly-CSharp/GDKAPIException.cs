using System;

public class GDKAPIException : Exception
{
	private int mHResult;

	public new int HResult => mHResult;

	public GDKAPIException(string message, int hresult)
		: base(message)
	{
		mHResult = hresult;
	}
}
