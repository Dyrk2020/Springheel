using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class Thwomp : ActiveBlock, IGameEventListener
{
	public Anvil anvil;

	public AnvilDropper anvilDropper;

	public float restDuration = 1f;

	private float restTimer;

	private bool inEditMode = true;

	private ThwompState state;

	public ThwompState State => state;

	protected override void Start()
	{
		base.Start();
		ChangeState(ThwompState.REST);
	}

	private void ChangeState(ThwompState newState)
	{
		if (state != newState)
		{
			switch (newState)
			{
			case ThwompState.REST:
				AkSoundEngine.PostEvent("SFX_ThwompBlock_Rest", base.gameObject);
				break;
			case ThwompState.TRIGGERED:
				AkSoundEngine.PostEvent("SFX_ThwompBlock_Alert", base.gameObject);
				break;
			case ThwompState.FALL:
				AkSoundEngine.PostEvent("SFX_ThwompBlock_Down", base.gameObject);
				break;
			case ThwompState.RETURN:
				AkSoundEngine.PostEvent("SFX_ThwompBlock_Up", base.gameObject);
				break;
			}
		}
		state = newState;
		if (!inEditMode)
		{
			UpdateActiveColliders();
		}
	}

	private void UpdateActiveColliders()
	{
		anvil.UpdateColliders(state);
		anvilDropper.UpdateColliders(state);
	}

	protected override void Act(float deltaTime)
	{
		base.Act(deltaTime);
		switch (state)
		{
		case ThwompState.FALL:
			Falling(deltaTime);
			break;
		case ThwompState.GROUNDED:
			restTimer -= deltaTime;
			if (restTimer <= 0f)
			{
				ChangeState(ThwompState.RETURN);
			}
			break;
		case ThwompState.RETURN:
			Returning(deltaTime);
			break;
		case ThwompState.CLOSING:
			anvil.ReturnStep(deltaTime);
			anvilDropper.PlayClosingAnimation();
			break;
		}
	}

	private void Falling(float deltaTime)
	{
		Collider2D collider2D = anvil.DetectFloor();
		if (collider2D != null)
		{
			AkSoundEngine.PostEvent("SFX_ThwompBlock_Ground", base.gameObject);
			GroundDetected(collider2D);
			return;
		}
		anvil.FallStep(deltaTime);
		if (anvil.HasExceededMaxFallDistance())
		{
			AkSoundEngine.PostEvent("SFX_ThwompBlock_Bottom", base.gameObject);
			MaxDistanceReached();
		}
	}

	private void MaxDistanceReached()
	{
		ChangeState(ThwompState.GROUNDED);
		anvil.WaitInMidAir();
		restTimer = restDuration;
	}

	private void GroundDetected(Collider2D collider)
	{
		ChangeState(ThwompState.GROUNDED);
		AkSoundEngine.PostEvent("SFX_ThwompBlock_Hit", base.gameObject);
		anvil.Ground(collider);
		restTimer = restDuration;
	}

	private void Returning(float deltaTime)
	{
		anvil.ReturnStep(deltaTime);
		anvilDropper.PlayClosingAnimation();
		if (anvil.HasReachedStartPosition())
		{
			ChangeState(ThwompState.REST);
		}
	}

	public void OnDropperOpeningComplete()
	{
		if (state == ThwompState.TRIGGERED)
		{
			anvil.StartFall();
			ChangeState(ThwompState.FALL);
		}
	}

	public void OnTriggerStay2D(Collider2D collider)
	{
		if (!inEditMode && state == ThwompState.REST)
		{
			CollisionTag component = collider.GetComponent<CollisionTag>();
			if (!(component == null) && state == ThwompState.REST && component.ContainsAnyTag(TagComparer.Tag.Player) && component.ContainsAnyTag(TagComparer.Tag.Solid))
			{
				SignalThwompTriggered();
			}
		}
	}

	public void SignalThwompTriggered()
	{
		TriggerThwompLocal();
		if (!(NetworkManager.singleton == null) && NetworkManager.singleton.client != null)
		{
			MsgThwompTriggered msgThwompTriggered = new MsgThwompTriggered();
			msgThwompTriggered.ThwompID = ID;
			NetworkManager.singleton.client.Send(NetMsgTypes.ThwompTriggered, msgThwompTriggered);
		}
	}

	private void TriggerThwompLocal()
	{
		if (state == ThwompState.REST)
		{
			ChangeState(ThwompState.TRIGGERED);
			anvilDropper.PlayOpeningAnimation();
			anvil.PlayAnticipationAnimation();
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ThwompTriggered && (networkMessageReceivedEvent.ReadMessage as MsgThwompTriggered).ThwompID == ID)
			{
				TriggerThwompLocal();
			}
		}
	}

	public override void Reset()
	{
		anvil.Reset();
		anvilDropper.Reset();
		ChangeState(ThwompState.REST);
	}

	public override void Pause()
	{
		base.Pause();
		anvil.Pause();
		anvilDropper.Pause();
	}

	public override void Unpause()
	{
		base.Unpause();
		anvil.Unpause();
		anvilDropper.Unpause();
	}

	public override void ToPlayMode()
	{
		base.ToPlayMode();
		inEditMode = false;
		UpdateActiveColliders();
	}

	protected override void ToPlaceMode(bool enableSelection)
	{
		base.ToPlaceMode(enableSelection);
		inEditMode = true;
		Reset();
	}
}
