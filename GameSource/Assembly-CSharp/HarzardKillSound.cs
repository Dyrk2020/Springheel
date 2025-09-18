using UnityEngine;

public class HarzardKillSound : MonoBehaviour
{
	public string EventName;

	public bool UseCharacterGameObject;

	private void playSound(Character character)
	{
		if (!EventName.Equals(""))
		{
			AkSoundEngine.PostEvent(EventName, UseCharacterGameObject ? character.gameObject : base.gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			playSound(componentInParent);
		}
	}
}
