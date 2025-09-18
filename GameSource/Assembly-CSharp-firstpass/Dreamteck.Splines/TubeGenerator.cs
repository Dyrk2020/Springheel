using UnityEngine;

namespace Dreamteck.Splines;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Dreamteck/Splines/Users/Tube Generator")]
[RequireComponent(typeof(MeshRenderer))]
public class TubeGenerator : MeshGenerator
{
	public enum CapMethod
	{
		None,
		Flat,
		Round
	}

	[SerializeField]
	[HideInInspector]
	private int _sides = 12;

	[SerializeField]
	[HideInInspector]
	private int _roundCapLatitude = 6;

	[HideInInspector]
	[SerializeField]
	private CapMethod _capMode;

	[SerializeField]
	[Range(0f, 360f)]
	[HideInInspector]
	private float _revolve = 360f;

	[SerializeField]
	[HideInInspector]
	private float _capUVScale = 1f;

	[HideInInspector]
	[SerializeField]
	private float _uvTwist;

	private int bodyVertexCount;

	private int bodyTrisCount;

	private int capVertexCount;

	private int capTrisCount;

	public int sides
	{
		get
		{
			return _sides;
		}
		set
		{
			if (value != _sides)
			{
				if (value < 3)
				{
					value = 3;
				}
				_sides = value;
				Rebuild();
			}
		}
	}

	public CapMethod capMode
	{
		get
		{
			return _capMode;
		}
		set
		{
			if (value != _capMode)
			{
				_capMode = value;
				Rebuild();
			}
		}
	}

	public int roundCapLatitude
	{
		get
		{
			return _roundCapLatitude;
		}
		set
		{
			if (value < 1)
			{
				value = 1;
			}
			if (value != _roundCapLatitude)
			{
				_roundCapLatitude = value;
				if (_capMode == CapMethod.Round)
				{
					Rebuild();
				}
			}
		}
	}

	public float revolve
	{
		get
		{
			return _revolve;
		}
		set
		{
			if (value != _revolve)
			{
				_revolve = value;
				Rebuild();
			}
		}
	}

	public float capUVScale
	{
		get
		{
			return _capUVScale;
		}
		set
		{
			if (value != _capUVScale)
			{
				_capUVScale = value;
				Rebuild();
			}
		}
	}

	public float uvTwist
	{
		get
		{
			return _uvTwist;
		}
		set
		{
			if (value != _uvTwist)
			{
				_uvTwist = value;
				Rebuild();
			}
		}
	}

	private bool useCap
	{
		get
		{
			bool flag = _capMode != CapMethod.None;
			if (base.spline != null)
			{
				if (flag)
				{
					if (base.spline.isClosed)
					{
						return base.span < 1.0;
					}
					return true;
				}
				return false;
			}
			return flag;
		}
	}

	protected override void Reset()
	{
		base.Reset();
	}

	protected override void Awake()
	{
		base.Awake();
		mesh.name = "tube";
	}

	protected override void BuildMesh()
	{
		if (_sides > 2)
		{
			base.BuildMesh();
			bodyVertexCount = (_sides + 1) * base.sampleCount;
			CapMethod capMethod = _capMode;
			if (!useCap)
			{
				capMethod = CapMethod.None;
			}
			switch (capMethod)
			{
			case CapMethod.Flat:
				capVertexCount = _sides + 1;
				break;
			case CapMethod.Round:
				capVertexCount = _roundCapLatitude * (sides + 1);
				break;
			default:
				capVertexCount = 0;
				break;
			}
			int vertexCount = bodyVertexCount + capVertexCount * 2;
			bodyTrisCount = _sides * (base.sampleCount - 1) * 2 * 3;
			switch (capMethod)
			{
			case CapMethod.Flat:
				capTrisCount = (_sides - 1) * 3 * 2;
				break;
			case CapMethod.Round:
				capTrisCount = _sides * _roundCapLatitude * 6;
				break;
			default:
				capTrisCount = 0;
				break;
			}
			AllocateMesh(vertexCount, bodyTrisCount + capTrisCount * 2);
			Generate();
			switch (capMethod)
			{
			case CapMethod.Flat:
				GenerateFlatCaps();
				break;
			case CapMethod.Round:
				GenerateRoundCaps();
				break;
			}
		}
	}

	private void Generate()
	{
		int num = 0;
		ResetUVDistance();
		bool flag = base.offset != Vector3.zero;
		for (int i = 0; i < base.sampleCount; i++)
		{
			GetSample(i, evalResult);
			Vector3 position = evalResult.position;
			Vector3 right = evalResult.right;
			float baseSize = GetBaseSize(evalResult);
			if (flag)
			{
				position += base.offset.x * baseSize * right + base.offset.y * baseSize * evalResult.up + base.offset.z * baseSize * evalResult.forward;
			}
			if (base.uvMode == UVMode.UniformClamp || base.uvMode == UVMode.UniformClip)
			{
				AddUVDistance(i);
			}
			Color color = GetBaseColor(evalResult) * base.color;
			for (int j = 0; j < _sides + 1; j++)
			{
				float num2 = (float)j / (float)_sides;
				Quaternion quaternion = Quaternion.AngleAxis(_revolve * num2 + base.rotation + 180f, evalResult.forward);
				tsMesh.vertices[num] = position + quaternion * right * (base.size * baseSize * 0.5f);
				CalculateUVs(evalResult.percent, num2);
				tsMesh.uv[num] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(base.uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - (MeshGenerator.uvs + Vector2.right * ((float)evalResult.percent * _uvTwist))));
				tsMesh.normals[num] = Vector3.Normalize(tsMesh.vertices[num] - position);
				tsMesh.colors[num] = color;
				num++;
			}
		}
		MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _sides, base.sampleCount, flip: false);
	}

	private void GenerateFlatCaps()
	{
		GetSample(0, evalResult);
		for (int i = 0; i < _sides + 1; i++)
		{
			int num = bodyVertexCount + i;
			tsMesh.vertices[num] = tsMesh.vertices[i];
			tsMesh.normals[num] = -evalResult.forward;
			tsMesh.colors[num] = tsMesh.colors[i];
			tsMesh.uv[num] = Quaternion.AngleAxis(_revolve * ((float)i / (float)(_sides - 1)), Vector3.forward) * Vector2.right * (0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
		}
		GetSample(base.sampleCount - 1, evalResult);
		for (int j = 0; j < _sides + 1; j++)
		{
			int num2 = bodyVertexCount + (_sides + 1) + j;
			int num3 = bodyVertexCount - (_sides + 1) + j;
			tsMesh.vertices[num2] = tsMesh.vertices[num3];
			tsMesh.normals[num2] = GetSampleRaw(base.sampleCount - 1).forward;
			tsMesh.colors[num2] = tsMesh.colors[num3];
			tsMesh.uv[num2] = Quaternion.AngleAxis(_revolve * ((float)num3 / (float)(_sides - 1)), Vector3.forward) * Vector2.right * (0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
		}
		int num4 = bodyTrisCount;
		int num5 = ((_revolve == 360f) ? (_sides - 1) : _sides);
		for (int k = 0; k < num5 - 1; k++)
		{
			tsMesh.triangles[num4++] = k + bodyVertexCount + 2;
			tsMesh.triangles[num4++] = k + bodyVertexCount + 1;
			tsMesh.triangles[num4++] = bodyVertexCount;
		}
		for (int l = 0; l < num5 - 1; l++)
		{
			tsMesh.triangles[num4++] = bodyVertexCount + (_sides + 1);
			tsMesh.triangles[num4++] = l + 1 + bodyVertexCount + (_sides + 1);
			tsMesh.triangles[num4++] = l + 2 + bodyVertexCount + (_sides + 1);
		}
	}

	private void GenerateRoundCaps()
	{
		GetSample(0, evalResult);
		Vector3 position = evalResult.position;
		bool flag = base.offset != Vector3.zero;
		float baseSize = GetBaseSize(evalResult);
		if (flag)
		{
			position += base.offset.x * baseSize * evalResult.right + base.offset.y * baseSize * evalResult.up + base.offset.z * baseSize * evalResult.forward;
		}
		Quaternion quaternion = Quaternion.LookRotation(-evalResult.forward, evalResult.up);
		float num = 0f;
		float num2 = 0f;
		switch (base.uvMode)
		{
		case UVMode.Clip:
			num = (float)evalResult.percent;
			num2 = base.size * 0.5f / base.spline.CalculateLength();
			break;
		case UVMode.UniformClip:
			num = base.spline.CalculateLength(0.0, evalResult.percent);
			num2 = base.size * 0.5f;
			break;
		case UVMode.UniformClamp:
			num = 0f;
			num2 = base.size * 0.5f / (float)base.span;
			break;
		case UVMode.Clamp:
			num2 = base.size * 0.5f / base.spline.CalculateLength(base.clipFrom, base.clipTo);
			break;
		}
		Color color = GetBaseColor(evalResult) * base.color;
		for (int i = 1; i < _roundCapLatitude + 1; i++)
		{
			float num3 = (float)i / (float)_roundCapLatitude;
			float angle = 90f * num3;
			for (int j = 0; j <= sides; j++)
			{
				float num4 = (float)j / (float)sides;
				int num5 = bodyVertexCount + j + (i - 1) * (sides + 1);
				Quaternion quaternion2 = Quaternion.AngleAxis(_revolve * num4 + base.rotation + 180f, -Vector3.forward) * Quaternion.AngleAxis(angle, Vector3.up);
				tsMesh.vertices[num5] = position + quaternion * quaternion2 * -Vector3.right * (base.size * 0.5f * evalResult.size);
				tsMesh.colors[num5] = color;
				tsMesh.normals[num5] = (tsMesh.vertices[num5] - position).normalized;
				float num6 = num + num2 * num3;
				Vector2 vector = new Vector2(num4 * base.uvScale.x - num6 * _uvTwist, num6 * base.uvScale.y) - base.uvOffset;
				tsMesh.uv[num5] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(base.uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - vector));
			}
		}
		int num7 = bodyTrisCount;
		for (int k = -1; k < _roundCapLatitude - 1; k++)
		{
			for (int l = 0; l < sides; l++)
			{
				int num8 = bodyVertexCount + l + k * (sides + 1);
				int num9 = num8 + (sides + 1);
				if (k == -1)
				{
					num8 = l;
					num9 = bodyVertexCount + l;
				}
				tsMesh.triangles[num7++] = num9 + 1;
				tsMesh.triangles[num7++] = num8 + 1;
				tsMesh.triangles[num7++] = num8;
				tsMesh.triangles[num7++] = num9;
				tsMesh.triangles[num7++] = num9 + 1;
				tsMesh.triangles[num7++] = num8;
			}
		}
		GetSample(base.sampleCount - 1, evalResult);
		position = evalResult.position;
		baseSize = GetBaseSize(evalResult);
		if (flag)
		{
			position += base.offset.x * baseSize * evalResult.right + base.offset.y * baseSize * evalResult.up + base.offset.z * baseSize * evalResult.forward;
		}
		quaternion = Quaternion.LookRotation(evalResult.forward, evalResult.up);
		switch (base.uvMode)
		{
		case UVMode.Clip:
			num = (float)evalResult.percent;
			break;
		case UVMode.UniformClip:
			num = base.spline.CalculateLength(0.0, evalResult.percent);
			break;
		case UVMode.Clamp:
			num = 1f;
			break;
		case UVMode.UniformClamp:
			num = base.spline.CalculateLength();
			break;
		}
		color = GetBaseColor(evalResult) * base.color;
		for (int m = 1; m < _roundCapLatitude + 1; m++)
		{
			float num10 = (float)m / (float)_roundCapLatitude;
			float angle2 = 90f * num10;
			for (int n = 0; n <= sides; n++)
			{
				float num11 = (float)n / (float)sides;
				int num12 = bodyVertexCount + capVertexCount + n + (m - 1) * (sides + 1);
				Quaternion quaternion3 = Quaternion.AngleAxis(_revolve * num11 + base.rotation + 180f, Vector3.forward) * Quaternion.AngleAxis(angle2, -Vector3.up);
				tsMesh.vertices[num12] = position + quaternion * quaternion3 * Vector3.right * base.size * 0.5f * evalResult.size;
				tsMesh.normals[num12] = (tsMesh.vertices[num12] - position).normalized;
				tsMesh.colors[num12] = color;
				float num13 = num + num2 * num10;
				Vector2 vector2 = new Vector2(num11 * base.uvScale.x + num13 * _uvTwist, num13 * base.uvScale.y) - base.uvOffset;
				tsMesh.uv[num12] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(base.uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - vector2));
			}
		}
		for (int num14 = -1; num14 < _roundCapLatitude - 1; num14++)
		{
			for (int num15 = 0; num15 < sides; num15++)
			{
				int num16 = bodyVertexCount + capVertexCount + num15 + num14 * (sides + 1);
				int num17 = num16 + (sides + 1);
				if (num14 == -1)
				{
					num16 = bodyVertexCount - (_sides + 1) + num15;
					num17 = bodyVertexCount + capVertexCount + num15;
				}
				tsMesh.triangles[num7++] = num16 + 1;
				tsMesh.triangles[num7++] = num17 + 1;
				tsMesh.triangles[num7++] = num17;
				tsMesh.triangles[num7++] = num17;
				tsMesh.triangles[num7++] = num16;
				tsMesh.triangles[num7++] = num16 + 1;
			}
		}
	}
}
