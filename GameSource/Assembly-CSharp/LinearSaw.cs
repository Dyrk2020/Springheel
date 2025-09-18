using UnityEngine;

public class LinearSaw : ActiveBlock
{
	private Animator anim;

	public Collider2D sawBlade;

	public float SawMoveSpeed = 1f;

	public Transform Spinner;

	public string LinearSawSoundEvent;

	protected override void Awake()
	{
		base.Awake();
		anim = GetComponent<Animator>();
		anim.SetBool("SawOn", value: true);
	}

	public void activateSound()
	{
		AkSoundEngine.PostEvent(LinearSawSoundEvent, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		anim.SetBool("SawOn", value: true);
		anim.speed = Modifiers.GetInstance().PlatformMoveSpeed * SawMoveSpeed;
	}

	public override void Disable()
	{
		base.Disable();
		anim.SetBool("SawOn", value: false);
		if (Spinner != null)
		{
			SpriteRenderer[] componentsInChildren = Spinner.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	public override void Enable()
	{
		base.Enable();
		if (Spinner != null)
		{
			SpriteRenderer[] componentsInChildren = Spinner.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	public override void EnablePlacement(bool showGuides = true)
	{
		base.EnablePlacement(showGuides);
		anim.SetBool("SawOn", value: false);
	}

	public override void Place(int playerNumber)
	{
		base.Place(playerNumber);
	}

	public override void Reset()
	{
		base.Reset();
		anim.SetTrigger("Reset");
		anim.SetBool("SawOn", value: false);
	}

	protected override void Activate()
	{
		base.Activate();
		anim.SetBool("SawOn", value: true);
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
