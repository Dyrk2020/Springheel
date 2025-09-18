using UnityEngine;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
[AddComponentMenu("Dreamteck/Splines/Users/Spline Projector")]
public class SplineProjector : SplineTracer
{
	public enum Mode
	{
		Accurate,
		Cached
	}

	[SerializeField]
	[HideInInspector]
	private Mode _mode = Mode.Cached;

	[SerializeField]
	[HideInInspector]
	private bool _autoProject = true;

	[Range(3f, 8f)]
	[SerializeField]
	[HideInInspector]
	private int _subdivide = 4;

	[SerializeField]
	[HideInInspector]
	private Transform _projectTarget;

	[HideInInspector]
	[SerializeField]
	private Transform applyTarget;

	[HideInInspector]
	[SerializeField]
	private GameObject _targetObject;

	[HideInInspector]
	[SerializeField]
	public Vector2 _offset;

	[SerializeField]
	[HideInInspector]
	public Vector3 _rotationOffset = Vector3.zero;

	[HideInInspector]
	[SerializeField]
	private Vector3 lastPosition = Vector3.zero;

	public Mode mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if (value != _mode)
			{
				_mode = value;
				Rebuild();
			}
		}
	}

	public bool autoProject
	{
		get
		{
			return _autoProject;
		}
		set
		{
			if (value != _autoProject)
			{
				_autoProject = value;
				if (_autoProject)
				{
					Rebuild();
				}
			}
		}
	}

	public int subdivide
	{
		get
		{
			return _subdivide;
		}
		set
		{
			if (value != _subdivide)
			{
				_subdivide = value;
				if (_mode == Mode.Accurate)
				{
					Rebuild();
				}
			}
		}
	}

	public Transform projectTarget
	{
		get
		{
			if (_projectTarget == null)
			{
				return base.transform;
			}
			return _projectTarget;
		}
		set
		{
			if (value != _projectTarget)
			{
				_projectTarget = value;
				Rebuild();
			}
		}
	}

	public GameObject targetObject
	{
		get
		{
			if (_targetObject == null && applyTarget != null)
			{
				_targetObject = applyTarget.gameObject;
				applyTarget = null;
				return _targetObject;
			}
			return _targetObject;
		}
		set
		{
			if (value != _targetObject)
			{
				_targetObject = value;
				RefreshTargets();
				Rebuild();
			}
		}
	}

	public event SplineReachHandler onEndReached;

	public event SplineReachHandler onBeginningReached;

	protected override void Reset()
	{
		base.Reset();
		_projectTarget = base.transform;
	}

	protected override Transform GetTransform()
	{
		if (targetObject == null)
		{
			return null;
		}
		return targetObject.transform;
	}

	protected override Rigidbody GetRigidbody()
	{
		if (targetObject == null)
		{
			return null;
		}
		return targetObject.GetComponent<Rigidbody>();
	}

	protected override Rigidbody2D GetRigidbody2D()
	{
		if (targetObject == null)
		{
			return null;
		}
		return targetObject.GetComponent<Rigidbody2D>();
	}

	protected override void LateRun()
	{
		base.LateRun();
		if (autoProject && (bool)projectTarget && lastPosition != projectTarget.position)
		{
			lastPosition = projectTarget.position;
			CalculateProjection();
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		CalculateProjection();
	}

	protected override void OnSplineChanged()
	{
		if (base.spline != null)
		{
			if (_mode == Mode.Accurate)
			{
				base.spline.Project(_result, _projectTarget.position, base.clipFrom, base.clipTo, SplineComputer.EvaluateMode.Calculate, subdivide);
			}
			else
			{
				base.spline.Project(_result, _projectTarget.position, base.clipFrom, base.clipTo);
			}
			_result.percent = ClipPercent(_result.percent);
		}
	}

	private void Project()
	{
		if (_mode == Mode.Accurate && base.spline != null)
		{
			base.spline.Project(_result, _projectTarget.position, base.clipFrom, base.clipTo, SplineComputer.EvaluateMode.Calculate, subdivide);
			_result.percent = ClipPercent(_result.percent);
		}
		else
		{
			Project(_projectTarget.position, _result);
		}
	}

	public void CalculateProjection()
	{
		if (_projectTarget == null)
		{
			return;
		}
		double num = _result.percent;
		Project();
		if (this.onBeginningReached != null && _result.percent <= base.clipFrom)
		{
			if (!Mathf.Approximately((float)num, (float)_result.percent))
			{
				this.onBeginningReached();
				if (base.samplesAreLooped)
				{
					CheckTriggers(num, 0.0);
					CheckNodes(num, 0.0);
					num = 1.0;
				}
			}
		}
		else if (this.onEndReached != null && _result.percent >= base.clipTo && !Mathf.Approximately((float)num, (float)_result.percent))
		{
			this.onEndReached();
			if (base.samplesAreLooped)
			{
				CheckTriggers(num, 1.0);
				CheckNodes(num, 1.0);
				num = 0.0;
			}
		}
		CheckTriggers(num, _result.percent);
		CheckNodes(num, _result.percent);
		if (targetObject != null)
		{
			ApplyMotion();
		}
		InvokeTriggers();
		InvokeNodes();
		lastPosition = projectTarget.position;
	}
}
