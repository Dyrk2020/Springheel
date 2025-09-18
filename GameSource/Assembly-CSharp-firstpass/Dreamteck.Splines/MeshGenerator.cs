using UnityEngine;

namespace Dreamteck.Splines;

public class MeshGenerator : SplineUser
{
	public enum UVMode
	{
		Clip,
		UniformClip,
		Clamp,
		UniformClamp
	}

	public enum NormalMethod
	{
		Recalculate,
		SplineNormals
	}

	protected const int UNITY_VERTEX_LIMIT = 65534;

	[SerializeField]
	[HideInInspector]
	private bool _baked;

	[SerializeField]
	[HideInInspector]
	private bool _markDynamic = true;

	[SerializeField]
	[HideInInspector]
	private float _size = 1f;

	[SerializeField]
	[HideInInspector]
	private Color _color = Color.white;

	[SerializeField]
	[HideInInspector]
	private Vector3 _offset = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private NormalMethod _normalMethod = NormalMethod.SplineNormals;

	[HideInInspector]
	[SerializeField]
	private bool _calculateTangents = true;

	[HideInInspector]
	[SerializeField]
	private bool _useSplineSize = true;

	[HideInInspector]
	[SerializeField]
	private bool _useSplineColor = true;

	[HideInInspector]
	[SerializeField]
	[Range(-360f, 360f)]
	private float _rotation;

	[SerializeField]
	[HideInInspector]
	private bool _flipFaces;

	[SerializeField]
	[HideInInspector]
	private bool _doubleSided;

	[HideInInspector]
	[SerializeField]
	private UVMode _uvMode;

	[SerializeField]
	[HideInInspector]
	private Vector2 _uvScale = Vector2.one;

	[SerializeField]
	[HideInInspector]
	private Vector2 _uvOffset = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private float _uvRotation;

	[HideInInspector]
	[SerializeField]
	protected MeshCollider meshCollider;

	[SerializeField]
	[HideInInspector]
	protected MeshFilter filter;

	[SerializeField]
	[HideInInspector]
	protected MeshRenderer meshRenderer;

	[SerializeField]
	[HideInInspector]
	protected TS_Mesh tsMesh = new TS_Mesh();

	[HideInInspector]
	[SerializeField]
	protected Mesh mesh;

	[HideInInspector]
	public float colliderUpdateRate = 0.2f;

	protected bool updateCollider;

	protected float lastUpdateTime;

	private float vDist;

	protected static Vector2 uvs = Vector2.zero;

	public float size
	{
		get
		{
			return _size;
		}
		set
		{
			if (value != _size)
			{
				_size = value;
				Rebuild();
			}
			else
			{
				_size = value;
			}
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (value != _color)
			{
				_color = value;
				Rebuild();
			}
		}
	}

	public Vector3 offset
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
				Rebuild();
			}
		}
	}

	public NormalMethod normalMethod
	{
		get
		{
			return _normalMethod;
		}
		set
		{
			if (value != _normalMethod)
			{
				_normalMethod = value;
				Rebuild();
			}
		}
	}

	public bool useSplineSize
	{
		get
		{
			return _useSplineSize;
		}
		set
		{
			if (value != _useSplineSize)
			{
				_useSplineSize = value;
				Rebuild();
			}
		}
	}

	public bool useSplineColor
	{
		get
		{
			return _useSplineColor;
		}
		set
		{
			if (value != _useSplineColor)
			{
				_useSplineColor = value;
				Rebuild();
			}
		}
	}

	public bool calculateTangents
	{
		get
		{
			return _calculateTangents;
		}
		set
		{
			if (value != _calculateTangents)
			{
				_calculateTangents = value;
				Rebuild();
			}
		}
	}

	public float rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			if (value != _rotation)
			{
				_rotation = value;
				Rebuild();
			}
		}
	}

	public bool flipFaces
	{
		get
		{
			return _flipFaces;
		}
		set
		{
			if (value != _flipFaces)
			{
				_flipFaces = value;
				Rebuild();
			}
		}
	}

	public bool doubleSided
	{
		get
		{
			return _doubleSided;
		}
		set
		{
			if (value != _doubleSided)
			{
				_doubleSided = value;
				Rebuild();
			}
		}
	}

	public UVMode uvMode
	{
		get
		{
			return _uvMode;
		}
		set
		{
			if (value != _uvMode)
			{
				_uvMode = value;
				Rebuild();
			}
		}
	}

	public Vector2 uvScale
	{
		get
		{
			return _uvScale;
		}
		set
		{
			if (value != _uvScale)
			{
				_uvScale = value;
				Rebuild();
			}
		}
	}

	public Vector2 uvOffset
	{
		get
		{
			return _uvOffset;
		}
		set
		{
			if (value != _uvOffset)
			{
				_uvOffset = value;
				Rebuild();
			}
		}
	}

	public float uvRotation
	{
		get
		{
			return _uvRotation;
		}
		set
		{
			if (value != _uvRotation)
			{
				_uvRotation = value;
				Rebuild();
			}
		}
	}

	public bool baked => _baked;

	public bool markDynamic
	{
		get
		{
			return _markDynamic;
		}
		set
		{
			if (value != _markDynamic)
			{
				_markDynamic = value;
				if (!_markDynamic)
				{
					Object.Destroy(mesh);
					mesh = new Mesh();
				}
				RebuildImmediate();
			}
		}
	}

	protected override void Awake()
	{
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		base.Awake();
		filter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
	}

	protected override void Reset()
	{
		base.Reset();
	}

	public void CloneMesh()
	{
		if (tsMesh != null)
		{
			tsMesh = TS_Mesh.Copy(tsMesh);
		}
		else
		{
			tsMesh = new TS_Mesh();
		}
		if (mesh != null)
		{
			mesh = Object.Instantiate(mesh);
		}
		else
		{
			mesh = new Mesh();
		}
	}

	public override void Rebuild()
	{
		if (!_baked)
		{
			base.Rebuild();
		}
	}

	public override void RebuildImmediate()
	{
		if (!_baked)
		{
			base.RebuildImmediate();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MeshFilter component = GetComponent<MeshFilter>();
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.hideFlags = HideFlags.None;
		}
		if (component2 != null)
		{
			component2.hideFlags = HideFlags.None;
		}
	}

	public void UpdateCollider()
	{
		meshCollider = GetComponent<MeshCollider>();
		if (meshCollider == null)
		{
			meshCollider = base.gameObject.AddComponent<MeshCollider>();
		}
		meshCollider.sharedMesh = filter.sharedMesh;
	}

	protected override void LateRun()
	{
		if (!_baked)
		{
			base.LateRun();
			if (updateCollider && meshCollider != null && Time.time - lastUpdateTime >= colliderUpdateRate)
			{
				lastUpdateTime = Time.time;
				updateCollider = false;
				meshCollider.sharedMesh = filter.sharedMesh;
			}
		}
	}

	protected override void Build()
	{
		base.Build();
		BuildMesh();
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		WriteMesh();
	}

	protected virtual void BuildMesh()
	{
	}

	protected virtual void WriteMesh()
	{
		MeshUtility.InverseTransformMesh(tsMesh, base.trs);
		if (_doubleSided)
		{
			MeshUtility.MakeDoublesidedHalf(tsMesh);
		}
		else if (_flipFaces)
		{
			MeshUtility.FlipFaces(tsMesh);
		}
		if (_calculateTangents)
		{
			MeshUtility.CalculateTangents(tsMesh);
		}
		if (tsMesh.vertexCount > 65534)
		{
			Debug.LogError("WARNING: The generated mesh for " + base.name + " has " + tsMesh.vertexCount + " vertices. The maximum vertex count for meshes in Unity is " + 65534 + ". The mesh will not be updated.");
		}
		if (_markDynamic)
		{
			mesh.MarkDynamic();
		}
		tsMesh.WriteMesh(ref mesh);
		if (_normalMethod == NormalMethod.Recalculate)
		{
			mesh.RecalculateNormals();
		}
		if (filter != null)
		{
			filter.sharedMesh = mesh;
		}
		updateCollider = true;
	}

	protected virtual void AllocateMesh(int vertexCount, int trisCount)
	{
		if (trisCount < 0)
		{
			trisCount = 0;
		}
		if (vertexCount < 0)
		{
			vertexCount = 0;
		}
		if (_doubleSided)
		{
			vertexCount *= 2;
			trisCount *= 2;
		}
		if (tsMesh.vertexCount != vertexCount)
		{
			tsMesh.vertices = new Vector3[vertexCount];
			tsMesh.normals = new Vector3[vertexCount];
			tsMesh.tangents = new Vector4[vertexCount];
			tsMesh.colors = new Color[vertexCount];
			tsMesh.uv = new Vector2[vertexCount];
		}
		if (tsMesh.triangles.Length != trisCount)
		{
			tsMesh.triangles = new int[trisCount];
		}
	}

	protected void ResetUVDistance()
	{
		vDist = 0f;
		if (uvMode == UVMode.UniformClip)
		{
			vDist = base.spline.CalculateLength(0.0, GetSampleRaw(0).percent);
		}
	}

	protected void AddUVDistance(int sampleIndex)
	{
		if (sampleIndex != 0)
		{
			vDist += Vector3.Distance(GetSampleRaw(sampleIndex).position, GetSampleRaw(sampleIndex - 1).position);
		}
	}

	protected void CalculateUVs(double percent, float u)
	{
		uvs.x = u * _uvScale.x - _uvOffset.x;
		switch (uvMode)
		{
		case UVMode.Clip:
			uvs.y = (float)percent * _uvScale.y - _uvOffset.y;
			break;
		case UVMode.Clamp:
			uvs.y = (float)DMath.InverseLerp(base.clipFrom, base.clipTo, percent) * _uvScale.y - _uvOffset.y;
			break;
		case UVMode.UniformClamp:
			uvs.y = vDist * _uvScale.y / (float)base.span - _uvOffset.y;
			break;
		default:
			uvs.y = vDist * _uvScale.y - _uvOffset.y;
			break;
		}
	}

	protected float GetBaseSize(SplineSample sample)
	{
		if (!_useSplineSize)
		{
			return 1f;
		}
		return sample.size;
	}

	protected Color GetBaseColor(SplineSample sample)
	{
		if (!_useSplineColor)
		{
			return Color.white;
		}
		return sample.color;
	}
}
