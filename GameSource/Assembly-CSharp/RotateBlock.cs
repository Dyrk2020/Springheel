using UnityEngine;

public class RotateBlock : ActiveBlock
{
	public float Degrees;

	public float RotateSpeed;

	public float Interval;

	public Vector3 CenterOfRotation;

	public bool Clockwise = true;

	public bool IsAnchor;

	public Transform Spinner;

	public string RotationSoundEvent;

	public bool ConstantSound;

	private float rotateTime;

	private float rotateAmt;

	public bool real;

	public float placementIndicatorInitialScale;

	public Transform counterRotateHolder;

	private float soundTimer;

	public float soundTimerInterval = 0.2f;

	protected Rigidbody2D rb;

	protected Rigidbody2D rbSpinner;

	public override RotationDirections RotationDirection
	{
		get
		{
			if (!Clockwise)
			{
				return RotationDirections.CounterClockwise;
			}
			return RotationDirections.Clockwise;
		}
		set
		{
			switch (value)
			{
			case RotationDirections.None:
				Debug.LogError("Rotating block cannot have rotation direction: None");
				break;
			case RotationDirections.Clockwise:
				Clockwise = true;
				UpdatePlacementGuides();
				break;
			case RotationDirections.CounterClockwise:
				Clockwise = false;
				UpdatePlacementGuides();
				break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.Rotation, 0f, base.transform.position, base.gameObject);
		placementIndicatorInitialScale = PlacementGuides[0].transform.localScale.x;
	}

	protected override void Start()
	{
		base.Start();
		Degrees = Mathf.Abs(Degrees);
		rb = GetComponent<Rigidbody2D>();
		if (IsAnchor && Spinner != null)
		{
			rbSpinner = Spinner.GetComponent<Rigidbody2D>();
		}
	}

	protected override void Act(float deltaTime)
	{
		if (rotateAmt == 0f)
		{
			rotateTime += deltaTime;
		}
		if (!(rotateTime >= Interval))
		{
			return;
		}
		float value = (IsAnchor ? 1f : calculateMassRatio());
		value = Mathf.Clamp(value, 1f, MaximumMassSpeedRatio);
		float num = Modifiers.GetInstance().RotatorSpeed * RotateSpeed * deltaTime * (float)((!Clockwise) ? 1 : (-1)) / value;
		if (Mathf.Abs(num) + rotateAmt > Degrees)
		{
			num = (Degrees - rotateAmt) * (float)((!Clockwise) ? 1 : (-1));
		}
		rotateAmt += Mathf.Abs(num);
		if (IsAnchor && Spinner != null)
		{
			if (rbSpinner != null && ParentPiece == null)
			{
				rbSpinner.transform.rotation = Quaternion.Euler(0f, 0f, num + Spinner.rotation.eulerAngles.z);
			}
			else
			{
				Spinner.Rotate(0f, 0f, num);
			}
		}
		else
		{
			rb.MoveRotation(num + base.transform.rotation.eulerAngles.z);
		}
		if (counterRotateHolder != null)
		{
			counterRotateHolder.Rotate(0f, 0f, num * -1f);
		}
		if (rotateAmt >= Degrees)
		{
			rotateAmt = 0f;
			rotateTime = 0f;
		}
		soundTimer += deltaTime;
		if (real && soundTimer > soundTimerInterval && !disabled && RotationSoundEvent != "" && !ConstantSound)
		{
			AkSoundEngine.PostEvent(RotationSoundEvent, base.gameObject);
			soundTimer = 0f;
		}
	}

	public override void EnablePlacement(bool showGuides = true)
	{
		base.EnablePlacement(showGuides);
		UpdatePlacementGuides();
	}

	public override void Reset()
	{
		base.Reset();
		rotateAmt = 0f;
		rotateTime = 0f;
		if (IsAnchor && Spinner != null)
		{
			Spinner.localRotation = Quaternion.identity;
		}
		else
		{
			base.transform.localRotation = OriginalRotation;
		}
		if (counterRotateHolder != null)
		{
			counterRotateHolder.localRotation = OriginalRotation;
		}
	}

	public override void Disable()
	{
		base.Disable();
		SpriteRenderer[] componentsInChildren;
		if (Spinner != null)
		{
			componentsInChildren = Spinner.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		componentsInChildren = PlacementGuides;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.localScale = new Vector3(1f, 1f, 1f) * placementIndicatorInitialScale;
		}
		Reset();
	}

	public override void Enable()
	{
		base.Enable();
		real = true;
		if (Spinner != null)
		{
			SpriteRenderer[] componentsInChildren = Spinner.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		UpdatePlacementGuides();
	}

	protected override void Activate()
	{
		base.Activate();
		if (RotationSoundEvent != "" && ConstantSound && !disabled)
		{
			AkSoundEngine.PostEvent(RotationSoundEvent, base.gameObject);
		}
	}

	public override void Place(int playerNumber)
	{
		base.Place(playerNumber);
	}

	public override void Flip(bool sprint)
	{
		base.Flip(sprint);
		OrientMode orientMode = ((sprint && OrientationAlt != OrientMode.NONE) ? OrientationAlt : Orientation);
		if ((uint)(orientMode - 1) <= 1u)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			Clockwise = !Clockwise;
			UpdatePlacementGuides();
		}
	}

	private void UpdatePlacementGuides()
	{
		SpriteRenderer[] placementGuides = PlacementGuides;
		for (int i = 0; i < placementGuides.Length; i++)
		{
			placementGuides[i].transform.localScale = new Vector3((!Clockwise) ? 1 : (-1), 1f, 1f) * placementIndicatorInitialScale;
		}
		if (counterRotateHolder != null)
		{
			counterRotateHolder.localScale = new Vector3(Clockwise ? 1 : (-1), 1f, 1f);
			SpinObject component = counterRotateHolder.GetComponent<SpinObject>();
			if (component != null)
			{
				component.Clockwise = Clockwise;
			}
		}
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		if (IsAnchor)
		{
			return base.GetPhysicsModifier();
		}
		pms[0].Direction = base.transform.position;
		if (rotateTime >= Interval && base.Active)
		{
			float num = (IsAnchor ? 1f : calculateMassRatio());
			float magnitude = Modifiers.GetInstance().RotatorSpeed * RotateSpeed * (float)((!Clockwise) ? 1 : (-1)) / num;
			pms[0].Magnitude = magnitude;
		}
		else
		{
			pms[0].Magnitude = 0f;
		}
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		if (IsAnchor)
		{
			return base.GetPhysicsModifiers();
		}
		pms[0].Direction = base.transform.position;
		if (rotateTime >= Interval && base.Active)
		{
			float num = (IsAnchor ? 1f : calculateMassRatio());
			float magnitude = Modifiers.GetInstance().RotatorSpeed * RotateSpeed * (float)((!Clockwise) ? 1 : (-1)) / num;
			pms[0].Magnitude = magnitude;
		}
		else
		{
			pms[0].Magnitude = 0f;
		}
		return new PhysicsModifier[1] { pms[0] };
	}
}
