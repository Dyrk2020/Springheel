using UnityEngine;

public class ButtonPressSpaceStation : MonoBehaviour
{
	public GameObject voteButton;

	public SpaceStationDoor spaceStationDoor;

	public Animator buttonAnimator;

	public string wiseAudioString;

	private bool characterInsideLastFrame;

	public bool characterInside;

	public NetworkSurrogate networkSurrogate;

	public void FixedUpdate()
	{
		if (characterInside)
		{
			if (!characterInsideLastFrame)
			{
				spaceStationDoor.Toggle();
				buttonAnimator.SetBool("ButtonPressed", value: true);
				_ = wiseAudioString;
				networkSurrogate.TriggerVal = true;
			}
		}
		else if (characterInsideLastFrame)
		{
			buttonAnimator.SetBool("ButtonPressed", value: false);
		}
		characterInsideLastFrame = characterInside;
		characterInside = false;
		if (networkSurrogate.TriggerVal && wiseAudioString != null)
		{
			AkSoundEngine.PostEvent(wiseAudioString, base.gameObject);
		}
	}

	public void OnTriggerStay2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (component != null && component.ContainsAnyTag(TagComparer.Tag.Player))
		{
			characterInside = true;
		}
	}
}
