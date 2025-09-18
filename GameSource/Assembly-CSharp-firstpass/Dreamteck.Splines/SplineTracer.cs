using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

public class SplineTracer : SplineUser
{
	public class NodeConnection
	{
		public Node node;

		public int point;

		public NodeConnection(Node node, int point)
		{
			this.node = node;
			this.point = point;
		}
	}

	public enum PhysicsMode
	{
		Transform,
		Rigidbody,
		Rigidbody2D
	}

	public delegate void JunctionHandler(List<NodeConnection> passed);

	private SplineTrigger[] triggerInvokeQueue = new SplineTrigger[0];

	private List<NodeConnection> nodeConnectionQueue = new List<NodeConnection>();

	private int addTriggerIndex;

	[HideInInspector]
	public bool applyDirectionRotation = true;

	[HideInInspector]
	public bool useTriggers;

	[HideInInspector]
	public int triggerGroup;

	[HideInInspector]
	[SerializeField]
	protected Spline.Direction _direction = Spline.Direction.Forward;

	[SerializeField]
	[HideInInspector]
	protected PhysicsMode _physicsMode;

	[SerializeField]
	[HideInInspector]
	protected TransformModule _motion;

	[SerializeField]
	[HideInInspector]
	protected Rigidbody targetRigidbody;

	[SerializeField]
	[HideInInspector]
	protected Rigidbody2D targetRigidbody2D;

	[SerializeField]
	[HideInInspector]
	protected Transform targetTransform;

	[SerializeField]
	[HideInInspector]
	protected SplineSample _result = new SplineSample();

	[SerializeField]
	[HideInInspector]
	protected SplineSample _finalResult = new SplineSample();

	private const double MIN_DELTA = 1E-06;

	public PhysicsMode physicsMode
	{
		get
		{
			return _physicsMode;
		}
		set
		{
			_physicsMode = value;
			RefreshTargets();
		}
	}

	public TransformModule motion
	{
		get
		{
			if (_motion == null)
			{
				_motion = new TransformModule();
			}
			return _motion;
		}
	}

	public SplineSample result => _result;

	public SplineSample modifiedResult => _finalResult;

	public Spline.Direction direction
	{
		get
		{
			return _direction;
		}
		set
		{
			if (value != _direction)
			{
				_direction = value;
				ApplyMotion();
			}
		}
	}

	public event JunctionHandler onNode;

	public event EmptySplineHandler onMotionApplied;

	protected virtual void Start()
	{
		RefreshTargets();
	}

	public virtual void SetPercent(double percent, bool checkTriggers = false, bool handleJunctions = false)
	{
		double percent2 = _result.percent;
		Evaluate(percent, _result);
		ApplyMotion();
		if (checkTriggers)
		{
			CheckTriggers(percent2, percent);
			InvokeTriggers();
		}
		if (handleJunctions)
		{
			CheckNodes(percent2, percent);
		}
	}

	public virtual void SetDistance(float distance, bool checkTriggers = false, bool handleJunctions = false)
	{
		double percent = _result.percent;
		Evaluate(Travel(0.0, distance), _result);
		ApplyMotion();
		if (checkTriggers)
		{
			CheckTriggers(percent, _result.percent);
			InvokeTriggers();
		}
		if (handleJunctions)
		{
			CheckNodes(percent, _result.percent);
		}
	}

	protected virtual Rigidbody GetRigidbody()
	{
		return GetComponent<Rigidbody>();
	}

	protected virtual Rigidbody2D GetRigidbody2D()
	{
		return GetComponent<Rigidbody2D>();
	}

	protected virtual Transform GetTransform()
	{
		return base.transform;
	}

	protected void ApplyMotion()
	{
		ModifySample(_result, _finalResult);
		motion.targetUser = this;
		motion.splineResult = _finalResult;
		if (applyDirectionRotation)
		{
			motion.direction = _direction;
		}
		else
		{
			motion.direction = Spline.Direction.Forward;
		}
		switch (_physicsMode)
		{
		case PhysicsMode.Transform:
			if (targetTransform == null)
			{
				RefreshTargets();
			}
			if (!(targetTransform == null))
			{
				motion.ApplyTransform(targetTransform);
				if (this.onMotionApplied != null)
				{
					this.onMotionApplied();
				}
			}
			break;
		case PhysicsMode.Rigidbody:
			if (targetRigidbody == null)
			{
				RefreshTargets();
				if (targetRigidbody == null)
				{
					throw new MissingComponentException("There is no Rigidbody attached to " + base.name + " but the Physics mode is set to use one.");
				}
			}
			motion.ApplyRigidbody(targetRigidbody);
			if (this.onMotionApplied != null)
			{
				this.onMotionApplied();
			}
			break;
		case PhysicsMode.Rigidbody2D:
			if (targetRigidbody2D == null)
			{
				RefreshTargets();
				if (targetRigidbody2D == null)
				{
					throw new MissingComponentException("There is no Rigidbody2D attached to " + base.name + " but the Physics mode is set to use one.");
				}
			}
			motion.ApplyRigidbody2D(targetRigidbody2D);
			if (this.onMotionApplied != null)
			{
				this.onMotionApplied();
			}
			break;
		}
	}

	protected void CheckNodes(double from, double to)
	{
		if (this.onNode == null || from == to)
		{
			return;
		}
		UnclipPercent(ref from);
		UnclipPercent(ref to);
		Spline.FormatFromTo(ref from, ref to);
		int num = base.spline.PercentToPointIndex(from, _direction);
		int num2 = base.spline.PercentToPointIndex(to, _direction);
		if (num != num2)
		{
			if (_direction == Spline.Direction.Forward)
			{
				for (int i = num + 1; i <= num2; i++)
				{
					NodeConnection junction = GetJunction(i);
					if (junction != null)
					{
						nodeConnectionQueue.Add(junction);
					}
				}
				return;
			}
			for (int num3 = num2 - 1; num3 >= num; num3--)
			{
				NodeConnection junction2 = GetJunction(num3);
				if (junction2 != null)
				{
					nodeConnectionQueue.Add(junction2);
				}
			}
		}
		else if (from < 1E-06 && to > from)
		{
			NodeConnection junction3 = GetJunction(0);
			if (junction3 != null)
			{
				nodeConnectionQueue.Add(junction3);
			}
		}
		else if (to > 0.999999 && from < to)
		{
			NodeConnection junction4 = GetJunction(base.spline.pointCount - 1);
			if (junction4 != null)
			{
				nodeConnectionQueue.Add(junction4);
			}
		}
	}

	protected void InvokeNodes()
	{
		if (nodeConnectionQueue.Count > 0)
		{
			this.onNode(nodeConnectionQueue);
			nodeConnectionQueue.Clear();
		}
	}

	protected void CheckTriggers(double from, double to)
	{
		if (!useTriggers || from == to)
		{
			return;
		}
		UnclipPercent(ref from);
		UnclipPercent(ref to);
		if (triggerGroup < 0 || triggerGroup >= base.spline.triggerGroups.Length)
		{
			return;
		}
		for (int i = 0; i < base.spline.triggerGroups[triggerGroup].triggers.Length; i++)
		{
			if (base.spline.triggerGroups[triggerGroup].triggers[i] != null && base.spline.triggerGroups[triggerGroup].triggers[i].Check(from, to))
			{
				AddTriggerToQueue(base.spline.triggerGroups[triggerGroup].triggers[i]);
			}
		}
	}

	private NodeConnection GetJunction(int pointIndex)
	{
		Node node = base.spline.GetNode(pointIndex);
		if (node == null)
		{
			return null;
		}
		return new NodeConnection(node, pointIndex);
	}

	protected void InvokeTriggers()
	{
		for (int i = 0; i < addTriggerIndex; i++)
		{
			if (triggerInvokeQueue[i] != null)
			{
				triggerInvokeQueue[i].Invoke(this);
			}
		}
		addTriggerIndex = 0;
	}

	protected void RefreshTargets()
	{
		switch (_physicsMode)
		{
		case PhysicsMode.Transform:
			targetTransform = GetTransform();
			break;
		case PhysicsMode.Rigidbody:
			targetRigidbody = GetRigidbody();
			break;
		case PhysicsMode.Rigidbody2D:
			targetRigidbody2D = GetRigidbody2D();
			break;
		}
	}

	private void AddTriggerToQueue(SplineTrigger trigger)
	{
		if (addTriggerIndex >= triggerInvokeQueue.Length)
		{
			SplineTrigger[] array = new SplineTrigger[triggerInvokeQueue.Length + base.spline.triggerGroups[triggerGroup].triggers.Length];
			triggerInvokeQueue.CopyTo(array, 0);
			triggerInvokeQueue = array;
		}
		triggerInvokeQueue[addTriggerIndex] = trigger;
		addTriggerIndex++;
	}
}
