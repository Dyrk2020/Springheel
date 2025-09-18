using UnityEngine;

public class ThumbnailGenerator : MonoBehaviour
{
	private static bool nextLevel;

	private static int codeIndex = 1;

	private static float startTime;

	public static void ThumbnailLoaded()
	{
		nextLevel = true;
	}
}
