using UnityEngine;
using UnityEngine.SceneManagement;

public class UserMessageManager : MonoBehaviour
{
	public enum UserMsgPriority
	{
		lo,
		hi
	}

	private static UserMessageManager instance;

	public MessagingSystem MessageHolderPrefab;

	private MessagingSystem UIMsg;

	public static UserMessageManager Instance
	{
		get
		{
			if (instance == null)
			{
				Debug.LogError("UserMessageInstance is null!");
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (SceneManager.GetActiveScene().name != "Savefile Viewer")
		{
			UIMsg = Object.Instantiate(MessageHolderPrefab);
		}
	}

	public void UserMessage(string text, bool tieToCurrentScene = false)
	{
		UserMessage(text, 2f, UserMsgPriority.lo, tieToCurrentScene);
	}

	public void UserMessage(string text, float duration, UserMsgPriority priority, bool tiedToCurrentScene)
	{
		if (text.NullOrEmpty())
		{
			Debug.LogError("Empty UserMessage detected!");
			return;
		}
		switch (priority)
		{
		case UserMsgPriority.lo:
			UIMsg.RightCorner.DisplayMessage(text, duration, tiedToCurrentScene);
			break;
		case UserMsgPriority.hi:
			UIMsg.Center.DisplayMessage(text, duration, tiedToCurrentScene);
			break;
		}
	}
}
