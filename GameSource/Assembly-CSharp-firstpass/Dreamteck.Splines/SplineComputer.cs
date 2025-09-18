using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
[AddComponentMenu("Dreamteck/Splines/Spline Computer")]
public class SplineComputer : MonoBehaviour
{
	public enum Space
	{
		World,
		Local
	}

	public enum EvaluateMode
	{
		Cached,
		Calculate
	}

	public enum SampleMode
	{
		Default,
		Uniform,
		Optimized
	}

	public enum UpdateMode
	{
		Update,
		FixedUpdate,
		LateUpdate,
		AllUpdate,
		None
	}

	[Serializable]
	internal class NodeLink
	{
		[SerializeField]
		internal Node node;

		[SerializeField]
		internal int pointIndex;

		internal List<Node.Connection> GetConnections(SplineComputer exclude)
		{
			Node.Connection[] connections = node.GetConnections();
			List<Node.Connection> list = new List<Node.Connection>();
			for (int i = 0; i < connections.Length; i++)
			{
				if (!(connections[i].spline == exclude))
				{
					list.Add(connections[i]);
				}
			}
			return list;
		}
	}

	[HideInInspector]
	public bool multithreaded;

	[HideInInspector]
	public bool rebuildOnAwake;

	[HideInInspector]
	public UpdateMode updateMode;

	[HideInInspector]
	public TriggerGroup[] triggerGroups = new TriggerGroup[0];

	[SerializeField]
	[HideInInspector]
	private Spline spline = new Spline(Spline.Type.CatmullRom);

	[SerializeField]
	[HideInInspector]
	private SplineSample[] _rawSamples = new SplineSample[0];

	[SerializeField]
	[HideInInspector]
	private SplineSample[] _transformedSamples = new SplineSample[0];

	[HideInInspector]
	[SerializeField]
	private SampleCollection sampleCollection = new SampleCollection();

	[HideInInspector]
	[SerializeField]
	private double[] originalSamplePercents = new double[0];

	private bool[] sampleFlter = new bool[0];

	[HideInInspector]
	[SerializeField]
	private int _sampleCount;

	[SerializeField]
	[HideInInspector]
	private bool _is2D;

	[SerializeField]
	[HideInInspector]
	private bool hasSamples;

	[SerializeField]
	[HideInInspector]
	private bool[] pointsDirty = new bool[0];

	[SerializeField]
	[Range(0.001f, 45f)]
	[HideInInspector]
	private float _optimizeAngleThreshold = 0.5f;

	[HideInInspector]
	[SerializeField]
	private Space _space = Space.Local;

	[SerializeField]
	[HideInInspector]
	private SampleMode _sampleMode;

	[SerializeField]
	[HideInInspector]
	private SplineUser[] _subscribers = new SplineUser[0];

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("_nodeLinks")]
	private NodeLink[] nodes = new NodeLink[0];

	private bool rebuildPending;

	private bool _trsCheck;

	private Transform _trs;

	private Matrix4x4 transformMatrix;

	private Matrix4x4 inverseTransformMatrix;

	private bool queueResample;

	private bool queueRebuild;

	private Vector3 lastPosition = Vector3.zero;

	private Vector3 lastScale = Vector3.zero;

	private bool uniformScale = true;

	private Quaternion lastRotation = Quaternion.identity;

	public Space space
	{
		get
		{
			return _space;
		}
		set
		{
			if (value == _space)
			{
				return;
			}
			SplinePoint[] points = GetPoints();
			_space = value;
			if (_space == Space.Local)
			{
				_transformedSamples = new SplineSample[_rawSamples.Length];
				for (int i = 0; i < _transformedSamples.Length; i++)
				{
					_transformedSamples[i] = new SplineSample();
				}
			}
			SetPoints(points);
			Rebuild(forceUpdateAll: true);
		}
	}

	public Spline.Type type
	{
		get
		{
			return spline.type;
		}
		set
		{
			if (value != spline.type)
			{
				spline.type = value;
				Rebuild(forceUpdateAll: true);
			}
		}
	}

	public bool linearAverageDirection
	{
		get
		{
			return spline.linearAverageDirection;
		}
		set
		{
			if (value != spline.linearAverageDirection)
			{
				spline.linearAverageDirection = value;
				Rebuild(forceUpdateAll: true);
			}
		}
	}

	public bool is2D
	{
		get
		{
			return _is2D;
		}
		set
		{
			if (value != _is2D)
			{
				_is2D = value;
				SetPoints(GetPoints());
			}
		}
	}

	public int sampleRate
	{
		get
		{
			return spline.sampleRate;
		}
		set
		{
			if (value != spline.sampleRate)
			{
				if (value < 2)
				{
					value = 2;
				}
				spline.sampleRate = value;
				Rebuild(forceUpdateAll: true);
			}
		}
	}

	public float optimizeAngleThreshold
	{
		get
		{
			return _optimizeAngleThreshold;
		}
		set
		{
			if (value != _optimizeAngleThreshold)
			{
				if (value < 0.001f)
				{
					value = 0.001f;
				}
				_optimizeAngleThreshold = value;
				if (_sampleMode == SampleMode.Optimized)
				{
					Rebuild(forceUpdateAll: true);
				}
			}
		}
	}

	public SampleMode sampleMode
	{
		get
		{
			return _sampleMode;
		}
		set
		{
			if (value != _sampleMode)
			{
				_sampleMode = value;
				Rebuild(forceUpdateAll: true);
			}
		}
	}

	public AnimationCurve customValueInterpolation
	{
		get
		{
			return spline.customValueInterpolation;
		}
		set
		{
			spline.customValueInterpolation = value;
			Rebuild();
		}
	}

	public AnimationCurve customNormalInterpolation
	{
		get
		{
			return spline.customNormalInterpolation;
		}
		set
		{
			spline.customNormalInterpolation = value;
			Rebuild();
		}
	}

	public int iterations => spline.iterations;

	public double moveStep => spline.moveStep;

	public bool isClosed => spline.isClosed;

	public int pointCount => spline.points.Length;

	public SplineSample[] samples => sampleCollection.samples;

	public int sampleCount => _sampleCount;

	public SplineSample[] rawSamples => _rawSamples;

	public Vector3 position => lastPosition;

	public Quaternion rotation => lastRotation;

	public Vector3 scale => lastScale;

	public int subscriberCount => _subscribers.Length;

	public Transform trs
	{
		get
		{
			if (!_trsCheck)
			{
				_trs = base.transform;
			}
			return _trs;
		}
	}

	private bool useMultithreading => multithreaded;

	public event EmptySplineHandler onRebuild;

	private void Awake()
	{
		if (rebuildOnAwake)
		{
			RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
		}
		ResampleTransform();
	}

	private void FixedUpdate()
	{
		if (updateMode == UpdateMode.FixedUpdate || updateMode == UpdateMode.AllUpdate)
		{
			RunUpdate();
		}
	}

	private void LateUpdate()
	{
		if (updateMode == UpdateMode.LateUpdate || updateMode == UpdateMode.AllUpdate)
		{
			RunUpdate();
		}
	}

	private void Update()
	{
		if (updateMode == UpdateMode.Update || updateMode == UpdateMode.AllUpdate)
		{
			RunUpdate();
		}
	}

	private void RunUpdate()
	{
		bool flag = TransformHasChanged();
		if (flag)
		{
			ResampleTransform();
			if (nodes.Length != 0)
			{
				UpdateConnectedNodes();
			}
		}
		if (useMultithreading && queueRebuild)
		{
			RebuildUsers();
		}
		if (queueResample)
		{
			if (useMultithreading)
			{
				if (!flag)
				{
					SplineThreading.Run(CalculateAndTransformSamples);
				}
				else
				{
					SplineThreading.Run(CalculateSamples);
				}
			}
			else
			{
				CalculateSamples();
				if (!flag)
				{
					TransformSamples();
				}
			}
		}
		if (flag)
		{
			SetPointsDirty();
			if (useMultithreading)
			{
				SplineThreading.Run(TransformSamplesThreaded);
			}
			else
			{
				TransformSamples(forceTransformAll: true);
			}
		}
		if (!useMultithreading && queueRebuild)
		{
			RebuildUsers();
		}
	}

	private void TransformSamplesThreaded()
	{
		TransformSamples(forceTransformAll: true);
	}

	private void CalculateAndTransformSamples()
	{
		CalculateSamples();
		TransformSamples();
	}

	private bool TransformHasChanged()
	{
		if (!(lastPosition != trs.position) && !(lastRotation != trs.rotation))
		{
			return lastScale != trs.lossyScale;
		}
		return true;
	}

	private void OnEnable()
	{
		if (rebuildPending)
		{
			rebuildPending = false;
			Rebuild();
		}
	}

	public void GetSamples(SampleCollection collection)
	{
		collection.samples = sampleCollection.samples;
		collection.optimizedIndices = sampleCollection.optimizedIndices;
		collection.sampleMode = _sampleMode;
	}

	public void ResampleTransform()
	{
		transformMatrix.SetTRS(trs.position, trs.rotation, trs.lossyScale);
		inverseTransformMatrix = transformMatrix.inverse;
		lastPosition = trs.position;
		lastRotation = trs.rotation;
		lastScale = trs.lossyScale;
		uniformScale = lastScale.x == lastScale.y && lastScale.y == lastScale.z;
	}

	public void Subscribe(SplineUser input)
	{
		if (!IsSubscribed(input))
		{
			ArrayUtility.Add(ref _subscribers, input);
		}
	}

	public void Unsubscribe(SplineUser input)
	{
		for (int i = 0; i < _subscribers.Length; i++)
		{
			if (_subscribers[i] == input)
			{
				ArrayUtility.RemoveAt(ref _subscribers, i);
				break;
			}
		}
	}

	public bool IsSubscribed(SplineUser user)
	{
		for (int i = 0; i < _subscribers.Length; i++)
		{
			if (_subscribers[i] == user)
			{
				return true;
			}
		}
		return false;
	}

	public SplineUser[] GetSubscribers()
	{
		SplineUser[] array = new SplineUser[_subscribers.Length];
		_subscribers.CopyTo(array, 0);
		return array;
	}

	public SplinePoint[] GetPoints(Space getSpace = Space.World)
	{
		SplinePoint[] array = new SplinePoint[spline.points.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = spline.points[i];
			if (_space == Space.Local && getSpace == Space.World)
			{
				array[i].position = TransformPoint(array[i].position);
				array[i].tangent = TransformPoint(array[i].tangent);
				array[i].tangent2 = TransformPoint(array[i].tangent2);
				array[i].normal = TransformDirection(array[i].normal);
			}
		}
		return array;
	}

	public SplinePoint GetPoint(int index, Space getSpace = Space.World)
	{
		if (index < 0 || index >= spline.points.Length)
		{
			return default(SplinePoint);
		}
		if (_space == Space.Local && getSpace == Space.World)
		{
			SplinePoint result = spline.points[index];
			result.position = TransformPoint(result.position);
			result.tangent = TransformPoint(result.tangent);
			result.tangent2 = TransformPoint(result.tangent2);
			result.normal = TransformDirection(result.normal);
			return result;
		}
		return spline.points[index];
	}

	public Vector3 GetPointPosition(int index, Space getSpace = Space.World)
	{
		if (_space == Space.Local && getSpace == Space.World)
		{
			return TransformPoint(spline.points[index].position);
		}
		return spline.points[index].position;
	}

	public Vector3 GetPointNormal(int index, Space getSpace = Space.World)
	{
		if (_space == Space.Local && getSpace == Space.World)
		{
			return TransformDirection(spline.points[index].normal).normalized;
		}
		return spline.points[index].normal;
	}

	public Vector3 GetPointTangent(int index, Space getSpace = Space.World)
	{
		if (_space == Space.Local && getSpace == Space.World)
		{
			return TransformPoint(spline.points[index].tangent);
		}
		return spline.points[index].tangent;
	}

	public Vector3 GetPointTangent2(int index, Space getSpace = Space.World)
	{
		if (_space == Space.Local && getSpace == Space.World)
		{
			return TransformPoint(spline.points[index].tangent2);
		}
		return spline.points[index].tangent2;
	}

	public float GetPointSize(int index, Space getSpace = Space.World)
	{
		return spline.points[index].size;
	}

	public Color GetPointColor(int index, Space getSpace = Space.World)
	{
		return spline.points[index].color;
	}

	private void Make2D(ref SplinePoint point)
	{
		point.normal = Vector3.back;
		point.position.z = 0f;
		point.tangent.z = 0f;
		point.tangent2.z = 0f;
	}

	public void SetPoints(SplinePoint[] points, Space setSpace = Space.World)
	{
		bool flag = false;
		if (points.Length != spline.points.Length)
		{
			flag = true;
			if (points.Length < 4)
			{
				Break();
			}
			spline.points = new SplinePoint[points.Length];
			SetPointsDirty();
		}
		for (int i = 0; i < points.Length; i++)
		{
			SplinePoint point = points[i];
			if (_space == Space.Local && setSpace == Space.World)
			{
				point.position = InverseTransformPoint(points[i].position);
				point.tangent = InverseTransformPoint(points[i].tangent);
				point.tangent2 = InverseTransformPoint(points[i].tangent2);
				point.normal = InverseTransformDirection(points[i].normal);
			}
			if (_is2D)
			{
				Make2D(ref point);
			}
			if (SplinePoint.AreDifferent(ref point, ref spline.points[i]))
			{
				SetDirty(i);
				flag = true;
			}
			spline.points[i] = point;
		}
		if (isClosed)
		{
			spline.points[spline.points.Length - 1] = spline.points[0];
		}
		if (flag)
		{
			Rebuild();
			UpdateConnectedNodes(points);
		}
	}

	public void SetPointPosition(int index, Vector3 pos, Space setSpace = Space.World)
	{
		if (index >= 0)
		{
			if (index >= spline.points.Length)
			{
				AppendPoints(index + 1 - spline.points.Length);
			}
			Vector3 vector = pos;
			if (_space == Space.Local && setSpace == Space.World)
			{
				vector = InverseTransformPoint(pos);
			}
			if (vector != spline.points[index].position)
			{
				SetDirty(index);
				spline.points[index].position = vector;
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}
	}

	public void SetPointTangents(int index, Vector3 tan1, Vector3 tan2, Space setSpace = Space.World)
	{
		if (index >= 0)
		{
			if (index >= spline.points.Length)
			{
				AppendPoints(index + 1 - spline.points.Length);
			}
			Vector3 vector = tan1;
			Vector3 vector2 = tan2;
			if (_space == Space.Local && setSpace == Space.World)
			{
				vector = InverseTransformPoint(tan1);
				vector2 = InverseTransformPoint(tan2);
			}
			bool flag = false;
			if (vector2 != spline.points[index].tangent2)
			{
				flag = true;
				spline.points[index].SetTangent2Position(vector2);
			}
			if (vector != spline.points[index].tangent)
			{
				flag = true;
				spline.points[index].SetTangentPosition(vector);
			}
			if (_is2D)
			{
				Make2D(ref spline.points[index]);
			}
			if (flag)
			{
				SetDirty(index);
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}
	}

	public void SetPointNormal(int index, Vector3 nrm, Space setSpace = Space.World)
	{
		if (index < 0)
		{
			return;
		}
		if (index >= spline.points.Length)
		{
			AppendPoints(index + 1 - spline.points.Length);
		}
		Vector3 vector = nrm;
		if (_space == Space.Local && setSpace == Space.World)
		{
			vector = InverseTransformDirection(nrm);
		}
		if (vector != spline.points[index].normal)
		{
			SetDirty(index);
			spline.points[index].normal = vector;
			if (_is2D)
			{
				Make2D(ref spline.points[index]);
			}
			Rebuild();
			SetNodeForPoint(index, GetPoint(index));
		}
	}

	public void SetPointSize(int index, float size)
	{
		if (index >= 0)
		{
			if (index >= spline.points.Length)
			{
				AppendPoints(index + 1 - spline.points.Length);
			}
			if (size != spline.points[index].size)
			{
				SetDirty(index);
				spline.points[index].size = size;
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}
	}

	public void SetPointColor(int index, Color color)
	{
		if (index >= 0)
		{
			if (index >= spline.points.Length)
			{
				AppendPoints(index + 1 - spline.points.Length);
			}
			if (color != spline.points[index].color)
			{
				SetDirty(index);
				spline.points[index].color = color;
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}
	}

	public void SetPoint(int index, SplinePoint point, Space setSpace = Space.World)
	{
		if (index >= 0)
		{
			if (index >= spline.points.Length)
			{
				AppendPoints(index + 1 - spline.points.Length);
			}
			bool flag = false;
			SplinePoint point2 = point;
			if (_space == Space.Local && setSpace == Space.World)
			{
				point2.position = InverseTransformPoint(point.position);
				point2.tangent = InverseTransformPoint(point.tangent);
				point2.tangent2 = InverseTransformPoint(point.tangent2);
				point2.normal = InverseTransformDirection(point.normal);
			}
			if (_is2D)
			{
				Make2D(ref point2);
			}
			if (SplinePoint.AreDifferent(ref point2, ref spline.points[index]))
			{
				flag = true;
			}
			if (flag)
			{
				SetDirty(index);
				spline.points[index] = point2;
				Rebuild();
				SetNodeForPoint(index, point);
			}
		}
	}

	private void AppendPoints(int count)
	{
		SplinePoint[] array = new SplinePoint[spline.points.Length + count];
		spline.points.CopyTo(array, 0);
		spline.points = array;
		Rebuild(forceUpdateAll: true);
	}

	public double GetPointPercent(int pointIndex)
	{
		double num = DMath.Clamp01((double)pointIndex / (double)(pointCount - 1));
		if (_sampleMode != SampleMode.Uniform)
		{
			return num;
		}
		if (originalSamplePercents.Length <= 1)
		{
			return 0.0;
		}
		for (int num2 = originalSamplePercents.Length - 2; num2 >= 0; num2--)
		{
			if (originalSamplePercents[num2] < num)
			{
				double t = DMath.InverseLerp(originalSamplePercents[num2], originalSamplePercents[num2 + 1], num);
				return DMath.Lerp(sampleCollection.samples[num2].percent, sampleCollection.samples[num2 + 1].percent, t);
			}
		}
		return 0.0;
	}

	public int PercentToPointIndex(double percent, Spline.Direction direction = Spline.Direction.Forward)
	{
		if (_sampleMode == SampleMode.Uniform)
		{
			GetSamplingValues(percent, out var index, out var lerp);
			if (lerp > 0.0)
			{
				lerp = DMath.Lerp(originalSamplePercents[index], originalSamplePercents[index + 1], lerp);
				if (direction == Spline.Direction.Forward)
				{
					return DMath.FloorInt(lerp * (double)(pointCount - 1));
				}
				return DMath.CeilInt(lerp * (double)(pointCount - 1));
			}
			if (direction == Spline.Direction.Forward)
			{
				return DMath.FloorInt(originalSamplePercents[index] * (double)(pointCount - 1));
			}
			return DMath.CeilInt(originalSamplePercents[index] * (double)(pointCount - 1));
		}
		if (direction == Spline.Direction.Forward)
		{
			return DMath.FloorInt(percent * (double)(pointCount - 1));
		}
		return DMath.CeilInt(percent * (double)(pointCount - 1));
	}

	public Vector3 EvaluatePosition(double percent)
	{
		return EvaluatePosition(percent, EvaluateMode.Cached);
	}

	public Vector3 EvaluatePosition(double percent, EvaluateMode mode = EvaluateMode.Cached)
	{
		if (mode == EvaluateMode.Calculate)
		{
			return TransformPoint(spline.EvaluatePosition(percent));
		}
		return sampleCollection.EvaluatePosition(percent);
	}

	public Vector3 EvaluatePosition(int pointIndex, EvaluateMode mode = EvaluateMode.Cached)
	{
		return EvaluatePosition(GetPointPercent(pointIndex), mode);
	}

	public SplineSample Evaluate(double percent)
	{
		return Evaluate(percent, EvaluateMode.Cached);
	}

	public SplineSample Evaluate(double percent, EvaluateMode mode = EvaluateMode.Cached)
	{
		SplineSample result = new SplineSample();
		Evaluate(percent, result, mode);
		return result;
	}

	public SplineSample Evaluate(int pointIndex)
	{
		SplineSample result = new SplineSample();
		Evaluate(pointIndex, result);
		return result;
	}

	public void Evaluate(int pointIndex, SplineSample result)
	{
		Evaluate(GetPointPercent(pointIndex), result);
	}

	public void Evaluate(double percent, SplineSample result)
	{
		Evaluate(percent, result, EvaluateMode.Cached);
	}

	public void Evaluate(double percent, SplineSample result, EvaluateMode mode = EvaluateMode.Cached)
	{
		if (mode == EvaluateMode.Calculate)
		{
			spline.Evaluate(result, percent);
			TransformResult(result);
		}
		else
		{
			sampleCollection.Evaluate(percent, result);
		}
	}

	public void Evaluate(ref SplineSample[] results, double from = 0.0, double to = 1.0)
	{
		sampleCollection.Evaluate(ref results, from, to);
	}

	public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
	{
		sampleCollection.EvaluatePositions(ref positions, from, to);
	}

	public double Travel(double start, float distance, out float moved, Spline.Direction direction = Spline.Direction.Forward)
	{
		return sampleCollection.Travel(start, distance, direction, out moved);
	}

	public double Travel(double start, float distance, Spline.Direction direction = Spline.Direction.Forward)
	{
		float moved;
		return Travel(start, distance, out moved, direction);
	}

	public void Project(SplineSample result, Vector3 position, double from = 0.0, double to = 1.0, EvaluateMode mode = EvaluateMode.Cached, int subdivisions = 4)
	{
		if (mode == EvaluateMode.Calculate)
		{
			position = InverseTransformPoint(position);
			double percent = spline.Project(position, subdivisions, from, to);
			spline.Evaluate(result, percent);
			TransformResult(result);
		}
		else
		{
			sampleCollection.Project(position, pointCount, result, from, to);
		}
	}

	public SplineSample Project(Vector3 point, double from = 0.0, double to = 1.0)
	{
		SplineSample result = new SplineSample();
		Project(result, point, from, to);
		return result;
	}

	public float CalculateLength(double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			return 0f;
		}
		return sampleCollection.CalculateLength(from, to);
	}

	private void TransformResult(SplineSample result)
	{
		result.position = TransformPoint(result.position);
		result.forward = TransformDirection(result.forward);
		result.up = TransformDirection(result.up);
		if (!uniformScale)
		{
			result.forward.Normalize();
			result.up.Normalize();
		}
	}

	public void Rebuild(bool forceUpdateAll = false)
	{
		if (forceUpdateAll)
		{
			SetPointsDirty();
		}
		queueResample = true;
		if (updateMode == UpdateMode.None)
		{
			queueResample = false;
		}
	}

	public void RebuildImmediate()
	{
		RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
	}

	public void RebuildImmediate(bool calculateSamples = true, bool forceUpdateAll = false)
	{
		if (calculateSamples)
		{
			queueResample = true;
			if (forceUpdateAll)
			{
				SetPointsDirty();
			}
		}
		else
		{
			queueResample = false;
		}
		RunUpdate();
	}

	private void RebuildUsers()
	{
		for (int num = _subscribers.Length - 1; num >= 0; num--)
		{
			if (_subscribers[num] != null)
			{
				if (_subscribers[num].spline != this)
				{
					ArrayUtility.RemoveAt(ref _subscribers, num);
				}
				else if (_subscribers[num].isActiveAndEnabled)
				{
					_subscribers[num].Rebuild();
				}
			}
			else
			{
				ArrayUtility.RemoveAt(ref _subscribers, num);
			}
		}
		if (this.onRebuild != null)
		{
			this.onRebuild();
		}
		queueRebuild = false;
	}

	private void UnsetPointsDirty()
	{
		if (pointsDirty.Length != spline.points.Length)
		{
			pointsDirty = new bool[spline.points.Length];
		}
		for (int i = 0; i < pointsDirty.Length; i++)
		{
			pointsDirty[i] = false;
		}
	}

	private void SetPointsDirty()
	{
		if (pointsDirty.Length != spline.points.Length)
		{
			pointsDirty = new bool[spline.points.Length];
		}
		for (int i = 0; i < pointsDirty.Length; i++)
		{
			pointsDirty[i] = true;
		}
	}

	private void SetDirty(int index)
	{
		if (sampleMode == SampleMode.Uniform)
		{
			SetPointsDirty();
			return;
		}
		if (pointsDirty.Length != spline.points.Length)
		{
			pointsDirty = new bool[spline.points.Length];
		}
		pointsDirty[index] = true;
		if (index == 0 && isClosed)
		{
			pointsDirty[pointsDirty.Length - 1] = true;
		}
	}

	private void CalculateSamples()
	{
		queueResample = false;
		if (pointCount == 0)
		{
			if (_rawSamples.Length != 0)
			{
				_rawSamples = new SplineSample[0];
				sampleCollection.samples = new SplineSample[0];
			}
			return;
		}
		if (pointCount == 1)
		{
			if (_rawSamples.Length != 1)
			{
				_rawSamples = new SplineSample[1];
				_rawSamples[0] = new SplineSample();
				sampleCollection.samples = new SplineSample[1];
				sampleCollection.samples[0] = new SplineSample();
			}
			Evaluate(0.0, _rawSamples[0]);
			return;
		}
		if (_sampleMode == SampleMode.Uniform)
		{
			spline.EvaluateUniform(ref _rawSamples, ref originalSamplePercents);
		}
		else
		{
			if (originalSamplePercents.Length != 0)
			{
				originalSamplePercents = new double[0];
			}
			if (_rawSamples.Length != spline.iterations)
			{
				_rawSamples = new SplineSample[spline.iterations];
				for (int i = 0; i < _rawSamples.Length; i++)
				{
					_rawSamples[i] = new SplineSample();
				}
			}
			bool flag = true;
			if (type == Spline.Type.Bezier || type == Spline.Type.Linear)
			{
				flag = false;
			}
			for (int j = 0; j < _rawSamples.Length; j++)
			{
				double num = (double)j / (double)(_rawSamples.Length - 1);
				if (flag ? IsDirtyHermite(num) : IsDirtyBezier(num))
				{
					spline.Evaluate(_rawSamples[j], num);
				}
			}
		}
		if (isClosed)
		{
			_rawSamples[_rawSamples.Length - 1].CopyFrom(_rawSamples[0]);
			_rawSamples[_rawSamples.Length - 1].percent = 1.0;
		}
	}

	private void TransformSamples(bool forceTransformAll = false)
	{
		if (_transformedSamples.Length != _rawSamples.Length)
		{
			_transformedSamples = new SplineSample[_rawSamples.Length];
			for (int i = 0; i < _transformedSamples.Length; i++)
			{
				_transformedSamples[i] = new SplineSample(_rawSamples[i]);
			}
		}
		bool flag = true;
		if (type == Spline.Type.Bezier || type == Spline.Type.Linear)
		{
			flag = false;
		}
		if (space == Space.Local)
		{
			for (int j = 0; j < _rawSamples.Length; j++)
			{
				if (!((!forceTransformAll && flag) ? (!IsDirtyHermite(_rawSamples[j].percent)) : (!IsDirtyBezier(_rawSamples[j].percent))))
				{
					_transformedSamples[j].CopyFrom(_rawSamples[j]);
					TransformResult(_transformedSamples[j]);
				}
			}
		}
		else
		{
			_transformedSamples = _rawSamples;
		}
		if (_sampleMode == SampleMode.Optimized)
		{
			OptimizeSamples();
		}
		else
		{
			sampleCollection.samples = _transformedSamples;
			if (sampleFlter.Length != 0)
			{
				sampleFlter = new bool[0];
			}
			_sampleCount = sampleCollection.Count;
		}
		if (_sampleMode == SampleMode.Optimized)
		{
			if (sampleCollection.optimizedIndices.Length != _rawSamples.Length)
			{
				sampleCollection.optimizedIndices = new int[_rawSamples.Length];
			}
			sampleCollection.optimizedIndices[0] = 0;
			sampleCollection.optimizedIndices[sampleCollection.optimizedIndices.Length - 1] = sampleCollection.Count - 1;
			for (int k = 1; k < _rawSamples.Length - 1; k++)
			{
				sampleCollection.optimizedIndices[k] = 0;
				double num = (double)k / (double)(_rawSamples.Length - 1);
				for (int l = 0; l < sampleCollection.Count && !(sampleCollection.samples[l].percent > num); l++)
				{
					sampleCollection.optimizedIndices[k] = l;
				}
			}
			if (sampleCollection.optimizedIndices.Length > 1)
			{
				sampleCollection.optimizedIndices[sampleCollection.optimizedIndices.Length - 1] = sampleCollection.Count - 1;
			}
		}
		else if (sampleCollection.Count > 0)
		{
			sampleCollection.optimizedIndices = new int[0];
		}
		sampleCollection.sampleMode = _sampleMode;
		queueRebuild = true;
		hasSamples = _sampleCount > 0;
		UnsetPointsDirty();
	}

	private void OptimizeSamples()
	{
		if (_transformedSamples.Length <= 1)
		{
			return;
		}
		if (sampleFlter.Length != _rawSamples.Length)
		{
			sampleFlter = new bool[_rawSamples.Length];
		}
		_sampleCount = 2;
		Vector3 forward = _transformedSamples[0].forward;
		sampleFlter[0] = true;
		sampleFlter[sampleFlter.Length - 1] = true;
		for (int i = 1; i < _transformedSamples.Length - 1; i++)
		{
			if (Vector3.Angle(forward, _transformedSamples[i].forward) >= _optimizeAngleThreshold)
			{
				sampleFlter[i] = true;
				_sampleCount++;
				forward = _transformedSamples[i].forward;
			}
			else
			{
				sampleFlter[i] = false;
			}
		}
		if (sampleCollection.Count != _sampleCount || sampleCollection.samples == _transformedSamples)
		{
			sampleCollection.samples = new SplineSample[_sampleCount];
			for (int j = 0; j < sampleCollection.Count; j++)
			{
				sampleCollection.samples[j] = new SplineSample();
			}
		}
		int num = 0;
		for (int k = 0; k < _transformedSamples.Length; k++)
		{
			if (sampleFlter[k])
			{
				sampleCollection.samples[num].CopyFrom(_transformedSamples[k]);
				num++;
			}
		}
	}

	private bool IsDirtyBezier(double samplePercent)
	{
		float num = (float)samplePercent * (float)(pointCount - 1);
		int num2 = Mathf.FloorToInt(num);
		if (pointsDirty[num2])
		{
			return true;
		}
		int num3 = num2 + 1;
		if (num3 > pointCount - 1)
		{
			num3 = ((!isClosed) ? (pointCount - 1) : 0);
		}
		if (pointsDirty[num3])
		{
			return true;
		}
		int num4 = num2 - 1;
		if (num4 < 0)
		{
			num4 = (isClosed ? (pointCount - 1) : 0);
		}
		if (pointsDirty[num4] && Mathf.Approximately(num, num2))
		{
			return true;
		}
		return false;
	}

	private bool IsDirtyHermite(double samplePercent)
	{
		float num = (float)samplePercent * (float)(pointCount - 1);
		int num2 = Mathf.FloorToInt(num);
		if (pointsDirty[num2])
		{
			return true;
		}
		int num3 = num2 + 1;
		if (num3 > pointCount - 1)
		{
			num3 = ((!isClosed) ? (pointCount - 1) : 0);
		}
		int num4 = num3 + 1;
		if (num4 > pointCount - 1)
		{
			num4 = (isClosed ? 1 : (pointCount - 1));
		}
		if (pointsDirty[num3] || pointsDirty[num4])
		{
			return true;
		}
		int num5 = num2 - 1;
		if (num5 < 0)
		{
			num5 = (isClosed ? (pointCount - 2) : 0);
		}
		int num6 = num5 - 1;
		if (num6 < 0)
		{
			num6 = (isClosed ? (pointCount - 2) : 0);
		}
		if (pointsDirty[num5])
		{
			return true;
		}
		if (pointsDirty[num6] && Mathf.Approximately(num, num2))
		{
			return true;
		}
		return false;
	}

	public void Break()
	{
		Break(0);
	}

	public void Break(int at)
	{
		if (spline.isClosed)
		{
			spline.Break(at);
			if (at != 0)
			{
				SetPointsDirty();
			}
			else
			{
				SetDirty(0);
				SetDirty(pointCount - 1);
			}
			Rebuild();
		}
	}

	public void Close()
	{
		if (!spline.isClosed)
		{
			spline.Close();
			SetDirty(0);
			SetDirty(pointCount - 1);
			Rebuild();
		}
	}

	public void CatToBezierTangents()
	{
		spline.CatToBezierTangents();
		SetPoints(spline.points, Space.Local);
	}

	public bool Raycast(out RaycastHit hit, out double hitPercent, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0, QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
	{
		resolution = DMath.Clamp01(resolution);
		Spline.FormatFromTo(ref from, ref to, preventInvert: false);
		double num = from;
		Vector3 vector = EvaluatePosition(num);
		hitPercent = 0.0;
		do
		{
			double a = num;
			num = DMath.Move(num, to, moveStep / resolution);
			Vector3 vector2 = EvaluatePosition(num);
			if (Physics.Linecast(vector, vector2, out hit, layerMask, hitTriggers))
			{
				double t = (hit.point - vector).sqrMagnitude / (vector2 - vector).sqrMagnitude;
				hitPercent = DMath.Lerp(a, num, t);
				return true;
			}
			vector = vector2;
		}
		while (num != to);
		return false;
	}

	public bool RaycastAll(out RaycastHit[] hits, out double[] hitPercents, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0, QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
	{
		resolution = DMath.Clamp01(resolution);
		Spline.FormatFromTo(ref from, ref to, preventInvert: false);
		double num = from;
		Vector3 vector = EvaluatePosition(num);
		List<RaycastHit> list = new List<RaycastHit>();
		List<double> list2 = new List<double>();
		bool result = false;
		do
		{
			double a = num;
			num = DMath.Move(num, to, moveStep / resolution);
			Vector3 vector2 = EvaluatePosition(num);
			RaycastHit[] array = Physics.RaycastAll(vector, vector2 - vector, Vector3.Distance(vector, vector2), layerMask, hitTriggers);
			for (int i = 0; i < array.Length; i++)
			{
				result = true;
				double t = (array[i].point - vector).sqrMagnitude / (vector2 - vector).sqrMagnitude;
				list2.Add(DMath.Lerp(a, num, t));
				list.Add(array[i]);
			}
			vector = vector2;
		}
		while (num != to);
		hits = list.ToArray();
		hitPercents = list2.ToArray();
		return result;
	}

	public void CheckTriggers(double start, double end, SplineUser user = null)
	{
		for (int i = 0; i < triggerGroups.Length; i++)
		{
			triggerGroups[i].Check(start, end);
		}
	}

	public void CheckTriggers(int group, double start, double end)
	{
		if (group < 0 || group >= triggerGroups.Length)
		{
			Debug.LogError("Trigger group " + group + " does not exist");
		}
		else
		{
			triggerGroups[group].Check(start, end);
		}
	}

	public void ResetTriggers()
	{
		for (int i = 0; i < triggerGroups.Length; i++)
		{
			triggerGroups[i].Reset();
		}
	}

	public void ResetTriggers(int group)
	{
		if (group < 0 || group >= triggerGroups.Length)
		{
			Debug.LogError("Trigger group " + group + " does not exist");
			return;
		}
		for (int i = 0; i < triggerGroups[group].triggers.Length; i++)
		{
			triggerGroups[group].triggers[i].Reset();
		}
	}

	public List<Node.Connection> GetJunctions(int pointIndex)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].pointIndex == pointIndex)
			{
				return nodes[i].GetConnections(this);
			}
		}
		return new List<Node.Connection>();
	}

	public Dictionary<int, List<Node.Connection>> GetJunctions(double start = 0.0, double end = 1.0)
	{
		sampleCollection.GetSamplingValues(start, out var _, out var _);
		Dictionary<int, List<Node.Connection>> dictionary = new Dictionary<int, List<Node.Connection>>();
		float num = (float)(pointCount - 1) * (float)start;
		float num2 = (float)(pointCount - 1) * (float)end;
		for (int i = 0; i < nodes.Length; i++)
		{
			bool flag = false;
			if (end > start && (float)nodes[i].pointIndex > num && (float)nodes[i].pointIndex < num2)
			{
				flag = true;
			}
			else if ((float)nodes[i].pointIndex < num && (float)nodes[i].pointIndex > num2)
			{
				flag = true;
			}
			if (!flag && Mathf.Abs(num - (float)nodes[i].pointIndex) <= 0.0001f)
			{
				flag = true;
			}
			if (!flag && Mathf.Abs(num2 - (float)nodes[i].pointIndex) <= 0.0001f)
			{
				flag = true;
			}
			if (flag)
			{
				dictionary.Add(nodes[i].pointIndex, nodes[i].GetConnections(this));
			}
		}
		return dictionary;
	}

	public void ConnectNode(Node node, int pointIndex)
	{
		if (node == null)
		{
			Debug.LogError("Missing Node");
			return;
		}
		if (pointIndex < 0 || pointIndex >= spline.points.Length)
		{
			Debug.Log("Invalid point index " + pointIndex);
			return;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].node == null || (nodes[i].pointIndex != pointIndex && !(nodes[i].node == node)))
			{
				continue;
			}
			Node.Connection[] connections = nodes[i].node.GetConnections();
			for (int j = 0; j < connections.Length; j++)
			{
				if (connections[j].spline == this)
				{
					Debug.LogError("Node " + node.name + " is already connected to spline " + base.name + " at point " + nodes[i].pointIndex);
					return;
				}
			}
			AddNodeLink(node, pointIndex);
			return;
		}
		node.AddConnection(this, pointIndex);
		AddNodeLink(node, pointIndex);
	}

	public void DisconnectNode(int pointIndex)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].pointIndex == pointIndex)
			{
				nodes[i].node.RemoveConnection(this, pointIndex);
				ArrayUtility.RemoveAt(ref nodes, i);
				break;
			}
		}
	}

	private void AddNodeLink(Node node, int pointIndex)
	{
		NodeLink nodeLink = new NodeLink();
		nodeLink.node = node;
		nodeLink.pointIndex = pointIndex;
		ArrayUtility.Add(ref nodes, nodeLink);
		UpdateConnectedNodes();
	}

	public Dictionary<int, Node> GetNodes(double start = 0.0, double end = 1.0)
	{
		sampleCollection.GetSamplingValues(start, out var _, out var _);
		Dictionary<int, Node> dictionary = new Dictionary<int, Node>();
		float num = (float)(pointCount - 1) * (float)start;
		float num2 = (float)(pointCount - 1) * (float)end;
		for (int i = 0; i < nodes.Length; i++)
		{
			bool flag = false;
			if (end > start && (float)nodes[i].pointIndex > num && (float)nodes[i].pointIndex < num2)
			{
				flag = true;
			}
			else if ((float)nodes[i].pointIndex < num && (float)nodes[i].pointIndex > num2)
			{
				flag = true;
			}
			if (!flag && Mathf.Abs(num - (float)nodes[i].pointIndex) <= 0.0001f)
			{
				flag = true;
			}
			if (!flag && Mathf.Abs(num2 - (float)nodes[i].pointIndex) <= 0.0001f)
			{
				flag = true;
			}
			if (flag)
			{
				dictionary.Add(nodes[i].pointIndex, nodes[i].node);
			}
		}
		return dictionary;
	}

	public Node GetNode(int pointIndex)
	{
		if (pointIndex < 0 || pointIndex >= pointCount)
		{
			return null;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].pointIndex == pointIndex)
			{
				return nodes[i].node;
			}
		}
		return null;
	}

	public void TransferNode(int pointIndex, int newPointIndex)
	{
		if (newPointIndex < 0 || newPointIndex >= pointCount)
		{
			Debug.LogError("Invalid new point index " + newPointIndex);
			return;
		}
		if (GetNode(newPointIndex) != null)
		{
			Debug.LogError("Cannot move node to point " + newPointIndex + ". Point already connected to a node");
			return;
		}
		Node node = GetNode(pointIndex);
		if (node == null)
		{
			Debug.LogError("No node connected to point " + pointIndex);
			return;
		}
		DisconnectNode(pointIndex);
		ConnectNode(node, newPointIndex);
	}

	public void ShiftNodes(int startIndex, int endIndex, int shift)
	{
		if (startIndex < endIndex)
		{
			for (int num = endIndex; num >= startIndex; num--)
			{
				if (GetNode(num) != null)
				{
					TransferNode(num, num + shift);
				}
			}
			return;
		}
		for (int num2 = startIndex; num2 >= endIndex; num2--)
		{
			if (GetNode(num2) != null)
			{
				TransferNode(num2, num2 + shift);
			}
		}
	}

	public void GetConnectedComputers(List<SplineComputer> computers, List<int> connectionIndices, List<int> connectedIndices, double percent, Spline.Direction direction, bool includeEqual)
	{
		if (computers == null)
		{
			computers = new List<SplineComputer>();
		}
		if (connectionIndices == null)
		{
			connectionIndices = new List<int>();
		}
		if (connectedIndices == null)
		{
			connectionIndices = new List<int>();
		}
		computers.Clear();
		connectionIndices.Clear();
		connectedIndices.Clear();
		int num = Mathf.FloorToInt((float)(pointCount - 1) * (float)percent);
		for (int i = 0; i < nodes.Length; i++)
		{
			bool flag = false;
			if (includeEqual)
			{
				flag = ((direction != Spline.Direction.Forward) ? (nodes[i].pointIndex <= num) : (nodes[i].pointIndex >= num));
			}
			if (!flag)
			{
				continue;
			}
			Node.Connection[] connections = nodes[i].node.GetConnections();
			for (int j = 0; j < connections.Length; j++)
			{
				if (connections[j].spline != this)
				{
					computers.Add(connections[j].spline);
					connectionIndices.Add(nodes[i].pointIndex);
					connectedIndices.Add(connections[j].pointIndex);
				}
			}
		}
	}

	public List<SplineComputer> GetConnectedComputers()
	{
		List<SplineComputer> computers = new List<SplineComputer>();
		computers.Add(this);
		if (nodes.Length == 0)
		{
			return computers;
		}
		GetConnectedComputers(ref computers);
		return computers;
	}

	public void GetSamplingValues(double percent, out int index, out double lerp)
	{
		sampleCollection.GetSamplingValues(percent, out index, out lerp);
	}

	private void GetConnectedComputers(ref List<SplineComputer> computers)
	{
		SplineComputer splineComputer = computers[computers.Count - 1];
		if (splineComputer == null)
		{
			return;
		}
		for (int i = 0; i < splineComputer.nodes.Length; i++)
		{
			if (splineComputer.nodes[i].node == null)
			{
				continue;
			}
			Node.Connection[] connections = splineComputer.nodes[i].node.GetConnections();
			for (int j = 0; j < connections.Length; j++)
			{
				bool flag = false;
				if (connections[j].spline == this)
				{
					continue;
				}
				for (int k = 0; k < computers.Count; k++)
				{
					if (computers[k] == connections[j].spline)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					computers.Add(connections[j].spline);
					GetConnectedComputers(ref computers);
				}
			}
		}
	}

	private void RemoveNodeLinkAt(int index)
	{
		NodeLink[] array = new NodeLink[nodes.Length - 1];
		for (int i = 0; i < nodes.Length; i++)
		{
			if (i != index)
			{
				if (i < index)
				{
					array[i] = nodes[i];
				}
				else
				{
					array[i - 1] = nodes[i];
				}
			}
		}
		nodes = array;
	}

	private void SetNodeForPoint(int index, SplinePoint worldPoint)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].pointIndex == index)
			{
				nodes[i].node.UpdatePoint(this, nodes[i].pointIndex, worldPoint);
				break;
			}
		}
	}

	private void UpdateConnectedNodes(SplinePoint[] worldPoints)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].node == null)
			{
				RemoveNodeLinkAt(i);
				i--;
				Rebuild();
				continue;
			}
			bool flag = false;
			Node.Connection[] connections = nodes[i].node.GetConnections();
			for (int j = 0; j < connections.Length; j++)
			{
				if (connections[j].spline == this)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RemoveNodeLinkAt(i);
				i--;
				Rebuild();
			}
			else
			{
				nodes[i].node.UpdatePoint(this, nodes[i].pointIndex, worldPoints[nodes[i].pointIndex]);
				nodes[i].node.UpdateConnectedComputers(this);
			}
		}
	}

	private void UpdateConnectedNodes()
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].node == null)
			{
				RemoveNodeLinkAt(i);
				Rebuild();
				i--;
				continue;
			}
			bool flag = false;
			Node.Connection[] connections = nodes[i].node.GetConnections();
			for (int j = 0; j < connections.Length; j++)
			{
				if (connections[j].spline == this && connections[j].pointIndex == nodes[i].pointIndex)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				nodes[i].node.UpdatePoint(this, nodes[i].pointIndex, GetPoint(nodes[i].pointIndex));
				nodes[i].node.UpdateConnectedComputers(this);
			}
			else
			{
				RemoveNodeLinkAt(i);
				Rebuild();
				i--;
			}
		}
	}

	public Vector3 TransformPoint(Vector3 point)
	{
		return transformMatrix.MultiplyPoint3x4(point);
	}

	public Vector3 InverseTransformPoint(Vector3 point)
	{
		return inverseTransformMatrix.MultiplyPoint3x4(point);
	}

	public Vector3 TransformDirection(Vector3 direction)
	{
		return transformMatrix.MultiplyVector(direction);
	}

	public Vector3 InverseTransformDirection(Vector3 direction)
	{
		return inverseTransformMatrix.MultiplyVector(direction);
	}
}
