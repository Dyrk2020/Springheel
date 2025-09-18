using UnityEngine;

public static class Util_GameObject
{
	public static GameObject AddPrefabAsChild(this GameObject self, Object prefab)
	{
		if (prefab is Component)
		{
			GameObject gameObject = Object.Instantiate(((Component)prefab).gameObject);
			gameObject.transform.SetParent(self.transform, worldPositionStays: false);
			return gameObject;
		}
		GameObject obj = (GameObject)Object.Instantiate(prefab);
		obj.transform.SetParent(self.transform, worldPositionStays: false);
		return obj;
	}

	public static T AddPrefabAsChild<T>(this GameObject self, Object prefab) where T : Component
	{
		return self.AddPrefabAsChild(prefab).GetComponent<T>();
	}
}
