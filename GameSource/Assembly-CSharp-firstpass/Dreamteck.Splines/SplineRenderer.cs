using UnityEngine;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Dreamteck/Splines/Users/Spline Renderer")]
[RequireComponent(typeof(MeshRenderer))]
public class SplineRenderer : MeshGenerator
{
	[HideInInspector]
	public bool autoOrient = true;

	[HideInInspector]
	public int updateFrameInterval;

	private int currentFrame;

	[HideInInspector]
	[SerializeField]
	private int _slices = 1;

	[HideInInspector]
	[SerializeField]
	private Vector3 vertexDirection = Vector3.up;

	private bool orthographic;

	private bool init;

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
		mesh.name = "spline";
	}

	private void Start()
	{
		if (Camera.current != null)
		{
			orthographic = Camera.current.orthographic;
		}
	}

	protected override void LateRun()
	{
		if (updateFrameInterval > 0)
		{
			currentFrame++;
			if (currentFrame > updateFrameInterval)
			{
				currentFrame = 0;
			}
		}
	}

	protected override void BuildMesh()
	{
		base.BuildMesh();
		GenerateVertices(vertexDirection, orthographic);
		MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, base.sampleCount, flip: false);
	}

	public void RenderWithCamera(Camera cam)
	{
		orthographic = true;
		if (cam != null)
		{
			if (cam.orthographic)
			{
				vertexDirection = -cam.transform.forward;
			}
			else
			{
				vertexDirection = cam.transform.position;
			}
			orthographic = cam.orthographic;
		}
		BuildMesh();
		WriteMesh();
	}

	private void OnWillRenderObject()
	{
		if (autoOrient && (updateFrameInterval <= 0 || currentFrame == 0))
		{
			if (!Application.isPlaying && !init)
			{
				Awake();
				init = true;
			}
			RenderWithCamera(Camera.current);
		}
	}

	public void GenerateVertices(Vector3 vertexDirection, bool orthoGraphic)
	{
		AllocateMesh((_slices + 1) * base.sampleCount, _slices * (base.sampleCount - 1) * 6);
		int num = 0;
		ResetUVDistance();
		bool flag = base.offset != Vector3.zero;
		for (int i = 0; i < base.sampleCount; i++)
		{
			GetSample(i, evalResult);
			Vector3 position = evalResult.position;
			if (flag)
			{
				position += base.offset.x * -Vector3.Cross(evalResult.forward, evalResult.up) + base.offset.y * evalResult.up + base.offset.z * evalResult.forward;
			}
			Vector3 vector = ((!orthoGraphic) ? (vertexDirection - position).normalized : vertexDirection);
			Vector3 normalized = Vector3.Cross(evalResult.forward, vector).normalized;
			if (base.uvMode == UVMode.UniformClamp || base.uvMode == UVMode.UniformClip)
			{
				AddUVDistance(i);
			}
			Color color = evalResult.color * base.color;
			for (int j = 0; j < _slices + 1; j++)
			{
				float num2 = (float)j / (float)_slices;
				tsMesh.vertices[num] = position - normalized * evalResult.size * 0.5f * base.size + normalized * evalResult.size * num2 * base.size;
				CalculateUVs(evalResult.percent, num2);
				tsMesh.uv[num] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(base.uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - MeshGenerator.uvs));
				tsMesh.normals[num] = vector;
				tsMesh.colors[num] = color;
				num++;
			}
		}
	}
}
