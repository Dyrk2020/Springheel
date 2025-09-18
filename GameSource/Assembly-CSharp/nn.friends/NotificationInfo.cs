using nn.account;

namespace nn.friends;

public struct NotificationInfo
{
	public NotificationType type;

	public NetworkServiceAccountId accountId;

	public override string ToString()
	{
		return $"{type} {accountId}";
	}
}
