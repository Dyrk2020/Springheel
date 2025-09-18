using GameEvent;
using UnityEngine;

public class FerrisWheel : MultipieceBlock
{
	public float RotateSpeed;

	public Transform Spinner;

	public bool Clockwise = true;

	public string FerrisWheelSoundEvent;

	public Transform[] PositionTargets;

	public Transform SpinnerNew;

	public Rigidbody2D[] PivotsRb;

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
				Debug.LogError("Ferris Wheel cannot have rotation direction: None");
				break;
			case RotationDirections.Clockwise:
				Clockwise = true;
				UpdatePlatformScales();
				break;
			case RotationDirections.CounterClockwise:
				Clockwise = false;
				UpdatePlatformScales();
				break;
			}
		}
	}

	protected override void Activate()
	{
		base.Activate();
		AkSoundEngine.PostEvent(FerrisWheelSoundEvent, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		float value = calculateMassRatio();
		value = Mathf.Clamp(value, 1f, MaximumMassSpeedRatio);
		float zAngle = Modifiers.GetInstance().RotatorSpeed * RotateSpeed * deltaTime * (float)((!Clockwise) ? 1 : (-1)) / value;
		SpinnerNew.Rotate(0f, 0f, zAngle);
		for (int i = 0; i < PivotsRb.Length; i++)
		{
			if (PivotsRb[i] != null)
			{
				PivotsRb[i].transform.position = PositionTargets[i].transform.position;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		SpinnerNew.localRotation = Quaternion.identity;
		for (int i = 0; i < PivotsRb.Length; i++)
		{
			if (PivotsRb[i] != null)
			{
				PivotsRb[i].transform.position = PositionTargets[i].transform.position;
			}
		}
	}

	public override void Disable()
	{
		base.Disable();
		Reset();
	}

	public override void Flip(bool sprint)
	{
		base.Flip(sprint);
		OrientMode orientMode = ((sprint && OrientationAlt != OrientMode.NONE) ? OrientationAlt : Orientation);
		if ((uint)(orientMode - 1) <= 1u)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			Clockwise = !Clockwise;
		}
		UpdatePlatformScales();
	}

	private void UpdatePlatformScales()
	{
		SpriteRenderer[] placementGuides = PlacementGuides;
		foreach (SpriteRenderer obj in placementGuides)
		{
			Vector3 localScale = obj.transform.localScale;
			localScale.x = (float)(Clockwise ? 1 : (-1)) * Mathf.Abs(localScale.x);
			obj.transform.localScale = localScale;
		}
	}

	public override void Place(int playerNumber, bool sendEvent, bool force)
	{
		base.Place(playerNumber, sendEvent: false, force);
		if (sendEvent)
		{
			GameEventManager.SendEvent(new PiecePlacedEvent(playerNumber, this));
		}
	}
}
