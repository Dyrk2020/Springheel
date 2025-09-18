using System.Runtime.InteropServices;
using nn.account;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential, Size = 256)]
public struct Profile
{
	public NetworkServiceAccountId GetAccountId()
	{
		return default(NetworkServiceAccountId);
	}

	public Nickname GetNickname()
	{
		return default(Nickname);
	}

	public Result GetProfileImageUrl(ref string outUrl, ImageSize imageSize)
	{
		return default(Result);
	}

	public bool IsValid()
	{
		return false;
	}
}
