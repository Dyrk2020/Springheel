using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines;

[Serializable]
public class TransformModule : ISerializationCallbackReceiver
{
	public enum VelocityHandleMode
	{
		Zero,
		Preserve,
		Align,
		AlignRealistic
	}

	[SerializeField]
	[HideInInspector]
	private bool _hasOffset;

	[SerializeField]
	[HideInInspector]
	private bool _hasRotationOffset;

	[FormerlySerializedAs("offset")]
	[HideInInspector]
	[SerializeField]
	private Vector2 _offset;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("rotationOffset")]
	private Vector3 _rotationOffset = Vector3.zero;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("baseScale")]
	private Vector3 _baseScale = Vector3.one;

	public VelocityHandleMode velocityHandleMode;

	private SplineSample _splineResult;

	public bool applyPositionX = true;

	public bool applyPositionY = true;

	public bool applyPositionZ = true;

	public Spline.Direction direction = Spline.Direction.Forward;

	public bool applyRotationX = true;

	public bool applyRotationY = true;

	public bool applyRotationZ = true;

	public bool applyScaleX;

	public bool applyScaleY;

	public bool applyScaleZ;

	[HideInInspector]
	public SplineUser targetUser;

	private static Vector3 position = Vector3.zero;

	private static Quaternion rotation = Quaternion.identity;

	public Vector2 offset
	{
		get
		{
			return _offset;
		}
		set
		{
			if (value != _offset)
			{
				_offset = value;
				_hasOffset = _offset != Vector2.zero;
				if (targetUser != null)
				{
					targetUser.Rebuild();
				}
			}
		}
	}

	public Vector3 rotationOffset
	{
		get
		{
			return _rotationOffset;
		}
		set
		{
			if (value != _rotationOffset)
			{
				_rotationOffset = value;
				_hasRotationOffset = _rotationOffset != Vector3.zero;
				if (targetUser != null)
				{
					targetUser.Rebuild();
				}
			}
		}
	}

	public bool hasOffset => _hasOffset;

	public bool hasRotationOffset => _hasRotationOffset;

	public Vector3 baseScale
	{
		get
		{
			return _baseScale;
		}
		set
		{
			if (value != _baseScale)
			{
				_baseScale = value;
				if (targetUser != null)
				{
					targetUser.Rebuild();
				}
			}
		}
	}

	public SplineSample splineResult
	{
		get
		{
			if (_splineResult == null)
			{
				_splineResult = new SplineSample();
			}
			return _splineResult;
		}
		set
		{
			if (_splineResult == null)
			{
				_splineResult = new SplineSample(value);
			}
			else
			{
				_splineResult.CopyFrom(value);
			}
		}
	}

	public bool applyPosition
	{
		get
		{
			if (!applyPositionX && !applyPositionY)
			{
				return applyPositionZ;
			}
			return true;
		}
		set
		{
			applyPositionX = (applyPositionY = (applyPositionZ = value));
		}
	}

	public bool applyRotation
	{
		get
		{
			if (!applyRotationX && !applyRotationY)
			{
				return applyRotationZ;
			}
			return true;
		}
		set
		{
			applyRotationX = (applyRotationY = (applyRotationZ = value));
		}
	}

	public bool applyScale
	{
		get
		{
			if (!applyScaleX && !applyScaleY)
			{
				return applyScaleZ;
			}
			return true;
		}
		set
		{
			applyScaleX = (applyScaleY = (applyScaleZ = value));
		}
	}

	public void ApplyTransform(Transform input)
	{
		input.position = GetPosition(input.position);
		input.rotation = GetRotation(input.rotation);
		input.localScale = GetScale(input.localScale);
	}

	public void ApplyRigidbody(Rigidbody input)
	{
		input.transform.localScale = GetScale(input.transform.localScale);
		input.MovePosition(GetPosition(input.position));
		input.velocity = HandleVelocity(input.velocity);
		input.MoveRotation(GetRotation(input.rotation));
		Vector3 angularVelocity = input.angularVelocity;
		if (applyRotationX)
		{
			angularVelocity.x = 0f;
		}
		if (applyRotationY)
		{
			angularVelocity.y = 0f;
		}
		if (applyRotationZ)
		{
			angularVelocity.z = 0f;
		}
		input.angularVelocity = angularVelocity;
	}

	public void ApplyRigidbody2D(Rigidbody2D input)
	{
		input.transform.localScale = GetScale(input.transform.localScale);
		input.position = GetPosition(input.position);
		input.velocity = HandleVelocity(input.velocity);
		input.rotation = 0f - GetRotation(Quaternion.Euler(0f, 0f, input.rotation)).eulerAngles.z;
		if (applyRotationX)
		{
			input.angularVelocity = 0f;
		}
	}

	private Vector3 HandleVelocity(Vector3 velocity)
	{
		Vector3 vector = Vector3.zero;
		Vector3 right = Vector3.right;
		switch (velocityHandleMode)
		{
		case VelocityHandleMode.Preserve:
			vector = velocity;
			break;
		case VelocityHandleMode.Align:
			right = _splineResult.forward;
			if (Vector3.Dot(velocity, right) < 0f)
			{
				right *= -1f;
			}
			vector = right * velocity.magnitude;
			break;
		case VelocityHandleMode.AlignRealistic:
			right = _splineResult.forward;
			if (Vector3.Dot(velocity, right) < 0f)
			{
				right *= -1f;
			}
			vector = right * velocity.magnitude * Vector3.Dot(velocity.normalized, right);
			break;
		}
		if (applyPositionX)
		{
			velocity.x = vector.x;
		}
		if (applyPositionY)
		{
			velocity.y = vector.y;
		}
		if (applyPositionZ)
		{
			velocity.z = vector.z;
		}
		return velocity;
	}

	private Vector3 GetPosition(Vector3 inputPosition)
	{
		position = _splineResult.position;
		Vector2 vector = _offset;
		if (vector != Vector2.zero)
		{
			position += _splineResult.right * vector.x * _splineResult.size + _splineResult.up * vector.y * _splineResult.size;
		}
		if (applyPositionX)
		{
			inputPosition.x = position.x;
		}
		if (applyPositionY)
		{
			inputPosition.y = position.y;
		}
		if (applyPositionZ)
		{
			inputPosition.z = position.z;
		}
		return inputPosition;
	}

	private Quaternion GetRotation(Quaternion inputRotation)
	{
		rotation = Quaternion.LookRotation(_splineResult.forward * ((direction == Spline.Direction.Forward) ? 1f : (-1f)), _splineResult.up);
		if (_rotationOffset != Vector3.zero)
		{
			rotation *= Quaternion.Euler(_rotationOffset);
		}
		if (!applyRotationX || !applyRotationY || !applyRotationZ)
		{
			Vector3 eulerAngles = rotation.eulerAngles;
			Vector3 eulerAngles2 = inputRotation.eulerAngles;
			if (!applyRotationX)
			{
				eulerAngles.x = eulerAngles2.x;
			}
			if (!applyRotationY)
			{
				eulerAngles.y = eulerAngles2.y;
			}
			if (!applyRotationZ)
			{
				eulerAngles.z = eulerAngles2.z;
			}
			inputRotation.eulerAngles = eulerAngles;
		}
		else
		{
			inputRotation = rotation;
		}
		return inputRotation;
	}

	private Vector3 GetScale(Vector3 inputScale)
	{
		if (applyScaleX)
		{
			inputScale.x = _baseScale.x * _splineResult.size;
		}
		if (applyScaleY)
		{
			inputScale.y = _baseScale.y * _splineResult.size;
		}
		if (applyScaleZ)
		{
			inputScale.z = _baseScale.z * _splineResult.size;
		}
		return inputScale;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_hasRotationOffset = _rotationOffset != Vector3.zero;
		_hasOffset = _offset != Vector2.zero;
	}
}
