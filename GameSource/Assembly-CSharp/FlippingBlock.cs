using UnityEngine;

public class FlippingBlock : ActiveBlock
{
	public BoxCollider2D BottomCollider;

	public BoxCollider2D HazardCollider;

	public float Interval;

	public Animator FlippingBlockAnimator;

	private float timer;

	private bool solid = true;

	protected override void Awake()
	{
		base.Awake();
		FlippingBlockAnimator.SetBool("Solid", value: true);
	}

	protected override void Act(float deltaTime)
	{
		timer += deltaTime;
		float num = Interval / Modifiers.GetInstance().PlatformMoveSpeed;
		if (timer >= num)
		{
			solid = !solid;
			FlippingBlockAnimator.SetBool("Solid", solid);
			timer = 0f;
		}
	}

	public override void Disable()
	{
		base.Disable();
		FlippingBlockAnimator.SetBool("Solid", value: true);
		FlippingBlockAnimator.SetTrigger("Reset");
		solid = true;
		timer = 0f;
	}

	public override void Enable()
	{
		base.Enable();
	}

	public override void Reset()
	{
		base.Reset();
		FlippingBlockAnimator.SetBool("Solid", value: true);
		solid = true;
		FlippingBlockAnimator.SetTrigger("Reset");
		timer = 0f;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		BottomCollider.enabled = true;
		HazardCollider.enabled = true;
	}

	public override void Pause()
	{
		base.Pause();
		FlippingBlockAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		FlippingBlockAnimator.speed = 1f;
	}
}
