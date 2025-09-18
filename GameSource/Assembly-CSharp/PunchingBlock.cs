using UnityEngine;

public class PunchingBlock : ActiveBlock
{
	public Animator gloveAnimator;

	public PunchingBlockTrigger[] triggers;

	private bool triggerTouched;

	private bool punching;

	private const float projectileGraceTime = 0.5f;

	private float projectileGraceTimer;

	public BoxCollider2D pusherCollider;

	public BoxCollider2D projectileCollider;

	public void OnTriggerTouched()
	{
		triggerTouched = true;
	}

	public override void Reset()
	{
		base.Reset();
		gloveAnimator.SetTrigger("Reset");
		gloveAnimator.ResetTrigger("Punch");
		DisableHazard();
		PunchingBlockTrigger[] array = triggers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
		triggerTouched = false;
		punching = false;
		projectileGraceTimer = 0f;
		projectileCollider.enabled = false;
	}

	protected override void Act(float deltaTime)
	{
		if (triggerTouched)
		{
			triggerTouched = false;
			if (!punching)
			{
				Punch(locallyTriggered: true);
			}
		}
		if (projectileGraceTimer > 0f)
		{
			projectileGraceTimer -= deltaTime;
			if (projectileGraceTimer < 0f)
			{
				projectileGraceTimer = 0f;
			}
			if (!punching)
			{
				Punch(locallyTriggered: true);
			}
		}
	}

	public void Punch(bool locallyTriggered)
	{
		if (locallyTriggered)
		{
			MsgPunchingBlockTriggered msgPunchingBlockTriggered = new MsgPunchingBlockTriggered();
			msgPunchingBlockTriggered.blockID = ID;
			msgPunchingBlockTriggered.hitTriggerMask = GetHitTriggerMask();
			msgPunchingBlockTriggered.playerNumber = LobbyManager.instance.ALocalNetworkNumber();
			LobbyManager.instance.client.Send(NetMsgTypes.PunchingBlockTriggered, msgPunchingBlockTriggered);
		}
		if (!punching)
		{
			gloveAnimator.SetTrigger("Punch");
			AkSoundEngine.PostEvent("SFX_Pieces_Boxing_Glove_Punch", base.gameObject);
			punching = true;
			projectileGraceTimer = 0f;
		}
	}

	public void OnPunchAnimFinished()
	{
		punching = false;
	}

	public void EnableHazard()
	{
		pusherCollider.enabled = true;
		projectileCollider.enabled = true;
	}

	public void DisableHazard()
	{
		pusherCollider.enabled = false;
		projectileCollider.enabled = false;
	}

	public int GetHitTriggerMask()
	{
		int num = 0;
		for (int i = 0; i < triggers.Length; i++)
		{
			if (triggers[i].TriggeredByLocalPlayer)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public void AnimateMaskedTriggers(int mask)
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			if ((mask & (1 << i)) != 0)
			{
				triggers[i].OnClientTouchedTrigger();
			}
		}
	}

	public void OnProjectileTouchedTrigger(PunchingBlockTrigger trigger)
	{
		if (isActive)
		{
			trigger.OnClientTouchedTrigger();
			if (!punching)
			{
				Punch(locallyTriggered: true);
			}
			else
			{
				projectileGraceTimer = 0.5f;
			}
		}
	}

	public void ProcessTriggerMessage(MsgPunchingBlockTriggered msg)
	{
		if (isActive)
		{
			AnimateMaskedTriggers(msg.hitTriggerMask);
			if (!punching)
			{
				Punch(locallyTriggered: false);
			}
		}
	}

	public override void Pause()
	{
		base.Pause();
		gloveAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		gloveAnimator.speed = 1f;
	}
}
