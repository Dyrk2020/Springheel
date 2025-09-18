using UnityEngine;

public class SphynxMove : ActiveBlock
{
	public Animator SphynxAnimator;

	protected override void Start()
	{
		base.Start();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected override void Activate()
	{
		base.Activate();
		SphynxAnimator.SetBool("Moving", value: true);
	}

	public override void Reset()
	{
		base.Reset();
		SphynxAnimator.SetBool("Moving", value: false);
	}

	public override void Pause()
	{
		base.Pause();
		SphynxAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		SphynxAnimator.speed = 1f;
	}
}
