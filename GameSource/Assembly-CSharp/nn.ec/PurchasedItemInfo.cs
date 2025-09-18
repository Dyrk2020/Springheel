using nn.account;

namespace nn.ec;

public struct PurchasedItemInfo
{
	public enum Type
	{
		Subscription,
		Consumable
	}

	public Type type;

	public NetworkServiceAccountId nsaId;

	internal CourseId _courseId;

	public CourseId GetCourseId()
	{
		return default(CourseId);
	}
}
