using UnityEngine;

public class FireHydrant : ActiveBlock
{
	public BoxCollider2D Geyser;

	public Transform Cap;

	public Collider2D capProjectile;

	public float Interval;

	public float riseTime;

	public float fallTime;

	public float riseHeight;

	public float timer;

	public bool on;

	public bool animating;

	public Animator HydrantAnimator;

	public float height;

	private bool real = true;

	private Vector3 geyserPos;

	private Vector3 capPos;

	protected override void Start()
	{
		base.Start();
		HydrantAnimator.SetBool("On", value: false);
		height = Geyser.size.y;
		geyserPos = Geyser.transform.localPosition;
		capPos = Cap.localPosition;
	}

	protected override void Act(float deltaTime)
	{
		if (!animating)
		{
			timer += deltaTime;
		}
		float num = Interval / Modifiers.GetInstance().PlatformMoveSpeed;
		if (timer >= num && !animating)
		{
			on = !on;
			HydrantAnimator.SetBool("On", on);
			timer = 0f;
			animating = true;
			Geyser.enabled = on && real && !disabled;
			capProjectile.enabled = on;
			if (Geyser.enabled)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Fire_Hydrant", base.gameObject);
			}
		}
		if (on && height < riseHeight)
		{
			float num2 = deltaTime / riseTime;
			height += num2 * riseHeight;
			if (height > riseHeight)
			{
				height = riseHeight;
			}
			Geyser.size = new Vector2(Geyser.size.x, height);
			geyserPos.Set(0f, height / 2f, 0f);
			Geyser.transform.localPosition = geyserPos;
		}
		else if (!on && height > 0.001f)
		{
			if (fallTime > 0f)
			{
				float num3 = deltaTime / fallTime;
				height -= num3 * riseHeight;
			}
			else
			{
				height = 0f;
			}
			if (height < 0.001f)
			{
				height = 0.001f;
			}
			Geyser.size = new Vector2(Geyser.size.x, height);
			geyserPos.Set(0f, height / 2f, 0f);
			Geyser.transform.localPosition = geyserPos;
		}
		else
		{
			animating = false;
		}
		capPos.Set(0f, height, 0f);
		Cap.localPosition = capPos;
	}

	public override void Disable()
	{
		base.Disable();
		if (HydrantAnimator != null)
		{
			HydrantAnimator.SetBool("On", value: false);
		}
		on = false;
		timer = 0f;
		Geyser.enabled = false;
		height = 0.001f;
		Geyser.size = new Vector2(Geyser.size.x, height);
		Cap.localPosition = new Vector3(0f, height, 0f);
	}

	public override void Enable()
	{
		base.Enable();
		real = true;
		timer = 0f;
	}

	public override void Reset()
	{
		base.Reset();
		HydrantAnimator.SetBool("On", value: false);
		timer = 0f;
		height = 0.001f;
		on = false;
		animating = false;
		Geyser.size = new Vector2(Geyser.size.x, height);
		Cap.localPosition = new Vector3(0f, height, 0f);
	}

	public override void Pause()
	{
		base.Pause();
		HydrantAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		HydrantAnimator.speed = 1f;
	}
}
