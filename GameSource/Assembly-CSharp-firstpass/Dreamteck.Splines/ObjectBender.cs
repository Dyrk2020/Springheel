using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Object Bender")]
public class ObjectBender : SplineUser
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public enum NormalMode
	{
		Spline,
		Auto,
		Custom
	}

	public enum ForwardMode
	{
		Spline,
		Custom
	}

	[Serializable]
	public class BendProperty
	{
		public bool enabled = true;

		public TS_Transform transform;

		public bool applyRotation = true;

		public bool applyScale = true;

		public bool generateLightmapUVs;

		[SerializeField]
		[HideInInspector]
		private bool _bendMesh = true;

		[SerializeField]
		[HideInInspector]
		private bool _bendSpline = true;

		[SerializeField]
		[HideInInspector]
		private bool _bendCollider = true;

		private float colliderUpdateDue;

		public float colliderUpdateRate = 0.2f;

		private bool updateCollider;

		public Vector3 originalPosition = Vector3.zero;

		public Vector3 originalScale = Vector3.one;

		public Quaternion originalRotation = Quaternion.identity;

		public Quaternion parentRotation = Quaternion.identity;

		public Vector3 positionPercent;

		public Vector3[] vertexPercents = new Vector3[0];

		public Vector3[] normals = new Vector3[0];

		public Vector3[] colliderVertexPercents = new Vector3[0];

		public Vector3[] colliderNormals = new Vector3[0];

		[HideInInspector]
		[SerializeField]
		private Mesh originalMesh;

		[HideInInspector]
		[SerializeField]
		private Mesh originalColliderMesh;

		private Spline _originalSpline;

		[SerializeField]
		[HideInInspector]
		private Mesh destinationMesh;

		[HideInInspector]
		[SerializeField]
		private Mesh destinationColliderMesh;

		public Spline destinationSpline;

		public TS_Mesh _editMesh;

		public TS_Mesh _editColliderMesh;

		public MeshFilter filter;

		public MeshCollider collider;

		public SplineComputer splineComputer;

		public Vector3[] splinePointPercents = new Vector3[0];

		public Vector3[] primaryTangentPercents = new Vector3[0];

		public Vector3[] secondaryTangentPercents = new Vector3[0];

		[HideInInspector]
		[SerializeField]
		private bool parent;

		public bool isValid
		{
			get
			{
				if (transform != null)
				{
					return transform.transform != null;
				}
				return false;
			}
		}

		public bool bendMesh
		{
			get
			{
				return _bendMesh;
			}
			set
			{
				if (value == _bendMesh)
				{
					return;
				}
				_bendMesh = value;
				if (value)
				{
					if (filter != null && filter.sharedMesh != null)
					{
						normals = originalMesh.normals;
						for (int i = 0; i < normals.Length; i++)
						{
							normals[i] = transform.transform.TransformDirection(normals[i]);
						}
					}
				}
				else
				{
					RevertMesh();
				}
			}
		}

		public bool bendCollider
		{
			get
			{
				return _bendCollider;
			}
			set
			{
				if (value == _bendCollider)
				{
					return;
				}
				_bendCollider = value;
				if (value)
				{
					if (collider != null && collider.sharedMesh != null && collider.sharedMesh != originalMesh)
					{
						colliderNormals = originalColliderMesh.normals;
					}
				}
				else
				{
					RevertCollider();
				}
			}
		}

		public bool bendSpline
		{
			get
			{
				return _bendSpline;
			}
			set
			{
				_bendSpline = value;
			}
		}

		public TS_Mesh editMesh
		{
			get
			{
				if (!bendMesh || originalMesh == null)
				{
					_editMesh = null;
				}
				else if (_editMesh == null && originalMesh != null)
				{
					_editMesh = new TS_Mesh(originalMesh);
				}
				return _editMesh;
			}
		}

		public TS_Mesh editColliderMesh
		{
			get
			{
				if (!bendCollider || originalColliderMesh == null)
				{
					_editColliderMesh = null;
				}
				else if (_editColliderMesh == null && originalColliderMesh != null && originalColliderMesh != originalMesh)
				{
					_editColliderMesh = new TS_Mesh(originalColliderMesh);
				}
				return _editColliderMesh;
			}
		}

		public Spline originalSpline
		{
			get
			{
				if (!bendSpline || splineComputer == null)
				{
					_originalSpline = null;
				}
				else if (_originalSpline == null && splineComputer != null)
				{
					_originalSpline = new Spline(splineComputer.type);
					_originalSpline.points = splineComputer.GetPoints();
				}
				return _originalSpline;
			}
		}

		public bool isParent => parent;

		public BendProperty(Transform t, bool parent = false)
		{
			this.parent = parent;
			transform = new TS_Transform(t);
			originalPosition = t.localPosition;
			originalScale = t.localScale;
			originalRotation = t.localRotation;
			parentRotation = t.transform.rotation;
			if (t.transform.parent != null)
			{
				parentRotation = t.transform.parent.rotation;
			}
			filter = t.GetComponent<MeshFilter>();
			collider = t.GetComponent<MeshCollider>();
			if (filter != null && filter.sharedMesh != null)
			{
				originalMesh = filter.sharedMesh;
				normals = originalMesh.normals;
				for (int i = 0; i < normals.Length; i++)
				{
					normals[i] = transform.transform.TransformDirection(normals[i]).normalized;
				}
			}
			if (collider != null && collider.sharedMesh != null)
			{
				originalColliderMesh = collider.sharedMesh;
				colliderNormals = originalColliderMesh.normals;
				for (int j = 0; j < colliderNormals.Length; j++)
				{
					colliderNormals[j] = transform.transform.TransformDirection(colliderNormals[j]);
				}
			}
			if (!parent)
			{
				splineComputer = t.GetComponent<SplineComputer>();
			}
			if (splineComputer != null)
			{
				if (splineComputer.isClosed)
				{
					originalSpline.Close();
				}
				destinationSpline = new Spline(originalSpline.type);
				destinationSpline.points = new SplinePoint[originalSpline.points.Length];
				destinationSpline.points = splineComputer.GetPoints();
				if (splineComputer.isClosed)
				{
					destinationSpline.Close();
				}
			}
		}

		public void Revert()
		{
			if (isValid)
			{
				RevertTransform();
				RevertCollider();
				RevertMesh();
				if (splineComputer != null)
				{
					splineComputer.SetPoints(_originalSpline.points);
				}
			}
		}

		private void RevertMesh()
		{
			if (filter != null)
			{
				filter.sharedMesh = originalMesh;
			}
			destinationMesh = null;
		}

		private void RevertTransform()
		{
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
			transform.Update();
			transform.scale = originalScale;
			transform.Update();
		}

		private void RevertCollider()
		{
			if (collider != null)
			{
				collider.sharedMesh = originalColliderMesh;
			}
			destinationColliderMesh = null;
		}

		public void Apply(bool applyTransform)
		{
			if (!enabled || !isValid)
			{
				return;
			}
			if (applyTransform)
			{
				transform.Update();
			}
			if (editMesh != null && editMesh.hasUpdate)
			{
				ApplyMesh();
			}
			if (bendCollider && collider != null && !updateCollider && ((editColliderMesh == null && editMesh != null) || editColliderMesh != null))
			{
				updateCollider = true;
				if (Application.isPlaying)
				{
					colliderUpdateDue = Time.time + colliderUpdateRate;
				}
			}
			if (splineComputer != null)
			{
				ApplySpline();
			}
		}

		public void Update()
		{
			if (Time.time >= colliderUpdateDue && updateCollider)
			{
				updateCollider = false;
				ApplyCollider();
			}
		}

		private void ApplyMesh()
		{
			if (!(filter == null))
			{
				MeshUtility.CalculateTangents(editMesh);
				if (destinationMesh == null)
				{
					destinationMesh = new Mesh();
					destinationMesh.name = originalMesh.name;
				}
				editMesh.WriteMesh(ref destinationMesh);
				destinationMesh.RecalculateBounds();
				filter.sharedMesh = destinationMesh;
			}
		}

		private void ApplyCollider()
		{
			if (collider == null)
			{
				return;
			}
			if (originalColliderMesh == originalMesh)
			{
				collider.sharedMesh = filter.sharedMesh;
				return;
			}
			MeshUtility.CalculateTangents(editColliderMesh);
			if (destinationColliderMesh == null)
			{
				destinationColliderMesh = new Mesh();
				destinationColliderMesh.name = originalColliderMesh.name;
			}
			editColliderMesh.WriteMesh(ref destinationColliderMesh);
			destinationColliderMesh.RecalculateBounds();
			collider.sharedMesh = destinationColliderMesh;
		}

		private void ApplySpline()
		{
			if (destinationSpline != null)
			{
				splineComputer.SetPoints(destinationSpline.points);
			}
		}
	}

	[HideInInspector]
	[SerializeField]
	private bool _bend;

	[HideInInspector]
	public BendProperty[] bendProperties = new BendProperty[0];

	[SerializeField]
	[HideInInspector]
	private bool _parentIsTheSpline;

	[SerializeField]
	[HideInInspector]
	private TS_Bounds bounds;

	[HideInInspector]
	[SerializeField]
	private Axis _axis = Axis.Z;

	[HideInInspector]
	[SerializeField]
	private NormalMode _normalMode = NormalMode.Auto;

	[SerializeField]
	[HideInInspector]
	private ForwardMode _forwardMode;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("_upVector")]
	private Vector3 _customNormal = Vector3.up;

	[SerializeField]
	[HideInInspector]
	private Vector3 _customForward = Vector3.forward;

	private Matrix4x4 normalMatrix;

	private Quaternion bendRotation = Quaternion.identity;

	public bool bend
	{
		get
		{
			return _bend;
		}
		set
		{
			if (_bend != value)
			{
				_bend = value;
				if (value)
				{
					UpdateReferences();
					Rebuild();
				}
				else
				{
					Revert();
				}
			}
		}
	}

	public Axis axis
	{
		get
		{
			return _axis;
		}
		set
		{
			if (base.spline != null && value != _axis)
			{
				_axis = value;
				UpdateReferences();
				Rebuild();
			}
			else
			{
				_axis = value;
			}
		}
	}

	public NormalMode upMode
	{
		get
		{
			return _normalMode;
		}
		set
		{
			if (base.spline != null && value != _normalMode)
			{
				_normalMode = value;
				Rebuild();
			}
			else
			{
				_normalMode = value;
			}
		}
	}

	public Vector3 customNormal
	{
		get
		{
			return _customNormal;
		}
		set
		{
			if (base.spline != null && value != _customNormal)
			{
				_customNormal = value;
				Rebuild();
			}
			else
			{
				_customNormal = value;
			}
		}
	}

	public ForwardMode forwardMode
	{
		get
		{
			return _forwardMode;
		}
		set
		{
			if (base.spline != null && value != _forwardMode)
			{
				_forwardMode = value;
				Rebuild();
			}
			else
			{
				_forwardMode = value;
			}
		}
	}

	public Vector3 customForward
	{
		get
		{
			return _customForward;
		}
		set
		{
			if (base.spline != null && value != _customForward)
			{
				_customForward = value;
				Rebuild();
			}
			else
			{
				_customForward = value;
			}
		}
	}

	private void GetTransformsRecursively(Transform current, ref List<Transform> transformList)
	{
		transformList.Add(current);
		foreach (Transform item in current)
		{
			GetTransformsRecursively(item, ref transformList);
		}
	}

	private void GetObjects()
	{
		List<Transform> transformList = new List<Transform>();
		GetTransformsRecursively(base.transform, ref transformList);
		BendProperty[] array = new BendProperty[transformList.Count];
		for (int i = 0; i < transformList.Count; i++)
		{
			CreateProperty(ref array[i], transformList[i]);
		}
		bendProperties = array;
		SplineComputer component = GetComponent<SplineComputer>();
		_parentIsTheSpline = component == base.spline;
	}

	public TS_Bounds GetBounds()
	{
		return new TS_Bounds(bounds.min, bounds.max, bounds.center);
	}

	private void CreateProperty(ref BendProperty property, Transform t)
	{
		property = new BendProperty(t, t == base.transform);
		for (int i = 0; i < bendProperties.Length; i++)
		{
			if (bendProperties[i].transform.transform == t)
			{
				property.enabled = bendProperties[i].enabled;
				property.applyRotation = bendProperties[i].applyRotation;
				property.applyScale = bendProperties[i].applyScale;
				property.bendMesh = bendProperties[i].bendMesh;
				property.bendCollider = bendProperties[i].bendCollider;
				property.generateLightmapUVs = bendProperties[i].generateLightmapUVs;
				property.colliderUpdateRate = bendProperties[i].colliderUpdateRate;
				break;
			}
		}
		if (t.transform != base.trs)
		{
			property.originalPosition = base.trs.InverseTransformPoint(t.position);
			property.originalRotation = Quaternion.Inverse(base.trs.rotation) * t.rotation;
		}
	}

	private void CalculateBounds()
	{
		if (bounds == null)
		{
			bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
		}
		bounds.min = (bounds.max = Vector3.zero);
		for (int i = 0; i < bendProperties.Length; i++)
		{
			CalculatePropertyBounds(ref bendProperties[i]);
		}
		for (int j = 0; j < bendProperties.Length; j++)
		{
			CalculatePercents(bendProperties[j]);
		}
	}

	private void CalculatePropertyBounds(ref BendProperty property)
	{
		if (!property.enabled || (property.isParent && _parentIsTheSpline))
		{
			return;
		}
		if (property.transform.transform == base.trs)
		{
			if (0f < bounds.min.x)
			{
				bounds.min.x = 0f;
			}
			if (0f < bounds.min.y)
			{
				bounds.min.y = 0f;
			}
			if (0f < bounds.min.z)
			{
				bounds.min.z = 0f;
			}
			if (0f > bounds.max.x)
			{
				bounds.max.x = 0f;
			}
			if (0f > bounds.max.y)
			{
				bounds.max.y = 0f;
			}
			if (0f > bounds.max.z)
			{
				bounds.max.z = 0f;
			}
		}
		else
		{
			if (property.originalPosition.x < bounds.min.x)
			{
				bounds.min.x = property.originalPosition.x;
			}
			if (property.originalPosition.y < bounds.min.y)
			{
				bounds.min.y = property.originalPosition.y;
			}
			if (property.originalPosition.z < bounds.min.z)
			{
				bounds.min.z = property.originalPosition.z;
			}
			if (property.originalPosition.x > bounds.max.x)
			{
				bounds.max.x = property.originalPosition.x;
			}
			if (property.originalPosition.y > bounds.max.y)
			{
				bounds.max.y = property.originalPosition.y;
			}
			if (property.originalPosition.z > bounds.max.z)
			{
				bounds.max.z = property.originalPosition.z;
			}
		}
		if (property.editMesh != null)
		{
			for (int i = 0; i < property.editMesh.vertices.Length; i++)
			{
				Vector3 position = property.transform.TransformPoint(property.editMesh.vertices[i]);
				position = base.trs.InverseTransformPoint(position);
				if (position.x < bounds.min.x)
				{
					bounds.min.x = position.x;
				}
				if (position.y < bounds.min.y)
				{
					bounds.min.y = position.y;
				}
				if (position.z < bounds.min.z)
				{
					bounds.min.z = position.z;
				}
				if (position.x > bounds.max.x)
				{
					bounds.max.x = position.x;
				}
				if (position.y > bounds.max.y)
				{
					bounds.max.y = position.y;
				}
				if (position.z > bounds.max.z)
				{
					bounds.max.z = position.z;
				}
			}
		}
		if (property.editColliderMesh != null)
		{
			for (int j = 0; j < property.editColliderMesh.vertices.Length; j++)
			{
				Vector3 position2 = property.transform.TransformPoint(property.editColliderMesh.vertices[j]);
				position2 = base.trs.InverseTransformPoint(position2);
				if (position2.x < bounds.min.x)
				{
					bounds.min.x = position2.x;
				}
				if (position2.y < bounds.min.y)
				{
					bounds.min.y = position2.y;
				}
				if (position2.z < bounds.min.z)
				{
					bounds.min.z = position2.z;
				}
				if (position2.x > bounds.max.x)
				{
					bounds.max.x = position2.x;
				}
				if (position2.y > bounds.max.y)
				{
					bounds.max.y = position2.y;
				}
				if (position2.z > bounds.max.z)
				{
					bounds.max.z = position2.z;
				}
			}
		}
		if (property.originalSpline != null)
		{
			for (int k = 0; k < property.originalSpline.points.Length; k++)
			{
				Vector3 vector = base.trs.InverseTransformPoint(property.originalSpline.points[k].position);
				if (vector.x < bounds.min.x)
				{
					bounds.min.x = vector.x;
				}
				if (vector.y < bounds.min.y)
				{
					bounds.min.y = vector.y;
				}
				if (vector.z < bounds.min.z)
				{
					bounds.min.z = vector.z;
				}
				if (vector.x > bounds.max.x)
				{
					bounds.max.x = vector.x;
				}
				if (vector.y > bounds.max.y)
				{
					bounds.max.y = vector.y;
				}
				if (vector.z > bounds.max.z)
				{
					bounds.max.z = vector.z;
				}
			}
		}
		bounds.CreateFromMinMax(bounds.min, bounds.max);
	}

	public void CalculatePercents(BendProperty property)
	{
		if (property.transform.transform != base.trs)
		{
			property.positionPercent = GetPercentage(base.trs.InverseTransformPoint(property.transform.position));
		}
		else
		{
			property.positionPercent = GetPercentage(Vector3.zero);
		}
		if (property.editMesh != null)
		{
			if (property.vertexPercents.Length != property.editMesh.vertexCount)
			{
				property.vertexPercents = new Vector3[property.editMesh.vertexCount];
			}
			if (property.editColliderMesh != null && property.colliderVertexPercents.Length != property.editMesh.vertexCount)
			{
				property.colliderVertexPercents = new Vector3[property.editColliderMesh.vertexCount];
			}
			for (int i = 0; i < property.editMesh.vertexCount; i++)
			{
				Vector3 position = property.transform.TransformPoint(property.editMesh.vertices[i]);
				position = base.trs.InverseTransformPoint(position);
				property.vertexPercents[i] = GetPercentage(position);
			}
			if (property.editColliderMesh != null)
			{
				for (int j = 0; j < property.editColliderMesh.vertexCount; j++)
				{
					Vector3 position2 = property.transform.TransformPoint(property.editColliderMesh.vertices[j]);
					position2 = base.trs.InverseTransformPoint(position2);
					property.colliderVertexPercents[j] = GetPercentage(position2);
				}
			}
		}
		if (property.splineComputer != null)
		{
			SplinePoint[] points = property.splineComputer.GetPoints();
			property.splinePointPercents = new Vector3[points.Length];
			property.primaryTangentPercents = new Vector3[points.Length];
			property.secondaryTangentPercents = new Vector3[points.Length];
			for (int k = 0; k < points.Length; k++)
			{
				property.splinePointPercents[k] = GetPercentage(base.trs.InverseTransformPoint(points[k].position));
				property.primaryTangentPercents[k] = GetPercentage(base.trs.InverseTransformPoint(points[k].tangent));
				property.secondaryTangentPercents[k] = GetPercentage(base.trs.InverseTransformPoint(points[k].tangent2));
			}
		}
	}

	private void Revert()
	{
		for (int i = 0; i < bendProperties.Length; i++)
		{
			bendProperties[i].Revert();
		}
	}

	public void UpdateReferences()
	{
		if (!base.hasTransform)
		{
			CacheTransform();
		}
		if (_bend)
		{
			for (int i = 0; i < bendProperties.Length; i++)
			{
				bendProperties[i].Revert();
			}
		}
		GetObjects();
		CalculateBounds();
		if (_bend)
		{
			Bend();
			for (int j = 0; j < bendProperties.Length; j++)
			{
				bendProperties[j].Apply(j > 0 || base.trs != base.spline.transform);
				bendProperties[j].Update();
			}
		}
	}

	private void GetevalResult(Vector3 percentage)
	{
		switch (axis)
		{
		case Axis.X:
			Evaluate(percentage.x, evalResult);
			break;
		case Axis.Y:
			Evaluate(percentage.y, evalResult);
			break;
		case Axis.Z:
			Evaluate(percentage.z, evalResult);
			break;
		}
		switch (_normalMode)
		{
		case NormalMode.Auto:
			evalResult.up = Vector3.Cross(evalResult.forward, evalResult.right);
			break;
		case NormalMode.Custom:
			evalResult.up = _customNormal;
			break;
		}
		if (_forwardMode == ForwardMode.Custom)
		{
			evalResult.forward = customForward;
		}
		ModifySample(evalResult);
		Vector3 right = evalResult.right;
		Quaternion quaternion = Quaternion.identity;
		switch (axis)
		{
		case Axis.Z:
			evalResult.position += right * Mathf.Lerp(bounds.min.x, bounds.max.x, percentage.x) * evalResult.size;
			evalResult.position += evalResult.up * Mathf.Lerp(bounds.min.y, bounds.max.y, percentage.y) * evalResult.size;
			break;
		case Axis.X:
			quaternion = Quaternion.Euler(0f, -90f, 0f);
			evalResult.position += right * Mathf.Lerp(bounds.max.z, bounds.min.z, percentage.z) * evalResult.size;
			evalResult.position += evalResult.up * Mathf.Lerp(bounds.min.y, bounds.max.y, percentage.y) * evalResult.size;
			break;
		case Axis.Y:
			quaternion = Quaternion.Euler(90f, 0f, 0f);
			evalResult.position += right * Mathf.Lerp(bounds.min.x, bounds.max.x, percentage.x) * evalResult.size;
			evalResult.position += evalResult.up * Mathf.Lerp(bounds.min.z, bounds.max.z, percentage.z) * evalResult.size;
			break;
		}
		bendRotation = evalResult.rotation * quaternion;
		normalMatrix = Matrix4x4.TRS(evalResult.position, bendRotation, Vector3.one * evalResult.size).inverse.transpose;
	}

	private Vector3 GetPercentage(Vector3 point)
	{
		point.x = Mathf.InverseLerp(bounds.min.x, bounds.max.x, point.x);
		point.y = Mathf.InverseLerp(bounds.min.y, bounds.max.y, point.y);
		point.z = Mathf.InverseLerp(bounds.min.z, bounds.max.z, point.z);
		return point;
	}

	protected override void Build()
	{
		base.Build();
		if (_bend)
		{
			Bend();
		}
	}

	private void Bend()
	{
		if (base.sampleCount > 1 && bendProperties.Length != 0)
		{
			for (int i = 0; i < bendProperties.Length; i++)
			{
				BendObject(bendProperties[i]);
			}
		}
	}

	public void BendObject(BendProperty p)
	{
		if (!p.enabled || (p.isParent && _parentIsTheSpline))
		{
			return;
		}
		GetevalResult(p.positionPercent);
		p.transform.position = evalResult.position;
		if (p.applyRotation)
		{
			p.transform.rotation = bendRotation * (Quaternion.Inverse(p.parentRotation) * p.originalRotation);
		}
		else
		{
			p.transform.rotation = p.originalRotation;
		}
		if (p.applyScale)
		{
			p.transform.scale = p.originalScale * evalResult.size;
		}
		Matrix4x4 inverse = Matrix4x4.TRS(p.transform.position, p.transform.rotation, p.transform.scale).inverse;
		if (p.editMesh != null)
		{
			BendMesh(p.vertexPercents, p.normals, p.editMesh, inverse);
			p.editMesh.hasUpdate = true;
		}
		if (p._editColliderMesh != null)
		{
			BendMesh(p.colliderVertexPercents, p.colliderNormals, p.editColliderMesh, inverse);
			p.editColliderMesh.hasUpdate = true;
		}
		if (p.originalSpline == null || p.isParent)
		{
			return;
		}
		for (int i = 0; i < p.splinePointPercents.Length; i++)
		{
			SplinePoint splinePoint = p.originalSpline.points[i];
			GetevalResult(p.splinePointPercents[i]);
			splinePoint.position = evalResult.position;
			GetevalResult(p.primaryTangentPercents[i]);
			splinePoint.tangent = evalResult.position;
			GetevalResult(p.secondaryTangentPercents[i]);
			splinePoint.tangent2 = evalResult.position;
			switch (axis)
			{
			case Axis.X:
				splinePoint.normal = Quaternion.LookRotation(evalResult.forward, evalResult.up) * Quaternion.FromToRotation(Vector3.up, evalResult.up) * splinePoint.normal;
				break;
			case Axis.Y:
				splinePoint.normal = Quaternion.LookRotation(evalResult.forward, evalResult.up) * Quaternion.FromToRotation(Vector3.up, evalResult.up) * splinePoint.normal;
				break;
			case Axis.Z:
				splinePoint.normal = Quaternion.LookRotation(evalResult.forward, evalResult.up) * splinePoint.normal;
				break;
			}
			p.destinationSpline.points[i] = splinePoint;
		}
	}

	private void BendMesh(Vector3[] vertexPercents, Vector3[] originalNormals, TS_Mesh mesh, Matrix4x4 worldToLocalMatrix)
	{
		if (mesh.vertexCount != vertexPercents.Length)
		{
			Debug.LogError("Vertex count mismatch");
			return;
		}
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			Vector3 percentage = vertexPercents[i];
			if (axis == Axis.Y)
			{
				percentage.z = 1f - percentage.z;
			}
			GetevalResult(percentage);
			mesh.vertices[i] = worldToLocalMatrix.MultiplyPoint3x4(evalResult.position);
			mesh.normals[i] = worldToLocalMatrix.MultiplyVector(normalMatrix.MultiplyVector(originalNormals[i]));
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		if (_bend)
		{
			for (int i = 0; i < bendProperties.Length; i++)
			{
				bendProperties[i].Apply(i > 0 || base.trs != base.spline.transform);
				bendProperties[i].Update();
			}
		}
	}

	protected override void LateRun()
	{
		base.LateRun();
		for (int i = 0; i < bendProperties.Length; i++)
		{
			bendProperties[i].Update();
		}
	}
}
