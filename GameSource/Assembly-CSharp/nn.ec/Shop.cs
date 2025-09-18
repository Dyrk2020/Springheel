using System.Runtime.InteropServices;
using nn.account;

namespace nn.ec;

public static class Shop
{
	public static void ShowApplicationInformation()
	{
	}

	public static void ShowApplicationInformation(UserHandle selectedUser)
	{
	}

	public static void ShowApplicationInformation(ulong applicationId)
	{
	}

	public static void ShowApplicationInformation(ulong applicationId, UserHandle selectedUser)
	{
	}

	public static void ShowAddOnContentList()
	{
	}

	public static void ShowAddOnContentList(UserHandle selectedUser)
	{
	}

	public static void ShowAddOnContentList(ulong applicationId)
	{
	}

	public static void ShowAddOnContentList(ulong applicationId, UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionList()
	{
	}

	public static void ShowSubscriptionList(UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionList(ulong applicationId)
	{
	}

	public static void ShowSubscriptionList(ulong applicationId, UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionList(CourseId courseId)
	{
	}

	public static void ShowSubscriptionList(CourseId courseId, UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionList(ulong applicationId, CourseId courseId)
	{
	}

	public static void ShowSubscriptionList(ulong applicationId, CourseId courseId, UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionDetails(CourseId courseId, NsUid nsuid)
	{
	}

	public static void ShowSubscriptionDetails(CourseId courseId, NsUid nsuid, UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionManagement()
	{
	}

	public static void ShowSubscriptionManagement(UserHandle selectedUser)
	{
	}

	public static void ShowSubscriptionManagement(CourseId[] courseIdList)
	{
	}

	public static void ShowSubscriptionManagement(CourseId[] courseIdList, UserHandle selectedUser)
	{
	}

	public static void ShowConsumableItemList()
	{
	}

	public static void ShowConsumableItemList(UserHandle selectedUser)
	{
	}

	public static void ShowConsumableItemList(ulong applicationId)
	{
	}

	public static void ShowConsumableItemList(ulong applicationId, UserHandle selectedUser)
	{
	}

	public static void ShowConsumableItemDetail(ConsumableId consumableId, NsUid nsUid)
	{
	}

	public static void ShowConsumableItemDetail(ConsumableId consumableId, NsUid nsUid, UserHandle selectedUser)
	{
	}

	public static void ShowConsumableItemDetail(ulong applicationId, ConsumableId consumableId, NsUid nsUid)
	{
	}

	public static void ShowConsumableItemDetail(ulong applicationId, ConsumableId consumableId, NsUid nsUid, UserHandle selectedUser)
	{
	}

	public static void ShowShopProductDetails(NsUid nsuid)
	{
	}

	public static void ShowShopProductDetails(NsUid nsuid, UserHandle selectedUser)
	{
	}

	public static void ShowShopProductList(NsUid[] nsuidList, string listName)
	{
	}

	public static void ShowShopProductList(NsUid[] nsuidList, string listName, UserHandle selectedUser)
	{
	}

	public static void ShowEnterCodeScene()
	{
	}

	public static void ShowEnterCodeScene(UserHandle selectedUser)
	{
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ec_MakeNsUid")]
	public static extern NsUid MakeNsUid(string str);
}
