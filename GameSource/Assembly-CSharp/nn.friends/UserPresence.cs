using System.Runtime.InteropServices;
using nn.account;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential, Size = 224)]
public struct UserPresence
{
	public Result Initialize(Uid uid)
	{
		return default(Result);
	}

	public Result Initialize()
	{
		return default(Result);
	}

	public void Clear()
	{
	}

	public Result Commit()
	{
		return default(Result);
	}

	public void DeclareOpenOnlinePlaySession()
	{
	}

	public void DeclareCloseOnlinePlaySession()
	{
	}

	public Result SetDescription(string description)
	{
		return default(Result);
	}
}
