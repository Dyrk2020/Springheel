using UnityEngine;

public class EmoteAnimationEvent : MonoBehaviour
{
	protected EmoteSystem emoteSystem;

	public void Start()
	{
		emoteSystem = GetComponentInParent<EmoteSystem>();
	}

	public void HideEmoteUI()
	{
		emoteSystem.HideEmoteUI();
	}
}
