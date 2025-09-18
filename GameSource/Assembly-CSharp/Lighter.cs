using UnityEngine;

public class Lighter : ActiveBlock
{
	public Animator animator;

	public Collider2D fireHazard;

	public float interval = 1f;

	private float timer;

	private bool lighterOn;

	public Transform dialHolder;

	protected override void Start()
	{
		animator.SetBool("Open", value: false);
	}

	protected override void Act(float deltaTime)
	{
		timer += deltaTime;
		animator.SetFloat("Timer", timer);
		if (timer >= interval / Modifiers.GetInstance().PlatformMoveSpeed)
		{
			timer = 0f;
			lighterOn = !lighterOn;
			animator.SetBool("Open", lighterOn);
			if (lighterOn)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Flamethrower_Shoot", base.gameObject);
			}
		}
		dialHolder.localEulerAngles = new Vector3(0f, 0f, (float)((!lighterOn) ? 180 : 0) + timer / interval * Modifiers.GetInstance().PlatformMoveSpeed * 360f / 2f);
	}

	public void SetLighterHazardState(bool onOff)
	{
		Collider2D[] components = fireHazard.GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = onOff;
		}
	}

	public override void Reset()
	{
		animator.SetBool("Open", value: false);
		lighterOn = false;
		SetLighterHazardState(onOff: false);
		timer = 0f;
		dialHolder.localEulerAngles = Vector3.zero;
	}
}
