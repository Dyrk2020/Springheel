using System;
using UnityEngine;

namespace Dreamteck.Splines;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Dreamteck/Splines/Users/Path Generator")]
public class PathGenerator : MeshGenerator
{
	[SerializeField]
	[HideInInspector]
	private int _slices = 1;

	[SerializeField]
	[HideInInspector]
	private bool _useShapeCurve;

	[SerializeField]
	[HideInInspector]
	private AnimationCurve _shape;

	[SerializeField]
	[HideInInspector]
	private AnimationCurve _lastShape;

	[HideInInspector]
	[SerializeField]
	private float _shapeExposure = 1f;

	public int slices
	{
		get
		{
			return _slices;
		}
		set
		{
			if (value != _slices)
			{
				if (value < 1)
				{
					value = 1;
				}
				_slices = value;
				Rebuild();
			}
		}
	}

	public bool useShapeCurve
	{
		get
		{
			return _useShapeCurve;
		}
		set
		{
			if (value != _useShapeCurve)
			{
				_useShapeCurve = value;
				if (_useShapeCurve)
				{
					_shape = new AnimationCurve();
					_shape.AddKey(new Keyframe(0f, 0f));
					_shape.AddKey(new Keyframe(1f, 0f));
				}
				else
				{
					_shape = null;
				}
				Rebuild();
			}
		}
	}

	public float shapeExposure
	{
		get
		{
			return _shapeExposure;
		}
		set
		{
			if (base.spline != null && value != _shapeExposure)
			{
				_shapeExposure = value;
				Rebuild();
			}
		}
	}

	public AnimationCurve shape
	{
		get
		{
			return _shape;
		}
		set
		{
			if (_lastShape == null)
			{
				_lastShape = new AnimationCurve();
			}
			bool flag = false;
			if (value.keys.Length != _lastShape.keys.Length)
			{
				flag = true;
			}
			else
			{
				for (int i = 0; i < value.keys.Length; i++)
				{
					if (value.keys[i].inTangent != _lastShape.keys[i].inTangent || value.keys[i].outTangent != _lastShape.keys[i].outTangent || value.keys[i].time != _lastShape.keys[i].time || value.keys[i].value != value.keys[i].value)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				Rebuild();
			}
			_lastShape.keys = new Keyframe[value.keys.Length];
			value.keys.CopyTo(_lastShape.keys, 0);
			_lastShape.preWrapMode = value.preWrapMode;
			_lastShape.postWrapMode = value.postWrapMode;
			_shape = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mesh.name = "path";
	}

	protected override void Reset()
	{
		base.Reset();
	}

	protected override void BuildMesh()
	{
		base.BuildMesh();
		GenerateVertices();
		MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, base.sampleCount, flip: false);
	}

	private void GenerateVertices()
	{
		int vertexCount = (_slices + 1) * base.sampleCount;
		AllocateMesh(vertexCount, _slices * (base.sampleCount - 1) * 6);
		int num = 0;
		ResetUVDistance();
		bool flag = base.offset != Vector3.zero;
		for (int i = 0; i < base.sampleCount; i++)
		{
			GetSample(i, evalResult);
			Vector3 zero = Vector3.zero;
			try
			{
				zero = evalResult.position;
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message + " for i = " + i);
				break;
			}
			Vector3 right = evalResult.right;
			float baseSize = GetBaseSize(evalResult);
			if (flag)
			{
				zero += base.offset.x * baseSize * right + base.offset.y * baseSize * evalResult.up + base.offset.z * baseSize * evalResult.forward;
			}
			float num2 = base.size * baseSize;
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = Quaternion.AngleAxis(base.rotation, evalResult.forward);
			if (base.uvMode == UVMode.UniformClamp || base.uvMode == UVMode.UniformClip)
			{
				AddUVDistance(i);
			}
			Color color = GetBaseColor(evalResult) * base.color;
			for (int j = 0; j < _slices + 1; j++)
			{
				float num3 = (float)j / (float)_slices;
				float num4 = 0f;
				if (_useShapeCurve)
				{
					num4 = _shape.Evaluate(num3);
				}
				tsMesh.vertices[num] = zero + quaternion * right * (num2 * 0.5f) - quaternion * right * (num2 * num3) + quaternion * evalResult.up * (num4 * _shapeExposure);
				CalculateUVs(evalResult.percent, 1f - num3);
				tsMesh.uv[num] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(base.uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - MeshGenerator.uvs));
				if (_slices > 1)
				{
					if (j < _slices)
					{
						float num5 = (float)(j + 1) / (float)_slices;
						num4 = 0f;
						if (_useShapeCurve)
						{
							num4 = _shape.Evaluate(num5);
						}
						Vector3 vector2 = zero + quaternion * right * num2 * 0.5f - quaternion * right * num2 * num5 + quaternion * evalResult.up * num4 * _shapeExposure;
						Vector3 vector3 = -Vector3.Cross(evalResult.forward, vector2 - tsMesh.vertices[num]).normalized;
						if (j > 0)
						{
							Vector3 b = -Vector3.Cross(evalResult.forward, tsMesh.vertices[num] - vector).normalized;
							tsMesh.normals[num] = Vector3.Slerp(vector3, b, 0.5f);
						}
						else
						{
							tsMesh.normals[num] = vector3;
						}
					}
					else
					{
						tsMesh.normals[num] = -Vector3.Cross(evalResult.forward, tsMesh.vertices[num] - vector).normalized;
					}
				}
				else
				{
					tsMesh.normals[num] = evalResult.up;
					if (base.rotation != 0f)
					{
						tsMesh.normals[num] = quaternion * tsMesh.normals[num];
					}
				}
				tsMesh.colors[num] = color;
				vector = tsMesh.vertices[num];
				num++;
			}
		}
	}
}
