using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class handicap : MonoBehaviour, IGameEventListener
{
	public enum HandicapAction
	{
		ADD,
		REMOVE,
		RESET,
		NONE
	}

	public Transform[] HandicapLineSlots;

	public GameObject HandicapLinePrefab;

	public Collider2D Add;

	public Collider2D Reduce;

	public Collider2D Reset;

	public Animator AddAnim;

	public Animator ReduceAnim;

	public Animator ResetAnim;

	public int increment = 10;

	protected float coolDownTimer;

	public float coolDownTime;

	public GameObject handicapMessagePrefab;

	private static TagComparer.Tag solidPlayerMask = (TagComparer.Tag)160;

	private void Start()
	{
		int num = 1;
		Transform[] handicapLineSlots = HandicapLineSlots;
		foreach (Transform parent in handicapLineSlots)
		{
			GameObject obj = Object.Instantiate(HandicapLinePrefab);
			obj.transform.SetParent(parent);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			HandicapLine component = obj.GetComponent<HandicapLine>();
			if (component != null)
			{
				component.PlayerNetworkNumber = num;
			}
			num++;
		}
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: false);
	}

	private void Update()
	{
		if (coolDownTimer > 0f)
		{
			coolDownTimer -= Time.unscaledDeltaTime;
		}
	}

	private bool IsSolidPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAllTags(solidPlayerMask);
		}
		return false;
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (!(coolDownTimer <= 0f))
		{
			return;
		}
		CollisionTag component = collision.gameObject.GetComponent<CollisionTag>();
		if (!IsSolidPlayer(collision.gameObject, component))
		{
			return;
		}
		Character componentInParent = collision.gameObject.GetComponentInParent<Character>();
		if (!(componentInParent != null) || !componentInParent.hasAuthority)
		{
			return;
		}
		coolDownTimer = coolDownTime;
		int num = componentInParent.AssociatedLobbyPlayer.handicap;
		HandicapAction handicapAction = HandicapAction.NONE;
		ContactPoint2D[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint2D contactPoint2D = contacts[i];
			if (contactPoint2D.otherCollider == Add)
			{
				num += increment;
				handicapAction = HandicapAction.ADD;
				AddAnim.SetTrigger("press");
				AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_PlusTen", base.gameObject);
				break;
			}
			if (contactPoint2D.otherCollider == Reduce)
			{
				num -= increment;
				handicapAction = HandicapAction.REMOVE;
				ReduceAnim.SetTrigger("press");
				AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_MinusTen", base.gameObject);
				break;
			}
			if (contactPoint2D.otherCollider == Reset)
			{
				num = 100;
				handicapAction = HandicapAction.RESET;
				ResetAnim.SetTrigger("press");
				AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_Reset", base.gameObject);
				break;
			}
		}
		if (handicapAction != HandicapAction.NONE)
		{
			sendHandicapMessage(num, componentInParent.networkNumber, handicapAction);
		}
		componentInParent.AssociatedLobbyPlayer.SetPlayerHandicap(num);
	}

	private void sendHandicapMessage(int handicap, int player, HandicapAction action)
	{
		MsgPlayerHandicapSet msgPlayerHandicapSet = new MsgPlayerHandicapSet();
		msgPlayerHandicapSet.Handicap = handicap;
		msgPlayerHandicapSet.NetworkPlayerNumber = player;
		msgPlayerHandicapSet.Action = action;
		LobbyManager.instance.client.Send(NetMsgTypes.PlayerHandicapSet, msgPlayerHandicapSet);
	}

	private void DisplayMessage(string messageText)
	{
		GameObject obj = Object.Instantiate(handicapMessagePrefab);
		obj.transform.position = base.transform.position;
		Text componentInChildren = obj.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			componentInChildren.text = messageText;
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.PlayerHandicapSet)
		{
			return;
		}
		MsgPlayerHandicapSet msgPlayerHandicapSet = (MsgPlayerHandicapSet)networkMessageReceivedEvent.ReadMessage;
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.networkNumber == msgPlayerHandicapSet.NetworkPlayerNumber)
			{
				return;
			}
		}
		switch (msgPlayerHandicapSet.Action)
		{
		case HandicapAction.ADD:
			AddAnim.SetTrigger("press");
			AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_PlusTen", base.gameObject);
			break;
		case HandicapAction.REMOVE:
			ReduceAnim.SetTrigger("press");
			AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_MinusTen", base.gameObject);
			break;
		case HandicapAction.RESET:
			ResetAnim.SetTrigger("press");
			AkSoundEngine.PostEvent("UI_Lobby_ScoreBalancer_Reset", base.gameObject);
			break;
		case HandicapAction.NONE:
			break;
		}
	}
}
