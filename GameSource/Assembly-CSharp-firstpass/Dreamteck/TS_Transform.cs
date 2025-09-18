using System;
using UnityEngine;

namespace Dreamteck;

[Serializable]
public class TS_Transform
{
	private bool setPosition;

	private bool setRotation;

	private bool setScale;

	private bool setLocalPosition;

	private bool setLocalRotation;

	[HideInInspector]
	[SerializeField]
	private Transform _transform;

	[HideInInspector]
	[SerializeField]
	private volatile float posX;

	[HideInInspector]
	[SerializeField]
	private volatile float posY;

	[HideInInspector]
	[SerializeField]
	private volatile float posZ;

	[SerializeField]
	[HideInInspector]
	private volatile float scaleX = 1f;

	[HideInInspector]
	[SerializeField]
	private volatile float scaleY = 1f;

	[SerializeField]
	[HideInInspector]
	private volatile float scaleZ = 1f;

	[SerializeField]
	[HideInInspector]
	private volatile float lossyScaleX = 1f;

	[SerializeField]
	[HideInInspector]
	private volatile float lossyScaleY = 1f;

	[SerializeField]
	[HideInInspector]
	private volatile float lossyScaleZ = 1f;

	[SerializeField]
	[HideInInspector]
	private volatile float rotX;

	[SerializeField]
	[HideInInspector]
	private volatile float rotY;

	[SerializeField]
	[HideInInspector]
	private volatile float rotZ;

	[SerializeField]
	[HideInInspector]
	private volatile float rotW;

	[SerializeField]
	[HideInInspector]
	private volatile float lposX;

	[SerializeField]
	[HideInInspector]
	private volatile float lposY;

	[SerializeField]
	[HideInInspector]
	private volatile float lposZ;

	[HideInInspector]
	[SerializeField]
	private volatile float lrotX;

	[HideInInspector]
	[SerializeField]
	private volatile float lrotY;

	[HideInInspector]
	[SerializeField]
	private volatile float lrotZ;

	[HideInInspector]
	[SerializeField]
	private volatile float lrotW;

	public Vector3 position
	{
		get
		{
			return new Vector3(posX, posY, posZ);
		}
		set
		{
			setPosition = true;
			setLocalPosition = false;
			posX = value.x;
			posY = value.y;
			posZ = value.z;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return new Quaternion(rotX, rotY, rotZ, rotW);
		}
		set
		{
			setRotation = true;
			setLocalRotation = false;
			rotX = value.x;
			rotY = value.y;
			rotZ = value.z;
			rotW = value.w;
		}
	}

	public Vector3 scale
	{
		get
		{
			return new Vector3(scaleX, scaleY, scaleZ);
		}
		set
		{
			setScale = true;
			scaleX = value.x;
			scaleY = value.y;
			scaleZ = value.z;
		}
	}

	public Vector3 lossyScale
	{
		get
		{
			return new Vector3(lossyScaleX, lossyScaleY, lossyScaleZ);
		}
		set
		{
			setScale = true;
			lossyScaleX = value.x;
			lossyScaleY = value.y;
			lossyScaleZ = value.z;
		}
	}

	public Vector3 localPosition
	{
		get
		{
			return new Vector3(lposX, lposY, lposZ);
		}
		set
		{
			setLocalPosition = true;
			setPosition = false;
			lposX = value.x;
			lposY = value.y;
			lposZ = value.z;
		}
	}

	public Quaternion localRotation
	{
		get
		{
			return new Quaternion(lrotX, lrotY, lrotZ, lrotW);
		}
		set
		{
			setLocalRotation = true;
			setRotation = false;
			lrotX = value.x;
			lrotY = value.y;
			lrotZ = value.z;
			lrotW = value.w;
		}
	}

	public Transform transform => _transform;

	public TS_Transform(Transform input)
	{
		SetTransform(input);
	}

	public void Update()
	{
		if (!(transform == null))
		{
			if (setPosition)
			{
				_transform.position = position;
			}
			else if (setLocalPosition)
			{
				_transform.localPosition = localPosition;
			}
			else
			{
				position = _transform.position;
				localPosition = _transform.localPosition;
			}
			if (setScale)
			{
				_transform.localScale = scale;
			}
			else
			{
				scale = _transform.localScale;
			}
			lossyScale = _transform.lossyScale;
			if (setRotation)
			{
				_transform.rotation = rotation;
			}
			else if (setLocalRotation)
			{
				_transform.localRotation = localRotation;
			}
			else
			{
				rotation = _transform.rotation;
				localRotation = _transform.localRotation;
			}
			setPosition = (setLocalPosition = (setRotation = (setLocalRotation = (setScale = false))));
		}
	}

	public void SetTransform(Transform input)
	{
		_transform = input;
		setPosition = (setLocalPosition = (setRotation = (setLocalRotation = (setScale = false))));
		Update();
	}

	public bool HasChange()
	{
		if (!HasPositionChange() && !HasRotationChange())
		{
			return HasScaleChange();
		}
		return true;
	}

	public bool HasPositionChange()
	{
		if (posX == _transform.position.x && posY == _transform.position.y)
		{
			return posZ != _transform.position.z;
		}
		return true;
	}

	public bool HasRotationChange()
	{
		if (rotX == _transform.rotation.x && rotY == _transform.rotation.y && rotZ == _transform.rotation.z)
		{
			return rotW != _transform.rotation.w;
		}
		return true;
	}

	public bool HasScaleChange()
	{
		if (lossyScaleX == _transform.lossyScale.x && lossyScaleY == _transform.lossyScale.y)
		{
			return lossyScaleZ != _transform.lossyScale.z;
		}
		return true;
	}

	public Vector3 TransformPoint(Vector3 point)
	{
		Vector3 vector = new Vector3(point.x * lossyScaleX, point.y * lossyScaleY, point.z * lossyScaleZ);
		Vector3 vector2 = rotation * vector;
		return position + vector2;
	}

	public Vector3 TransformDirection(Vector3 direction)
	{
		return TransformPoint(direction) - position;
	}

	public Vector3 InverseTransformPoint(Vector3 point)
	{
		return InverseTransformDirection(point - position);
	}

	public Vector3 InverseTransformDirection(Vector3 direction)
	{
		Vector3 vector = Quaternion.Inverse(rotation) * direction;
		return new Vector3(vector.x / lossyScaleX, vector.y / lossyScaleY, vector.z / lossyScaleZ);
	}

	public T GetComponent<T>()
	{
		return _transform.GetComponent<T>();
	}
}
