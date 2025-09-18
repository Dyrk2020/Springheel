using UnityEngine;

public class MessagingSystem : MonoBehaviour
{
	public MessageComponent RightCorner;

	public MessageComponent Center;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
