using UnityEngine;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Surface Generator")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SurfaceGenerator : MeshGenerator
{
	[SerializeField]
	[HideInInspector]
	private float _expand;

	[SerializeField]
	[HideInInspector]
	private float _extrude;

	[SerializeField]
	[HideInInspector]
	private Vector2 _sideUvScale = Vector2.one;

	[SerializeField]
	[HideInInspector]
	private Vector2 _sideUvOffset = Vector2.zero;

	[SerializeField]
	[HideInInspector]
	private SplineComputer _extrudeSpline;

	[SerializeField]
	[HideInInspector]
	private SplineSample[] extrudeResults = new SplineSample[0];

	[SerializeField]
	[HideInInspector]
	private Vector3[] identityVertices = new Vector3[0];

	[SerializeField]
	[HideInInspector]
	private Vector3[] identityNormals = new Vector3[0];

	[SerializeField]
	[HideInInspector]
	private Vector2[] projectedVerts = new Vector2[0];

	[SerializeField]
	[HideInInspector]
	private int[] surfaceTris = new int[0];

	[SerializeField]
	[HideInInspector]
	private int[] wallTris = new int[0];

	[SerializeField]
	[HideInInspector]
	private double _extrudeFrom;

	[SerializeField]
	[HideInInspector]
	private double _extrudeTo = 1.0;

	[SerializeField]
	[HideInInspector]
	private bool _uniformUvs;

	public float expand
	{
		get
		{
			return _expand;
		}
		set
		{
			if (value != _expand)
			{
				_expand = value;
				Rebuild();
			}
		}
	}

	public float extrude
	{
		get
		{
			return _extrude;
		}
		set
		{
			if (value != _extrude)
			{
				_extrude = value;
				Rebuild();
			}
		}
	}

	public double extrudeClipFrom
	{
		get
		{
			return _extrudeFrom;
		}
		set
		{
			if (value != _extrudeFrom)
			{
				_extrudeFrom = value;
				Rebuild();
			}
		}
	}

	public double extrudeClipTo
	{
		get
		{
			return _extrudeTo;
		}
		set
		{
			if (value != _extrudeTo)
			{
				_extrudeTo = value;
				Rebuild();
			}
		}
	}

	public Vector2 sideUvScale
	{
		get
		{
			return _sideUvScale;
		}
		set
		{
			if (value != _sideUvScale)
			{
				_sideUvScale = value;
				Rebuild();
			}
			else
			{
				_sideUvScale = value;
			}
		}
	}

	public Vector2 sideUvOffset
	{
		get
		{
			return _sideUvOffset;
		}
		set
		{
			if (value != _sideUvOffset)
			{
				_sideUvOffset = value;
				Rebuild();
			}
			else
			{
				_sideUvOffset = value;
			}
		}
	}

	public SplineComputer extrudeSpline
	{
		get
		{
			return _extrudeSpline;
		}
		set
		{
			if (value != _extrudeSpline)
			{
				if (_extrudeSpline != null)
				{
					_extrudeSpline.Unsubscribe(this);
				}
				_extrudeSpline = value;
				if (value != null)
				{
					_extrudeSpline.Subscribe(this);
				}
				Rebuild();
			}
		}
	}

	public bool uniformUvs
	{
		get
		{
			return _uniformUvs;
		}
		set
		{
			if (value != _uniformUvs)
			{
				_uniformUvs = value;
				Rebuild();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mesh.name = "surface";
	}

	protected override void BuildMesh()
	{
		if (base.spline.pointCount != 0)
		{
			base.BuildMesh();
			Generate();
		}
	}

	public void Generate()
	{
		int num = base.sampleCount;
		if (base.spline.isClosed)
		{
			num--;
		}
		int num2 = num;
		if (_extrudeSpline != null)
		{
			_extrudeSpline.Evaluate(ref extrudeResults, _extrudeFrom, _extrudeTo);
		}
		else if (extrudeResults.Length != 0)
		{
			extrudeResults = new SplineSample[0];
		}
		bool flag = (bool)_extrudeSpline && extrudeResults.Length != 0;
		bool flag2 = !flag && _extrude != 0f;
		if (flag)
		{
			num2 *= 2;
			num2 += base.sampleCount * extrudeResults.Length;
		}
		else if (flag2)
		{
			num2 *= 4;
			num2 += 2;
		}
		GetProjectedVertices(num, out var center, out var normal);
		bool flag3 = IsClockwise(projectedVerts);
		bool flag4 = false;
		bool flag5 = false;
		if (!flag3)
		{
			flag5 = !flag5;
		}
		if (flag2 && _extrude < 0f)
		{
			flag4 = !flag4;
			flag5 = !flag5;
		}
		GenerateSurfaceTris(flag4);
		int num3 = surfaceTris.Length;
		if (flag2)
		{
			num3 *= 2;
			num3 += 2 * base.sampleCount * 2 * 3;
		}
		else
		{
			num3 *= 2;
			num3 += extrudeResults.Length * base.sampleCount * 2 * 3;
		}
		AllocateMesh(num2, num3);
		Vector3 vector = base.trs.right * base.offset.x + base.trs.up * base.offset.y + base.trs.forward * base.offset.z;
		for (int i = 0; i < num; i++)
		{
			GetSample(i, evalResult);
			tsMesh.vertices[i] = evalResult.position + vector;
			tsMesh.normals[i] = evalResult.up;
			tsMesh.colors[i] = evalResult.color * base.color;
		}
		Vector2 vector2 = projectedVerts[0];
		Vector2 vector3 = projectedVerts[0];
		for (int j = 1; j < projectedVerts.Length; j++)
		{
			if (vector2.x < projectedVerts[j].x)
			{
				vector2.x = projectedVerts[j].x;
			}
			if (vector2.y < projectedVerts[j].y)
			{
				vector2.y = projectedVerts[j].y;
			}
			if (vector3.x > projectedVerts[j].x)
			{
				vector3.x = projectedVerts[j].x;
			}
			if (vector3.y > projectedVerts[j].y)
			{
				vector3.y = projectedVerts[j].y;
			}
		}
		for (int k = 0; k < projectedVerts.Length; k++)
		{
			tsMesh.uv[k].x = Mathf.InverseLerp(vector3.x, vector2.x, projectedVerts[k].x) * base.uvScale.x - base.uvScale.x * 0.5f + base.uvOffset.x + 0.5f;
			tsMesh.uv[k].y = Mathf.InverseLerp(vector2.y, vector3.y, projectedVerts[k].y) * base.uvScale.y - base.uvScale.y * 0.5f + base.uvOffset.y + 0.5f;
		}
		if (flag4)
		{
			for (int l = 0; l < num; l++)
			{
				tsMesh.normals[l] *= -1f;
			}
		}
		if (_expand != 0f)
		{
			for (int m = 0; m < num; m++)
			{
				GetSample(m, evalResult);
				tsMesh.vertices[m] += (flag3 ? (-evalResult.right) : evalResult.right) * _expand;
			}
		}
		if (flag)
		{
			GetIdentityVerts(center, normal, flag3);
			for (int n = 0; n < num; n++)
			{
				tsMesh.vertices[n + num] = extrudeResults[0].position + extrudeResults[0].rotation * identityVertices[n] + vector;
				tsMesh.normals[n + num] = -extrudeResults[0].forward;
				tsMesh.colors[n + num] = tsMesh.colors[n] * extrudeResults[0].color;
				tsMesh.uv[n + num] = new Vector2(1f - tsMesh.uv[n].x, tsMesh.uv[n].y);
				tsMesh.vertices[n] = extrudeResults[extrudeResults.Length - 1].position + extrudeResults[extrudeResults.Length - 1].rotation * identityVertices[n] + vector;
				tsMesh.normals[n] = extrudeResults[extrudeResults.Length - 1].forward;
				tsMesh.colors[n] *= extrudeResults[extrudeResults.Length - 1].color;
			}
			float num4 = 0f;
			for (int num5 = 0; num5 < extrudeResults.Length; num5++)
			{
				if (_uniformUvs && num5 > 0)
				{
					num4 += Vector3.Distance(extrudeResults[num5].position, extrudeResults[num5 - 1].position);
				}
				int num6 = num * 2 + num5 * base.sampleCount;
				for (int num7 = 0; num7 < identityVertices.Length; num7++)
				{
					tsMesh.vertices[num6 + num7] = extrudeResults[num5].position + extrudeResults[num5].rotation * identityVertices[num7] + vector;
					tsMesh.normals[num6 + num7] = extrudeResults[num5].rotation * identityNormals[num7];
					if (_uniformUvs)
					{
						tsMesh.uv[num6 + num7] = new Vector2((float)num7 / (float)(identityVertices.Length - 1) * _sideUvScale.x + _sideUvOffset.x, num4 * _sideUvScale.y + _sideUvOffset.y);
					}
					else
					{
						tsMesh.uv[num6 + num7] = new Vector2((float)num7 / (float)(identityVertices.Length - 1) * _sideUvScale.x + _sideUvOffset.x, (float)num5 / (float)(extrudeResults.Length - 1) * _sideUvScale.y + _sideUvOffset.y);
					}
					if (flag3)
					{
						tsMesh.uv[num6 + num7].x = 1f - tsMesh.uv[num6 + num7].x;
					}
				}
			}
			int trisOffset = WriteTris(ref surfaceTris, ref tsMesh.triangles, 0, 0, flip: false);
			trisOffset = WriteTris(ref surfaceTris, ref tsMesh.triangles, num, trisOffset, flip: true);
			MeshUtility.GeneratePlaneTriangles(ref wallTris, base.sampleCount - 1, extrudeResults.Length, flag5, 0, 0, reallocateArray: true);
			WriteTris(ref wallTris, ref tsMesh.triangles, num * 2, trisOffset, flip: false);
		}
		else if (flag2)
		{
			for (int num8 = 0; num8 < num; num8++)
			{
				tsMesh.vertices[num8 + num] = tsMesh.vertices[num8];
				tsMesh.normals[num8 + num] = -tsMesh.normals[num8];
				tsMesh.colors[num8 + num] = tsMesh.colors[num8];
				tsMesh.uv[num8 + num] = new Vector2(1f - tsMesh.uv[num8].x, tsMesh.uv[num8].y);
				tsMesh.vertices[num8] += normal * _extrude;
			}
			for (int num9 = 0; num9 < num + 1; num9++)
			{
				int num10 = num9;
				if (num9 >= num)
				{
					num10 = num9 - num;
				}
				GetSample(num10, evalResult);
				tsMesh.vertices[num9 + num * 2] = tsMesh.vertices[num10] - normal * _extrude;
				tsMesh.normals[num9 + num * 2] = (flag3 ? (-evalResult.right) : evalResult.right);
				tsMesh.colors[num9 + num * 2] = tsMesh.colors[num10];
				tsMesh.uv[num9 + num * 2] = new Vector2((float)num9 / (float)(num - 1) * _sideUvScale.x + _sideUvOffset.x, 0f + _sideUvOffset.y);
				if (flag3)
				{
					tsMesh.uv[num9 + num * 2].x = 1f - tsMesh.uv[num9 + num * 2].x;
				}
				int num11 = num9 + num * 3 + 1;
				tsMesh.vertices[num11] = tsMesh.vertices[num10];
				tsMesh.normals[num11] = tsMesh.normals[num9 + num * 2];
				tsMesh.colors[num11] = tsMesh.colors[num10];
				if (_uniformUvs)
				{
					tsMesh.uv[num11] = new Vector2((float)num9 / (float)num * _sideUvScale.x + _sideUvOffset.x, _extrude * _sideUvScale.y + _sideUvOffset.y);
				}
				else
				{
					tsMesh.uv[num11] = new Vector2((float)num9 / (float)num * _sideUvScale.x + _sideUvOffset.x, 1f * _sideUvScale.y + _sideUvOffset.y);
				}
				if (flag3)
				{
					tsMesh.uv[num11].x = 1f - tsMesh.uv[num11].x;
				}
			}
			int trisOffset2 = WriteTris(ref surfaceTris, ref tsMesh.triangles, 0, 0, flip: false);
			trisOffset2 = WriteTris(ref surfaceTris, ref tsMesh.triangles, num, trisOffset2, flip: true);
			MeshUtility.GeneratePlaneTriangles(ref wallTris, base.sampleCount - 1, 2, flag5, 0, 0, reallocateArray: true);
			WriteTris(ref wallTris, ref tsMesh.triangles, num * 2, trisOffset2, flip: false);
		}
		else
		{
			WriteTris(ref surfaceTris, ref tsMesh.triangles, 0, 0, flip: false);
		}
	}

	private void GenerateSurfaceTris(bool flip)
	{
		MeshUtility.Triangulate(projectedVerts, ref surfaceTris);
		if (flip)
		{
			MeshUtility.FlipTriangles(ref surfaceTris);
		}
	}

	private int WriteTris(ref int[] tris, ref int[] target, int vertexOffset, int trisOffset, bool flip)
	{
		for (int i = trisOffset; i < trisOffset + tris.Length; i += 3)
		{
			if (flip)
			{
				target[i] = tris[i + 2 - trisOffset] + vertexOffset;
				target[i + 1] = tris[i + 1 - trisOffset] + vertexOffset;
				target[i + 2] = tris[i - trisOffset] + vertexOffset;
			}
			else
			{
				target[i] = tris[i - trisOffset] + vertexOffset;
				target[i + 1] = tris[i + 1 - trisOffset] + vertexOffset;
				target[i + 2] = tris[i + 2 - trisOffset] + vertexOffset;
			}
		}
		return trisOffset + tris.Length;
	}

	private bool IsClockwise(Vector2[] points2D)
	{
		float num = 0f;
		for (int i = 1; i < points2D.Length; i++)
		{
			Vector2 vector = points2D[i];
			Vector2 vector2 = points2D[(i + 1) % points2D.Length];
			num += (vector2.x - vector.x) * (vector2.y + vector.y);
		}
		num += (points2D[0].x - points2D[^1].x) * (points2D[0].y + points2D[^1].y);
		return num <= 0f;
	}

	private void GetIdentityVerts(Vector3 center, Vector3 normal, bool clockwise)
	{
		Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(normal));
		if (identityVertices.Length != base.sampleCount)
		{
			identityVertices = new Vector3[base.sampleCount];
			identityNormals = new Vector3[base.sampleCount];
		}
		for (int i = 0; i < base.sampleCount; i++)
		{
			identityVertices[i] = quaternion * (GetSampleRaw(i).position - center + (clockwise ? (-GetSampleRaw(i).right) : GetSampleRaw(i).right) * _expand);
			identityNormals[i] = quaternion * (clockwise ? (-GetSampleRaw(i).right) : GetSampleRaw(i).right);
		}
	}

	private void GetProjectedVertices(int count, out Vector3 center, out Vector3 normal)
	{
		center = Vector3.zero;
		normal = Vector3.zero;
		Vector3 vector = base.trs.right * base.offset.x + base.trs.up * base.offset.y + base.trs.forward * base.offset.z;
		for (int i = 0; i < count; i++)
		{
			center += GetSampleRaw(i).position + vector;
			normal += GetSampleRaw(i).up;
		}
		normal.Normalize();
		center /= (float)count;
		Quaternion quaternion = Quaternion.LookRotation(normal, Vector3.up);
		Vector3 vector2 = quaternion * Vector3.up;
		Vector3 vector3 = quaternion * Vector3.right;
		if (projectedVerts.Length != count)
		{
			projectedVerts = new Vector2[count];
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 vector4 = GetSampleRaw(j).position + vector - center;
			float num = Vector3.Project(vector4, vector3).magnitude;
			if (Vector3.Dot(vector4, vector3) < 0f)
			{
				num *= -1f;
			}
			float num2 = Vector3.Project(vector4, vector2).magnitude;
			if (Vector3.Dot(vector4, vector2) < 0f)
			{
				num2 *= -1f;
			}
			projectedVerts[j].x = num;
			projectedVerts[j].y = num2;
		}
	}
}
