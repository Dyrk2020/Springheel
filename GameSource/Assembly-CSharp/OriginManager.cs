using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OriginManager : MonoBehaviour
{
	public static OriginManager Instance = null;

	public static bool initialized = false;

	public static HashSet<ulong> blockedUserIds = new HashSet<ulong>();

	public static HashSet<int> unlockedAchievements = new HashSet<int>();

	public static HashSet<int> queuedAchievementUnlocks = new HashSet<int>();

	public static string AchievementCode = "78112439-4ad3-427d-8cde-7f36c576afdf";

	public static string SecurityKey = "{82448BC5-282B-4FDF-A862-3E3D9C6833E2}";

	public static string ContentId = "198132";

	public static string MultiplayerId = "198132";

	public bool useDebugOverlay = true;

	public Canvas originDebugCanvas;

	public Text originDebugText;

	public int debugPort;

	public bool useDebugPort;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
