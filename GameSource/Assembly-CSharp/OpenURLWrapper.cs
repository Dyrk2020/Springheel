using UnityEngine;

public static class OpenURLWrapper
{
	public static void Open(string url)
	{
		Application.OpenURL(url);
	}
}
