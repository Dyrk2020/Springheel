using UnityEngine;

public class ButtonPress : MonoBehaviour
{
	public GameObject voteButton;

	public DumbWaiter dumWaiter;

	public Animator buttonAnimator;

	public string wiseAudioString;

	private bool characterInsideLastFrame;

	public bool characterInside;

	public void FixedUpdate()
	{
		if (characterInside)
		{
			if (!characterInsideLastFrame)
			{
				dumWaiter.Go();
				buttonAnimator.SetBool("ButtonPressed", value: true);
				if (wiseAudioString != null)
				{
					AkSoundEngine.PostEvent(wiseAudioString, base.gameObject);
				}
			}
		}
		else if (characterInsideLastFrame)
		{
			buttonAnimator.SetBool("ButtonPressed", value: false);
		}
		characterInsideLastFrame = characterInside;
		characterInside = false;
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
