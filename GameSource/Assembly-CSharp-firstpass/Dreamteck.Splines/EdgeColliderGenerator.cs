using UnityEngine;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Edge Collider Generator")]
[RequireComponent(typeof(EdgeCollider2D))]
public class EdgeColliderGenerator : SplineUser
{
	[HideInInspector]
	[SerializeField]
	private float _offset;

	[HideInInspector]
	[SerializeField]
	protected EdgeCollider2D edgeCollider;

	[HideInInspector]
	[SerializeField]
	protected Vector2[] vertices = new Vector2[0];

	[HideInInspector]
	public float updateRate = 0.1f;

	protected float lastUpdateTime;

	private bool updateCollider;

	public float offset
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

	protected override void Awake()
	{
		base.Awake();
		edgeCollider = GetComponent<EdgeCollider2D>();
	}

	protected override void Reset()
	{
		base.Reset();
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
	}

	protected override void LateRun()
	{
		base.LateRun();
		if (updateCollider && edgeCollider != null && Time.time - lastUpdateTime >= updateRate)
		{
			lastUpdateTime = Time.time;
			updateCollider = false;
			edgeCollider.points = vertices;
		}
	}

	protected override void Build()
	{
		base.Build();
		if (vertices.Length != base.sampleCount)
		{
			vertices = new Vector2[base.sampleCount];
		}
		bool flag = offset != 0f;
		for (int i = 0; i < base.sampleCount; i++)
		{
			GetSample(i, evalResult);
			vertices[i] = evalResult.position;
			if (flag)
			{
				Vector2 vector = new Vector2(0f - evalResult.forward.y, evalResult.forward.x).normalized * evalResult.size;
				vertices[i] += vector * offset;
			}
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		if (!(edgeCollider == null))
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = base.transform.InverseTransformPoint(vertices[i]);
			}
			if (updateRate == 0f)
			{
				edgeCollider.points = vertices;
			}
			else
			{
				updateCollider = true;
			}
		}
	}
}
