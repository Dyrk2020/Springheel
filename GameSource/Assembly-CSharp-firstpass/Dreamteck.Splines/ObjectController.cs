using System;
using System.Collections;
using UnityEngine;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Object Controller")]
public class ObjectController : SplineUser
{
	[Serializable]
	internal class ObjectControl
	{
		public GameObject gameObject;

		public Vector3 position = Vector3.zero;

		public Quaternion rotation = Quaternion.identity;

		public Vector3 scale = Vector3.one;

		public bool active = true;

		public Vector3 baseScale = Vector3.one;

		public bool isNull => gameObject == null;

		public Transform transform
		{
			get
			{
				if (gameObject == null)
				{
					return null;
				}
				return gameObject.transform;
			}
		}

		public ObjectControl(GameObject input)
		{
			gameObject = input;
			baseScale = gameObject.transform.localScale;
		}

		public void Destroy()
		{
			if (!(gameObject == null))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}

		public void DestroyImmediate()
		{
			if (!(gameObject == null))
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}

		public void Apply()
		{
			if (!(gameObject == null))
			{
				transform.position = position;
				transform.rotation = rotation;
				transform.localScale = scale;
				gameObject.SetActive(active);
			}
		}
	}

	public enum ObjectMethod
	{
		Instantiate,
		GetChildren
	}

	public enum Positioning
	{
		Stretch,
		Clip
	}

	public enum Iteration
	{
		Ordered,
		Random
	}

	[HideInInspector]
	[SerializeField]
	public GameObject[] objects = new GameObject[0];

	[HideInInspector]
	[SerializeField]
	private float _evaluateOffset;

	[SerializeField]
	[HideInInspector]
	private int _spawnCount;

	[SerializeField]
	[HideInInspector]
	private Positioning _objectPositioning;

	[SerializeField]
	[HideInInspector]
	private Iteration _iteration;

	[SerializeField]
	[HideInInspector]
	private int _randomSeed = 1;

	[SerializeField]
	[HideInInspector]
	private Vector3 _minOffset = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private Vector3 _maxOffset = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private bool _offsetUseWorldCoords;

	[SerializeField]
	[HideInInspector]
	private Vector3 _minRotation = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private Vector3 _maxRotation = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private bool _uniformScaleLerp = true;

	[SerializeField]
	[HideInInspector]
	private Vector3 _minScaleMultiplier = Vector3.one;

	[SerializeField]
	[HideInInspector]
	private Vector3 _maxScaleMultiplier = Vector3.one;

	[SerializeField]
	[HideInInspector]
	private bool _shellOffset;

	[SerializeField]
	[HideInInspector]
	private bool _applyRotation = true;

	[HideInInspector]
	[SerializeField]
	private bool _rotateByOffset;

	[HideInInspector]
	[SerializeField]
	private bool _applyScale;

	[HideInInspector]
	[SerializeField]
	private ObjectMethod _objectMethod;

	[HideInInspector]
	public bool delayedSpawn;

	[HideInInspector]
	public float spawnDelay = 0.1f;

	[HideInInspector]
	[SerializeField]
	private int lastChildCount;

	[SerializeField]
	[HideInInspector]
	private ObjectControl[] spawned = new ObjectControl[0];

	private System.Random offsetRandomizer;

	private System.Random shellRandomizer;

	private System.Random rotationRandomizer;

	private System.Random scaleRandomizer;

	public ObjectMethod objectMethod
	{
		get
		{
			return _objectMethod;
		}
		set
		{
			if (value != _objectMethod)
			{
				if (value == ObjectMethod.GetChildren)
				{
					_objectMethod = value;
					Spawn();
				}
				else
				{
					_objectMethod = value;
				}
			}
		}
	}

	public int spawnCount
	{
		get
		{
			return _spawnCount;
		}
		set
		{
			if (value == _spawnCount)
			{
				return;
			}
			if (value < 0)
			{
				value = 0;
			}
			if (_objectMethod == ObjectMethod.Instantiate)
			{
				if (value < _spawnCount)
				{
					_spawnCount = value;
					Remove();
				}
				else
				{
					_spawnCount = value;
					Spawn();
				}
			}
			else
			{
				_spawnCount = value;
			}
		}
	}

	public Positioning objectPositioning
	{
		get
		{
			return _objectPositioning;
		}
		set
		{
			if (value != _objectPositioning)
			{
				_objectPositioning = value;
				Rebuild();
			}
		}
	}

	public Iteration iteration
	{
		get
		{
			return _iteration;
		}
		set
		{
			if (value != _iteration)
			{
				_iteration = value;
				Rebuild();
			}
		}
	}

	public int randomSeed
	{
		get
		{
			return _randomSeed;
		}
		set
		{
			if (value != _randomSeed)
			{
				_randomSeed = value;
				Rebuild();
			}
		}
	}

	public Vector3 minOffset
	{
		get
		{
			return _minOffset;
		}
		set
		{
			if (value != _minOffset)
			{
				_minOffset = value;
				Rebuild();
			}
		}
	}

	public Vector3 maxOffset
	{
		get
		{
			return _maxOffset;
		}
		set
		{
			if (value != _maxOffset)
			{
				_maxOffset = value;
				Rebuild();
			}
		}
	}

	public bool offsetUseWorldCoords
	{
		get
		{
			return _offsetUseWorldCoords;
		}
		set
		{
			if (value != _offsetUseWorldCoords)
			{
				_offsetUseWorldCoords = value;
				Rebuild();
			}
		}
	}

	public Vector3 minRotation
	{
		get
		{
			return _minRotation;
		}
		set
		{
			if (value != _minRotation)
			{
				_minRotation = value;
				Rebuild();
			}
		}
	}

	public Vector3 maxRotation
	{
		get
		{
			return _maxRotation;
		}
		set
		{
			if (value != _maxRotation)
			{
				_maxRotation = value;
				Rebuild();
			}
		}
	}

	public Vector3 rotationOffset
	{
		get
		{
			return (_maxRotation + _minRotation) / 2f;
		}
		set
		{
			if (value != _minRotation || value != _maxRotation)
			{
				_minRotation = (_maxRotation = value);
				Rebuild();
			}
		}
	}

	public Vector3 minScaleMultiplier
	{
		get
		{
			return _minScaleMultiplier;
		}
		set
		{
			if (value != _minScaleMultiplier)
			{
				_minScaleMultiplier = value;
				Rebuild();
			}
		}
	}

	public Vector3 maxScaleMultiplier
	{
		get
		{
			return _maxScaleMultiplier;
		}
		set
		{
			if (value != _maxScaleMultiplier)
			{
				_maxScaleMultiplier = value;
				Rebuild();
			}
		}
	}

	public bool uniformScaleLerp
	{
		get
		{
			return _uniformScaleLerp;
		}
		set
		{
			if (value != _uniformScaleLerp)
			{
				_uniformScaleLerp = value;
				Rebuild();
			}
		}
	}

	public bool shellOffset
	{
		get
		{
			return _shellOffset;
		}
		set
		{
			if (value != _shellOffset)
			{
				_shellOffset = value;
				Rebuild();
			}
		}
	}

	public bool applyRotation
	{
		get
		{
			return _applyRotation;
		}
		set
		{
			if (value != _applyRotation)
			{
				_applyRotation = value;
				Rebuild();
			}
		}
	}

	public bool rotateByOffset
	{
		get
		{
			return _rotateByOffset;
		}
		set
		{
			if (value != _rotateByOffset)
			{
				_rotateByOffset = value;
				Rebuild();
			}
		}
	}

	public bool applyScale
	{
		get
		{
			return _applyScale;
		}
		set
		{
			if (value != _applyScale)
			{
				_applyScale = value;
				Rebuild();
			}
		}
	}

	public float evaluateOffset
	{
		get
		{
			return _evaluateOffset;
		}
		set
		{
			if (value != _evaluateOffset)
			{
				_evaluateOffset = value;
				Rebuild();
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < spawned.Length; i++)
		{
			if (spawned[i] != null && !(spawned[i].transform == null))
			{
				spawned[i].transform.localScale = spawned[i].baseScale;
				if (_objectMethod == ObjectMethod.GetChildren)
				{
					spawned[i].gameObject.SetActive(value: false);
				}
				else
				{
					spawned[i].Destroy();
				}
			}
		}
		spawned = new ObjectControl[0];
	}

	private void OnValidate()
	{
		if (_spawnCount < 0)
		{
			_spawnCount = 0;
		}
	}

	private void Remove()
	{
		if (_spawnCount >= spawned.Length)
		{
			return;
		}
		int num = spawned.Length - 1;
		while (num >= _spawnCount && num < spawned.Length)
		{
			if (spawned[num] != null)
			{
				spawned[num].transform.localScale = spawned[num].baseScale;
				if (_objectMethod == ObjectMethod.GetChildren)
				{
					spawned[num].gameObject.SetActive(value: false);
				}
				else if (Application.isEditor)
				{
					spawned[num].DestroyImmediate();
				}
				else
				{
					spawned[num].Destroy();
				}
			}
			num--;
		}
		ObjectControl[] array = new ObjectControl[_spawnCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = spawned[i];
		}
		spawned = array;
		Rebuild();
	}

	public void GetAll()
	{
		ObjectControl[] array = new ObjectControl[base.transform.childCount];
		int num = 0;
		foreach (Transform item in base.transform)
		{
			if (array[num] == null)
			{
				array[num++] = new ObjectControl(item.gameObject);
				continue;
			}
			bool flag = false;
			for (int i = 0; i < spawned.Length; i++)
			{
				if (spawned[i].gameObject == item.gameObject)
				{
					array[num++] = spawned[i];
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array[num++] = new ObjectControl(item.gameObject);
			}
		}
		spawned = array;
	}

	public void Spawn()
	{
		if (_objectMethod == ObjectMethod.Instantiate)
		{
			if (delayedSpawn && Application.isPlaying)
			{
				StopCoroutine("InstantiateAllWithDelay");
				StartCoroutine(InstantiateAllWithDelay());
			}
			else
			{
				InstantiateAll();
			}
		}
		else
		{
			GetAll();
		}
		Rebuild();
	}

	protected override void LateRun()
	{
		base.LateRun();
		if (_objectMethod == ObjectMethod.GetChildren && lastChildCount != base.transform.childCount)
		{
			Spawn();
			lastChildCount = base.transform.childCount;
		}
	}

	private IEnumerator InstantiateAllWithDelay()
	{
		if (!(base.spline == null) && objects.Length != 0)
		{
			for (int i = spawned.Length; i <= spawnCount; i++)
			{
				InstantiateSingle();
				yield return new WaitForSeconds(spawnDelay);
			}
		}
	}

	private void InstantiateAll()
	{
		if (!(base.spline == null) && objects.Length != 0)
		{
			for (int i = spawned.Length; i < spawnCount; i++)
			{
				InstantiateSingle();
			}
		}
	}

	private void InstantiateSingle()
	{
		if (objects.Length != 0)
		{
			int num = 0;
			num = ((_iteration != Iteration.Ordered) ? UnityEngine.Random.Range(0, objects.Length) : (spawned.Length - Mathf.FloorToInt(spawned.Length / objects.Length) * objects.Length));
			if (!(objects[num] == null))
			{
				ObjectControl[] array = new ObjectControl[spawned.Length + 1];
				spawned.CopyTo(array, 0);
				array[^1] = new ObjectControl(UnityEngine.Object.Instantiate(objects[num], base.transform.position, base.transform.rotation));
				array[^1].transform.parent = base.transform;
				spawned = array;
			}
		}
	}

	protected override void Build()
	{
		base.Build();
		offsetRandomizer = new System.Random(_randomSeed);
		if (_shellOffset)
		{
			shellRandomizer = new System.Random(_randomSeed + 1);
		}
		rotationRandomizer = new System.Random(_randomSeed + 2);
		scaleRandomizer = new System.Random(_randomSeed + 3);
		bool flag = _minScaleMultiplier != _maxScaleMultiplier;
		for (int i = 0; i < spawned.Length; i++)
		{
			if (spawned[i] == null)
			{
				Clear();
				Spawn();
				break;
			}
			float num = 0f;
			if (spawned.Length > 1)
			{
				num = ((!base.spline.isClosed) ? ((float)i / (float)(spawned.Length - 1)) : ((float)i / (float)spawned.Length));
			}
			num += _evaluateOffset;
			if (num > 1f)
			{
				num -= 1f;
			}
			else if (num < 0f)
			{
				num += 1f;
			}
			if (objectPositioning == Positioning.Clip)
			{
				base.spline.Evaluate(num, evalResult);
			}
			else
			{
				Evaluate(num, evalResult);
			}
			ModifySample(evalResult);
			spawned[i].position = evalResult.position;
			if (_applyScale)
			{
				Vector3 scale = spawned[i].baseScale * evalResult.size;
				Vector3 vector = _minScaleMultiplier;
				if (flag)
				{
					if (_uniformScaleLerp)
					{
						vector = Vector3.Lerp(new Vector3(_minScaleMultiplier.x, _minScaleMultiplier.y, _minScaleMultiplier.x), new Vector3(_maxScaleMultiplier.x, _maxScaleMultiplier.y, _maxScaleMultiplier.z), (float)scaleRandomizer.NextDouble());
					}
					else
					{
						vector.x = Mathf.Lerp(_minScaleMultiplier.x, _maxScaleMultiplier.x, (float)scaleRandomizer.NextDouble());
						vector.y = Mathf.Lerp(_minScaleMultiplier.y, _maxScaleMultiplier.y, (float)scaleRandomizer.NextDouble());
						vector.z = Mathf.Lerp(_minScaleMultiplier.z, _maxScaleMultiplier.z, (float)scaleRandomizer.NextDouble());
					}
				}
				scale.x *= vector.x;
				scale.y *= vector.y;
				scale.z *= vector.z;
				spawned[i].scale = scale;
			}
			else
			{
				spawned[i].scale = spawned[i].baseScale;
			}
			Vector3 normalized = Vector3.Cross(evalResult.forward, evalResult.up).normalized;
			Vector3 vector2 = _minOffset;
			if (_minOffset != _maxOffset)
			{
				if (_shellOffset)
				{
					float num2 = _maxOffset.x - _minOffset.x;
					float num3 = _maxOffset.y - _minOffset.y;
					float f = (float)shellRandomizer.NextDouble() * 360f * (MathF.PI / 180f);
					vector2 = new Vector2(0.5f * Mathf.Cos(f), 0.5f * Mathf.Sin(f));
					vector2.x *= num2;
					vector2.y *= num3;
				}
				else
				{
					float t = (float)offsetRandomizer.NextDouble();
					vector2.x = Mathf.Lerp(_minOffset.x, _maxOffset.x, t);
					t = (float)offsetRandomizer.NextDouble();
					vector2.y = Mathf.Lerp(_minOffset.y, _maxOffset.y, t);
					t = (float)offsetRandomizer.NextDouble();
					vector2.z = Mathf.Lerp(_minOffset.z, _maxOffset.z, t);
				}
			}
			if (_offsetUseWorldCoords)
			{
				spawned[i].position += vector2;
			}
			else
			{
				spawned[i].position += normalized * vector2.x * evalResult.size + evalResult.up * vector2.y * evalResult.size;
			}
			if (_applyRotation)
			{
				Quaternion quaternion = Quaternion.Euler(Mathf.Lerp(_minRotation.x, _maxRotation.x, (float)rotationRandomizer.NextDouble()), Mathf.Lerp(_minRotation.y, _maxRotation.y, (float)rotationRandomizer.NextDouble()), Mathf.Lerp(_minRotation.z, _maxRotation.z, (float)rotationRandomizer.NextDouble()));
				if (_rotateByOffset)
				{
					spawned[i].rotation = Quaternion.LookRotation(evalResult.forward, spawned[i].position - evalResult.position) * quaternion;
				}
				else
				{
					spawned[i].rotation = evalResult.rotation * quaternion;
				}
			}
			if (_objectPositioning == Positioning.Clip)
			{
				if ((double)num < base.clipFrom || (double)num > base.clipTo)
				{
					spawned[i].active = false;
				}
				else
				{
					spawned[i].active = true;
				}
			}
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		for (int i = 0; i < spawned.Length; i++)
		{
			spawned[i].Apply();
		}
	}
}
