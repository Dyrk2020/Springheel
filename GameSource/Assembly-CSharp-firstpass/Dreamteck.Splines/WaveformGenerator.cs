using UnityEngine;

namespace Dreamteck.Splines;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Dreamteck/Splines/Users/Waveform Generator")]
public class WaveformGenerator : MeshGenerator
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public enum Space
	{
		World,
		Local
	}

	public enum UVWrapMode
	{
		Clamp,
		UniformX,
		UniformY,
		Uniform
	}

	[SerializeField]
	[HideInInspector]
	private Axis _axis = Axis.Y;

	[HideInInspector]
	[SerializeField]
	private bool _symmetry;

	[SerializeField]
	[HideInInspector]
	private UVWrapMode _uvWrapMode;

	[HideInInspector]
	[SerializeField]
	private int _slices = 1;

	public Axis axis
	{
		get
		{
			return _axis;
		}
		set
		{
			if (value != _axis)
			{
				_axis = value;
				Rebuild();
			}
		}
	}

	public bool symmetry
	{
		get
		{
			return _symmetry;
		}
		set
		{
			if (value != _symmetry)
			{
				_symmetry = value;
				Rebuild();
			}
		}
	}

	public UVWrapMode uvWrapMode
	{
		get
		{
			return _uvWrapMode;
		}
		set
		{
			if (value != _uvWrapMode)
			{
				_uvWrapMode = value;
				Rebuild();
			}
		}
	}

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

	protected override void Awake()
	{
		base.Awake();
		mesh.name = "waveform";
	}

	protected override void BuildMesh()
	{
		base.BuildMesh();
		Generate();
	}

	protected override void Build()
	{
		base.Build();
	}

	protected override void LateRun()
	{
		base.LateRun();
	}

	private void Generate()
	{
		int vertexCount = base.sampleCount * (_slices + 1);
		AllocateMesh(vertexCount, _slices * (base.sampleCount - 1) * 6);
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		_ = base.spline.position;
		Vector3 vector = base.spline.TransformDirection(Vector3.right);
		switch (_axis)
		{
		case Axis.Y:
			vector = base.spline.TransformDirection(Vector3.up);
			break;
		case Axis.Z:
			vector = base.spline.TransformDirection(Vector3.forward);
			break;
		}
		for (int i = 0; i < base.sampleCount; i++)
		{
			evalResult = GetSampleRaw(i);
			float baseSize = GetBaseSize(evalResult);
			Vector3 position = evalResult.position;
			Vector3 vector2 = base.spline.InverseTransformPoint(position);
			Vector3 point = vector2;
			Vector3 forward = evalResult.forward;
			Vector3 up = evalResult.up;
			float num4 = 1f;
			if ((_uvWrapMode == UVWrapMode.UniformX || _uvWrapMode == UVWrapMode.Uniform) && i > 0)
			{
				num3 += Vector3.Distance(evalResult.position, GetSampleRaw(i - 1).position);
			}
			switch (_axis)
			{
			case Axis.X:
				point.x = (_symmetry ? (0f - vector2.x) : 0f);
				num4 = base.uvScale.y * Mathf.Abs(vector2.x);
				num2 += vector2.x;
				break;
			case Axis.Y:
				point.y = (_symmetry ? (0f - vector2.y) : 0f);
				num4 = base.uvScale.y * Mathf.Abs(vector2.y);
				num2 += vector2.y;
				break;
			case Axis.Z:
				point.z = (_symmetry ? (0f - vector2.z) : 0f);
				num4 = base.uvScale.y * Mathf.Abs(vector2.z);
				num2 += vector2.z;
				break;
			}
			point = base.spline.TransformPoint(point);
			Vector3 normalized = Vector3.Cross(vector, forward).normalized;
			Vector3 vector3 = Vector3.Cross(up, forward);
			for (int j = 0; j < _slices + 1; j++)
			{
				float num5 = (float)j / (float)_slices;
				tsMesh.vertices[num] = Vector3.Lerp(point, position, num5) + vector * (base.offset.y * baseSize) + vector3 * (base.offset.x * baseSize);
				tsMesh.normals[num] = normalized;
				switch (_uvWrapMode)
				{
				case UVWrapMode.Clamp:
					tsMesh.uv[num] = new Vector2((float)evalResult.percent * base.uvScale.x + base.uvOffset.x, num5 * base.uvScale.y + base.uvOffset.y);
					break;
				case UVWrapMode.UniformX:
					tsMesh.uv[num] = new Vector2(num3 * base.uvScale.x + base.uvOffset.x, num5 * base.uvScale.y + base.uvOffset.y);
					break;
				case UVWrapMode.UniformY:
					tsMesh.uv[num] = new Vector2((float)evalResult.percent * base.uvScale.x + base.uvOffset.x, num4 * num5 * base.uvScale.y + base.uvOffset.y);
					break;
				case UVWrapMode.Uniform:
					tsMesh.uv[num] = new Vector2(num3 * base.uvScale.x + base.uvOffset.x, num4 * num5 * base.uvScale.y + base.uvOffset.y);
					break;
				}
				tsMesh.colors[num] = GetBaseColor(evalResult) * base.color;
				num++;
			}
		}
		if (base.sampleCount > 0)
		{
			num2 /= (float)base.sampleCount;
		}
		MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, base.sampleCount, num2 < 0f);
	}
}
