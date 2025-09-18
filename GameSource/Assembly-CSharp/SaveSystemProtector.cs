using UnityEngine;

public class SaveSystemProtector : MonoBehaviour
{
	private float protectionTime;

	private static SaveSystemProtector instance;

	public static bool Protected => Instance.protectionTime > 0f;

	public static SaveSystemProtector Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("SaveSystemProtector").AddComponent<SaveSystemProtector>();
			}
			return instance;
		}
	}

	public static bool WaitingForSavefileOperations => false;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			Debug.Log("SaveSystemProtector shutting down...");
		}
	}

	private void Update()
	{
		if (protectionTime > 0f)
		{
			protectionTime -= Time.unscaledDeltaTime;
			if (protectionTime <= 0f)
			{
				protectionTime = 0f;
			}
		}
	}

	public static void Protect(float seconds = 5f)
	{
		Instance.protectionTime = seconds;
	}

	public static void UnProtect()
	{
		Instance.protectionTime = 0f;
	}
}
