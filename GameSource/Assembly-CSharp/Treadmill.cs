using UnityEngine;

public class Treadmill : MultipieceBlock
{
	public Animator TreadmillAnimator;

	public Collider2D LargeCollider;

	public string TreadmillSoundEvent;

	public float animationSpeed = 1f;

	public bool animationOn;

	public float animationTimestamp;

	public float innerRadius = 0.25f;

	public float fullRevolutionTime = 5f;

	private float massSpeedRatio = 1f;

	private Vector2[] colliderPositions;

	private float[] colliderAngles;

	private Rigidbody2D[] partsRB;

	protected override void Awake()
	{
		base.Awake();
		if (LargeCollider != null)
		{
			MultipiecePart[] parts = Parts;
			for (int i = 0; i < parts.Length; i++)
			{
				Collider2D[] componentsInChildren = parts[i].GetComponentsInChildren<Collider2D>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					Physics2D.IgnoreCollision(componentsInChildren[j], LargeCollider);
				}
			}
		}
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.Treadmill, 0f, Vector2.zero, base.gameObject);
		partsRB = new Rigidbody2D[Parts.Length];
		for (int k = 0; k < Parts.Length; k++)
		{
			partsRB[k] = Parts[k].GetComponent<Rigidbody2D>();
		}
		RecalculateColliderPositions();
		colliderAngles = new float[8] { 0f, 0f, 0f, 90f, 180f, 180f, 180f, 270f };
		animationTimestamp = 0f;
		UpdatePartPositions();
	}

	private void RecalculateColliderPositions()
	{
		colliderPositions = new Vector2[8]
		{
			new Vector2(1f, innerRadius),
			new Vector2(0f, innerRadius),
			new Vector2(-1f, innerRadius),
			new Vector2(0f - (1f + innerRadius), 0f),
			new Vector2(-1f, 0f - innerRadius),
			new Vector2(0f, 0f - innerRadius),
			new Vector2(1f, 0f - innerRadius),
			new Vector2(1f + innerRadius, 0f)
		};
	}

	public void activateSound()
	{
	}

	protected override void Activate()
	{
		base.Activate();
		TreadmillAnimator.SetBool("TreadmillOn", value: true);
		animationOn = true;
		AkSoundEngine.PostEvent(TreadmillSoundEvent, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (animationOn)
		{
			massSpeedRatio = calculateMassRatio();
			massSpeedRatio = Mathf.Clamp(massSpeedRatio, 1f, MaximumMassSpeedRatio);
			animationSpeed = 1f / massSpeedRatio;
			animationTimestamp += deltaTime * (Modifiers.GetInstance().RotatorSpeed * animationSpeed / fullRevolutionTime);
			if (animationTimestamp >= 1f)
			{
				animationTimestamp -= 1f;
			}
			UpdatePartPositions();
		}
	}

	public override void EnablePlacement(bool showGuides)
	{
		base.EnablePlacement(showGuides);
		if (LargeCollider != null)
		{
			MultipiecePart[] parts = Parts;
			for (int i = 0; i < parts.Length; i++)
			{
				Physics2D.IgnoreCollision(parts[i].GetComponentInChildren<Collider2D>(), LargeCollider);
			}
		}
	}

	public override void Enable()
	{
		base.Enable();
		if (LargeCollider != null)
		{
			MultipiecePart[] parts = Parts;
			for (int i = 0; i < parts.Length; i++)
			{
				Physics2D.IgnoreCollision(parts[i].GetComponentInChildren<Collider2D>(), LargeCollider);
			}
		}
	}

	public override void Disable()
	{
		base.Disable();
		TreadmillAnimator.SetBool("TreadmillOn", value: false);
		animationOn = false;
	}

	public override void Reset()
	{
		base.Reset();
		ResetPartPositions();
		TreadmillAnimator.SetTrigger("Reset");
		TreadmillAnimator.SetBool("TreadmillOn", value: false);
		animationOn = false;
	}

	public override void Pause()
	{
		base.Pause();
		animationSpeed = 0f;
	}

	public override void Unpause()
	{
		base.Pause();
		if (!paused && !scoreboard)
		{
			animationSpeed = 1f;
		}
	}

	public override bool CanPlace()
	{
		return CanPlaceWithParts();
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		return GetPhysicsModifiers();
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		pms[0].Direction = new Vector2(0f - base.transform.localScale.x, 0f);
		pms[0].Magnitude = (animationOn ? (Modifiers.GetInstance().RotatorSpeed * 8f / fullRevolutionTime / massSpeedRatio) : 0f);
		return pms;
	}

	private void ResetPartPositions()
	{
		animationTimestamp = 0f;
		UpdatePartPositions();
	}

	private void UpdatePartPositions()
	{
		float treadmillBaseRotation = Mathf.Round(base.transform.localRotation.eulerAngles.z);
		int num = 0;
		Rigidbody2D[] array = partsRB;
		foreach (Rigidbody2D partRB in array)
		{
			float num2 = (float)num * 0.125f;
			float num3 = animationTimestamp + num2;
			if (num3 >= 1f)
			{
				num3 -= 1f;
			}
			SetPartTransform(num3, treadmillBaseRotation, partRB);
			num++;
		}
	}

	private void SetPartTransform(float t, float treadmillBaseRotation, Rigidbody2D partRB)
	{
		if (partRB == null)
		{
			Debug.LogError("Part is null!");
			return;
		}
		int num = Mathf.FloorToInt(t * 8f);
		int num2 = (num + 1) % 8;
		float num3 = (float)num / 8f;
		float t2 = (t - num3) * 8f;
		switch (num)
		{
		case 0:
		case 1:
		case 4:
		case 5:
			partRB.transform.position = base.transform.TransformPoint(Vector2.LerpUnclamped(colliderPositions[num], colliderPositions[num2], t2));
			partRB.transform.rotation = Quaternion.Euler(0f, 0f, colliderAngles[num] + treadmillBaseRotation);
			break;
		case 2:
		case 3:
		case 6:
		case 7:
		{
			Vector3 vector = new Vector3((num >= 4) ? 1 : (-1), 0f, 0f);
			float b = ((num == 7) ? 360f : colliderAngles[num2]);
			float num4 = Mathf.LerpUnclamped(colliderAngles[num], b, t2);
			Quaternion quaternion = Quaternion.Euler(0f, 0f, num4);
			partRB.transform.position = base.transform.TransformPoint(vector + quaternion * (Vector3.up * innerRadius));
			if (base.transform.localScale.x < 0f)
			{
				num4 = 360f - num4;
			}
			partRB.transform.rotation = Quaternion.Euler(0f, 0f, num4 + treadmillBaseRotation);
			break;
		}
		default:
			Debug.LogError("ERROR! SetPartTransform: Impossible quadrant: " + num);
			break;
		}
	}
}
