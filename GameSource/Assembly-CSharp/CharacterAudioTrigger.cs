using UnityEngine;

public class CharacterAudioTrigger : MonoBehaviour
{
	public string SFXEnterEventName;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		Character componentInParent = collider.GetComponentInParent<Character>();
		if (componentInParent != null && componentInParent.hasAuthority)
		{
			componentInParent.audioEvent(SFXEnterEventName, componentInParent.gameObject, ignoreGhostZombie: true);
		}
	}
}
