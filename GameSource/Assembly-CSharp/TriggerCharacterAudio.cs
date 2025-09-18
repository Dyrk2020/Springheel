using UnityEngine;

public class TriggerCharacterAudio : MonoBehaviour
{
	public string AudioEventName;

	public void OnTriggerEnter2D(Collider2D c)
	{
		if (c.gameObject.CompareTag("Player_Body"))
		{
			Character componentInParent = c.gameObject.GetComponentInParent<Character>();
			if (componentInParent != null && !componentInParent.Dead && !componentInParent.Dying)
			{
				componentInParent.audioEvent(AudioEventName, componentInParent.gameObject);
			}
		}
	}
}
