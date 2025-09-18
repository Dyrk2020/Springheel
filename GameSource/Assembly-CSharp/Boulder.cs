using UnityEngine;

public class Boulder : ActiveBlock
{
	private Animator animator;

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
	}

	public void TriggerTrap()
	{
		if (animator != null)
		{
			animator.SetTrigger("Trap");
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (animator != null)
		{
			animator.SetTrigger("Reset");
		}
		AkSoundEngine.PostEvent("SFX_Level_Jungle_RockWheel_Stop", base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		if (animator != null)
		{
			animator.speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (animator != null)
		{
			animator.speed = 1f;
		}
	}

	public void playBounceSound()
	{
	}
}
