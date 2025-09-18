using System.Runtime.InteropServices;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential, Size = 224)]
public struct FriendPresence
{
	public string GetDescription()
	{
		return string.Empty;
	}

	public ApplicationInfo GetLastPlayedApplication()
	{
		return default(ApplicationInfo);
	}

	public long GetLastUpdatePosixTime()
	{
		return 0L;
	}

	public PresenceStatus GetStatus()
	{
		return PresenceStatus.Offline;
	}

	public bool IsSamePresenceGroupApplication()
	{
		return false;
	}
}
