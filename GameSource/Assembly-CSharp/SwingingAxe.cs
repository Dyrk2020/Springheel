using UnityEngine;

public class SwingingAxe : ActiveBlock
{
	private Animator anim;

	public float SawMoveSpeed = 1f;

	public Transform RotateLocked;

	public string LinearSawSoundEvent;

	protected override void Awake()
	{
		base.Awake();
		anim = base.gameObject.GetComponentInChildren<Animator>();
		anim.SetBool("On", value: true);
	}

	protected void Update()
	{
		RotateLocked.transform.rotation = Quaternion.identity;
	}

	public void activateSound()
	{
		AkSoundEngine.PostEvent(LinearSawSoundEvent, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		anim.SetBool("On", value: true);
		anim.speed = Modifiers.GetInstance().PlatformMoveSpeed * SawMoveSpeed;
	}

	public override void Disable()
	{
		base.Disable();
		anim.SetBool("On", value: false);
	}

	public override void Enable()
	{
		base.Enable();
	}

	public override void EnablePlacement(bool showGuides = true)
	{
		base.EnablePlacement(showGuides);
		anim.SetBool("On", value: false);
	}

	public override void Place(int playerNumber)
	{
		base.Place(playerNumber);
	}

	public override void Reset()
	{
		base.Reset();
		anim.SetBool("On", value: false);
	}

	protected override void Activate()
	{
		base.Activate();
		anim.SetBool("On", value: true);
	}

	public override void Pause()
	{
		base.Pause();
		anim.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		anim.speed = Modifiers.GetInstance().PlatformMoveSpeed * SawMoveSpeed;
	}
}
