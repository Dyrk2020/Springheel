using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
public class SplineUser : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum UpdateMethod
	{
		Update,
		FixedUpdate,
		LateUpdate
	}

	[HideInInspector]
	public UpdateMethod updateMethod;

	[SerializeField]
	[FormerlySerializedAs("_computer")]
	[HideInInspector]
	private SplineComputer _spline;

	[SerializeField]
	[HideInInspector]
	private bool _autoUpdate = true;

	[SerializeField]
	[HideInInspector]
	protected RotationModifier _rotationModifier = new RotationModifier();

	[SerializeField]
	[HideInInspector]
	protected OffsetModifier _offsetModifier = new OffsetModifier();

	[HideInInspector]
	[SerializeField]
	protected ColorModifier _colorModifier = new ColorModifier();

	[HideInInspector]
	[SerializeField]
	protected SizeModifier _sizeModifier = new SizeModifier();

	[HideInInspector]
	[SerializeField]
	private SampleCollection sampleCollection = new SampleCollection();

	[SerializeField]
	[HideInInspector]
	private SplineSample clipFromSample = new SplineSample();

	[HideInInspector]
	[SerializeField]
	private SplineSample clipToSample = new SplineSample();

	[HideInInspector]
	[SerializeField]
	private bool _loopSamples;

	[SerializeField]
	[HideInInspector]
	private double _clipFrom;

	[HideInInspector]
	[SerializeField]
	private double _clipTo = 1.0;

	[SerializeField]
	[HideInInspector]
	private float animClipFrom;

	[SerializeField]
	[HideInInspector]
	private float animClipTo = 1f;

	private bool rebuild;

	private bool getSamples;

	private bool postBuild;

	private Transform _trs;

	private bool _hasTransform;

	[SerializeField]
	[HideInInspector]
	private int _sampleCount;

	[HideInInspector]
	[SerializeField]
	private int startSampleIndex;

	protected SplineSample evalResult = new SplineSample();

	[HideInInspector]
	public volatile bool multithreaded;

	[HideInInspector]
	public bool buildOnAwake;

	[HideInInspector]
	public bool buildOnEnable;

	[SerializeField]
	[HideInInspector]
	private bool _isUpdated;

	public SplineComputer spline
	{
		get
		{
			return _spline;
		}
		set
		{
			if (value != _spline)
			{
				if (_spline != null)
				{
					_spline.Unsubscribe(this);
				}
				_spline = value;
				if (_spline != null)
				{
					_spline.Subscribe(this);
					Rebuild();
				}
				OnSplineChanged();
			}
		}
	}

	public double clipFrom
	{
		get
		{
			return _clipFrom;
		}
		set
		{
			if (value != _clipFrom)
			{
				animClipFrom = (float)_clipFrom;
				_clipFrom = DMath.Clamp01(value);
				if (_clipFrom > _clipTo && !_spline.isClosed)
				{
					_clipTo = _clipFrom;
				}
				getSamples = true;
				Rebuild();
			}
		}
	}

	public double clipTo
	{
		get
		{
			return _clipTo;
		}
		set
		{
			if (value != _clipTo)
			{
				animClipTo = (float)_clipTo;
				_clipTo = DMath.Clamp01(value);
				if (_clipTo < _clipFrom && !_spline.isClosed)
				{
					_clipFrom = _clipTo;
				}
				getSamples = true;
				Rebuild();
			}
		}
	}

	public bool autoUpdate
	{
		get
		{
			return _autoUpdate;
		}
		set
		{
			if (value != _autoUpdate)
			{
				_autoUpdate = value;
				if (value)
				{
					Rebuild();
				}
			}
		}
	}

	public bool loopSamples
	{
		get
		{
			return _loopSamples;
		}
		set
		{
			if (value != _loopSamples)
			{
				_loopSamples = value;
				if (!_loopSamples && _clipTo < _clipFrom)
				{
					double num = _clipTo;
					_clipTo = _clipFrom;
					_clipFrom = num;
				}
				Rebuild();
			}
		}
	}

	public double span
	{
		get
		{
			if (samplesAreLooped)
			{
				return 1.0 - _clipFrom + _clipTo;
			}
			return _clipTo - _clipFrom;
		}
	}

	public bool samplesAreLooped
	{
		get
		{
			if (_loopSamples)
			{
				return _clipFrom >= _clipTo;
			}
			return false;
		}
	}

	public RotationModifier rotationModifier => _rotationModifier;

	public OffsetModifier offsetModifier => _offsetModifier;

	public ColorModifier colorModifier => _colorModifier;

	public SizeModifier sizeModifier => _sizeModifier;

	protected Transform trs => _trs;

	protected bool hasTransform => _hasTransform;

	public int sampleCount => _sampleCount;

	public event EmptySplineHandler onPostBuild;

	protected virtual void Awake()
	{
		CacheTransform();
		if (buildOnAwake)
		{
			RebuildImmediate();
		}
	}

	protected void CacheTransform()
	{
		_trs = base.transform;
		_hasTransform = true;
	}

	protected virtual void Reset()
	{
	}

	protected virtual void OnEnable()
	{
		if (buildOnEnable)
		{
			RebuildImmediate();
		}
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnDidApplyAnimationProperties()
	{
		bool flag = false;
		if (_clipFrom != (double)animClipFrom || _clipTo != (double)animClipTo)
		{
			flag = true;
		}
		_clipFrom = animClipFrom;
		_clipTo = animClipTo;
		Rebuild();
		if (flag)
		{
			GetSamples();
		}
	}

	public SplineSample GetSampleRaw(int index)
	{
		if (index >= _sampleCount)
		{
			index = _sampleCount - 1;
		}
		if (samplesAreLooped)
		{
			sampleCollection.GetSamplingValues(clipFrom, out var sampleIndex, out var lerp);
			sampleCollection.GetSamplingValues(clipTo, out var sampleIndex2, out lerp);
			if (index == 0)
			{
				return clipFromSample;
			}
			int num = sampleIndex2;
			if (lerp > 0.0)
			{
				num++;
			}
			if (index == _sampleCount - 1)
			{
				return clipToSample;
			}
			int num2 = sampleIndex + index;
			if (num2 >= sampleCollection.Count)
			{
				num2 -= sampleCollection.Count;
			}
			return sampleCollection.samples[num2];
		}
		if (index == 0)
		{
			return clipFromSample;
		}
		if (index == _sampleCount - 1)
		{
			return clipToSample;
		}
		return sampleCollection.samples[startSampleIndex + index];
	}

	public void GetSample(int index, SplineSample target)
	{
		ModifySample(GetSampleRaw(index), target);
	}

	public virtual void Rebuild()
	{
		if (autoUpdate)
		{
			rebuild = (getSamples = true);
		}
	}

	public virtual void RebuildImmediate()
	{
		try
		{
			GetSamples();
			Build();
			PostBuild();
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
		rebuild = false;
		getSamples = false;
	}

	private void Update()
	{
		if (updateMethod == UpdateMethod.Update)
		{
			Run();
			RunUpdate();
			LateRun();
		}
	}

	private void LateUpdate()
	{
		if (updateMethod == UpdateMethod.LateUpdate)
		{
			Run();
			RunUpdate();
			LateRun();
		}
	}

	private void FixedUpdate()
	{
		if (updateMethod == UpdateMethod.FixedUpdate)
		{
			Run();
			RunUpdate();
			LateRun();
		}
	}

	private void RunUpdate()
	{
		if (rebuild)
		{
			if (multithreaded)
			{
				if (getSamples)
				{
					SplineThreading.Run(ResampleAndBuildThreaded);
				}
				else
				{
					SplineThreading.Run(BuildThreaded);
				}
			}
			else
			{
				if (getSamples || spline.sampleMode == SplineComputer.SampleMode.Optimized)
				{
					GetSamples();
				}
				Build();
				postBuild = true;
			}
			rebuild = false;
		}
		if (postBuild)
		{
			PostBuild();
			if (this.onPostBuild != null)
			{
				this.onPostBuild();
			}
			postBuild = false;
		}
	}

	private void BuildThreaded()
	{
		Build();
		postBuild = true;
	}

	private void ResampleAndBuildThreaded()
	{
		GetSamples();
		Build();
		postBuild = true;
	}

	protected virtual void Run()
	{
	}

	protected virtual void LateRun()
	{
	}

	protected virtual void Build()
	{
	}

	protected virtual void PostBuild()
	{
	}

	protected virtual void OnSplineChanged()
	{
	}

	public void ModifySample(SplineSample source, SplineSample destination)
	{
		destination.CopyFrom(source);
		ModifySample(destination);
	}

	public void ModifySample(SplineSample sample)
	{
		offsetModifier.Apply(sample);
		_rotationModifier.Apply(sample);
		_colorModifier.Apply(sample);
		_sizeModifier.Apply(sample);
	}

	public void SetClipRange(double from, double to)
	{
		if (!_spline.isClosed && to < from)
		{
			to = from;
		}
		_clipFrom = DMath.Clamp01(from);
		_clipTo = DMath.Clamp01(to);
		GetSamples();
		Rebuild();
	}

	private void GetSamples()
	{
		if (!(spline == null))
		{
			getSamples = false;
			spline.GetSamples(sampleCollection);
			sampleCollection.Evaluate(clipFrom, clipFromSample);
			sampleCollection.Evaluate(clipTo, clipToSample);
			_sampleCount = sampleCollection.GetClippedSampleCount(clipFrom, clipTo, out var _, out var _);
			sampleCollection.GetSamplingValues(_clipFrom, out startSampleIndex, out var _);
		}
	}

	public double ClipPercent(double percent)
	{
		ClipPercent(ref percent);
		return percent;
	}

	public void ClipPercent(ref double percent)
	{
		if (sampleCollection.Count == 0)
		{
			percent = 0.0;
		}
		else if (samplesAreLooped)
		{
			if (percent >= clipFrom && percent <= 1.0)
			{
				percent = DMath.InverseLerp(clipFrom, clipFrom + span, percent);
			}
			else if (percent <= clipTo)
			{
				percent = DMath.InverseLerp(clipTo - span, clipTo, percent);
			}
			else if (DMath.InverseLerp(clipTo, clipFrom, percent) < 0.5)
			{
				percent = 1.0;
			}
			else
			{
				percent = 0.0;
			}
		}
		else
		{
			percent = DMath.InverseLerp(clipFrom, clipTo, percent);
		}
	}

	public double UnclipPercent(double percent)
	{
		UnclipPercent(ref percent);
		return percent;
	}

	public void UnclipPercent(ref double percent)
	{
		if (percent == 0.0)
		{
			percent = clipFrom;
			return;
		}
		if (percent == 1.0)
		{
			percent = clipTo;
			return;
		}
		if (samplesAreLooped)
		{
			double num = (1.0 - clipFrom) / span;
			if (num == 0.0)
			{
				percent = 0.0;
				return;
			}
			if (percent < num)
			{
				percent = DMath.Lerp(clipFrom, 1.0, percent / num);
			}
			else
			{
				if (clipTo == 0.0)
				{
					percent = 0.0;
					return;
				}
				percent = DMath.Lerp(0.0, clipTo, (percent - num) / (clipTo / span));
			}
		}
		else
		{
			percent = DMath.Lerp(clipFrom, clipTo, percent);
		}
		percent = DMath.Clamp01(percent);
	}

	private int GetSampleIndex(double percent)
	{
		sampleCollection.GetSamplingValues(UnclipPercent(percent), out var sampleIndex, out var _);
		return sampleIndex;
	}

	public Vector3 EvaluatePosition(double percent)
	{
		return sampleCollection.EvaluatePosition(UnclipPercent(percent));
	}

	public void Evaluate(double percent, SplineSample result)
	{
		sampleCollection.Evaluate(UnclipPercent(percent), result);
		result.percent = DMath.Clamp01(percent);
	}

	public SplineSample Evaluate(double percent)
	{
		SplineSample splineSample = new SplineSample();
		Evaluate(UnclipPercent(percent), splineSample);
		splineSample.percent = DMath.Clamp01(percent);
		return splineSample;
	}

	public void Evaluate(ref SplineSample[] results, double from = 0.0, double to = 1.0)
	{
		sampleCollection.Evaluate(ref results, UnclipPercent(from), UnclipPercent(to));
		for (int i = 0; i < results.Length; i++)
		{
			ClipPercent(ref results[i].percent);
		}
	}

	public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
	{
		sampleCollection.EvaluatePositions(ref positions, UnclipPercent(from), UnclipPercent(to));
	}

	public double Travel(double start, float distance, Spline.Direction direction, out float moved)
	{
		moved = 0f;
		if (direction == Spline.Direction.Forward && start >= 1.0)
		{
			return 1.0;
		}
		if (direction == Spline.Direction.Backward && start <= 0.0)
		{
			return 0.0;
		}
		if (distance == 0f)
		{
			return DMath.Clamp01(start);
		}
		double percent = sampleCollection.Travel(UnclipPercent(start), distance, direction, out moved, clipFrom, clipTo);
		return ClipPercent(percent);
	}

	public double Travel(double start, float distance, Spline.Direction direction = Spline.Direction.Forward)
	{
		float moved;
		return Travel(start, distance, direction, out moved);
	}

	public double TravelWithOffset(double start, float distance, Spline.Direction direction, Vector3 offset, out float moved)
	{
		moved = 0f;
		if (direction == Spline.Direction.Forward && start >= 1.0)
		{
			return 1.0;
		}
		if (direction == Spline.Direction.Backward && start <= 0.0)
		{
			return 0.0;
		}
		if (distance == 0f)
		{
			return DMath.Clamp01(start);
		}
		double percent = sampleCollection.TravelWithOffset(UnclipPercent(start), distance, direction, offset, out moved, clipFrom, clipTo);
		return ClipPercent(percent);
	}

	public virtual void Project(Vector3 position, SplineSample result, double from = 0.0, double to = 1.0)
	{
		if (!(_spline == null))
		{
			sampleCollection.Project(position, _spline.pointCount, result, UnclipPercent(from), UnclipPercent(to));
			ClipPercent(ref result.percent);
		}
	}

	public float CalculateLength(double from = 0.0, double to = 1.0)
	{
		return sampleCollection.CalculateLength(UnclipPercent(from), UnclipPercent(to));
	}

	public float CalculateLengthWithOffset(Vector3 offset, double from = 0.0, double to = 1.0)
	{
		return sampleCollection.CalculateLengthWithOffset(offset, UnclipPercent(from), UnclipPercent(to));
	}

	public virtual void OnBeforeSerialize()
	{
		sampleCollection.clipFrom = _clipFrom;
		sampleCollection.clipTo = _clipTo;
		sampleCollection.loopSamples = _loopSamples;
	}

	public virtual void OnAfterDeserialize()
	{
		if (!_isUpdated)
		{
			_clipFrom = sampleCollection.clipFrom;
			_clipTo = sampleCollection.clipTo;
			_loopSamples = sampleCollection.loopSamples;
			_isUpdated = true;
			if ((bool)spline)
			{
				spline.Subscribe(this);
			}
		}
	}
}
