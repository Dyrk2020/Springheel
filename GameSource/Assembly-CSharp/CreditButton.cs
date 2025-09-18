using GameEvent;
using UnityEngine;

public class CreditButton : MonoBehaviour, IGameEventListener
{
	public GameObject voteButton;

	public CreditMover creditMover;

	public string wiseAudioString;

	public Animator buttonAnimator;

	private bool characterInsideLastFrame;

	private bool characterInside;

	private void Start()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: false);
	}

	public void FixedUpdate()
	{
		if (characterInside)
		{
			if (!characterInsideLastFrame)
			{
				buttonAnimator.SetBool("ButtonPressed", value: true);
				LobbyManager.instance.client.Send(NetMsgTypes.ShowCredits, new MsgShowCredits());
				showCredits();
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

	private void showCredits()
	{
		creditMover.Go();
		if (wiseAudioString != null)
		{
			AkSoundEngine.PostEvent(wiseAudioString, base.gameObject);
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is NetworkMessageReceivedEvent networkMessageReceivedEvent && networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ShowCredits)
		{
			showCredits();
		}
	}
}
