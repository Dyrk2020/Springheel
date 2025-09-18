using UnityEngine;

public class AllowedOnPlatform : MonoBehaviour
{
	public static int WINDOWS = 1;

	public static int OSX = 2;

	public static int LINUX = 4;

	public static int XBOX = 8;

	public static int PS4 = 16;

	public static int ANDROID = 32;

	public static int SWITCH = 64;

	public static int WEGAME = 128;

	public static int WINDOWS_GDK = 256;

	public static int ALL = WINDOWS | OSX | LINUX | XBOX | PS4 | ANDROID | SWITCH | WEGAME | WINDOWS_GDK;

	public int allowedPlatform;

	public int onPlatform = ALL;

	public bool GetAllowed => (onPlatform & allowedPlatform) != 0;

	private void Awake()
	{
		onPlatform = WINDOWS;
		if ((onPlatform & allowedPlatform) == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
