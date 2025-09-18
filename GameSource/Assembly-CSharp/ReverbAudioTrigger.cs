using UnityEngine;

public class ReverbAudioTrigger : MonoBehaviour
{
	public void OnTriggerEnter2D(Collider2D collision)
	{
		Character componentInParent = collision.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			componentInParent.Reverb = true;
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		Character componentInParent = collision.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			componentInParent.Reverb = false;
		}
	}
}
