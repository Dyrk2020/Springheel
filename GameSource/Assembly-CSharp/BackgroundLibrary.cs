using UnityEngine;

public class BackgroundLibrary : MonoBehaviour
{
	private static BackgroundLibrary instance;

	public CustomBackground[] Backgrounds;

	public static BackgroundLibrary Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("BackgroundLibrary", typeof(BackgroundLibrary)).GetComponent<BackgroundLibrary>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public CustomBackground GetBackground(BackgroundType background)
	{
		for (int i = 0; i != Backgrounds.Length; i++)
		{
			if (Backgrounds[i].background == background)
			{
				return Backgrounds[i];
			}
		}
		return null;
	}
}
