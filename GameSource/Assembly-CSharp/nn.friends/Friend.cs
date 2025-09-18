using System.Runtime.InteropServices;
using nn.account;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential, Size = 512)]
public struct Friend
{
	public NetworkServiceAccountId GetAccountId()
	{
		return default(NetworkServiceAccountId);
	}

	public Nickname GetNickname()
	{
		return default(Nickname);
	}

	public FriendPresence GetPresence()
	{
		return default(FriendPresence);
	}

	public Result GetProfileImage(ref long outSize, byte[] buffer)
	{
		return default(Result);
	}

	public bool IsFavorite()
	{
		return false;
	}

	public bool IsNewly()
	{
		return false;
	}

	public bool IsValid()
	{
		return false;
	}

	public Result Update()
	{
		return default(Result);
	}
}
