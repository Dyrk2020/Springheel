using UnityEngine;

public static class Util_Transform
{
	public static void DestroyAllChildren(this Transform self)
	{
		foreach (Transform item in self)
		{
			Object.Destroy(item.gameObject);
		}
	}
}
