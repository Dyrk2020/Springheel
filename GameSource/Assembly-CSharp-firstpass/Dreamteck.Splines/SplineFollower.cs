using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Users/Spline Follower")]
public class SplineFollower : SplineTracer
{
	public enum FollowMode
	{
		Uniform,
		Time
	}

	public enum Wrap
	{
		Default,
		Loop,
		PingPong
	}

	[Serializable]
	public class FloatEvent : UnityEvent<float>
	{
	}

	[HideInInspector]
	public Wrap wrapMode;

	[HideInInspector]
	public FollowMode followMode;

	[HideInInspector]
	public bool autoStartPosition;

	[HideInInspector]
	public bool follow = true;

	[HideInInspector]
	public bool preserveUniformSpeedWithOffset;

	[SerializeField]
	[HideInInspector]
	private float _followSpeed = 1f;

	[SerializeField]
	[HideInInspector]
	private float _followDuration = 1f;

	[SerializeField]
	[HideInInspector]
	[Range(0f, 1f)]
	private double _startPosition;

	[HideInInspector]
	[SerializeField]
	private FollowerSpeedModifier _speedModifier = new FollowerSpeedModifier();

	[HideInInspector]
	[SerializeField]
	private FloatEvent _unityOnEndReached;

	[HideInInspector]
	[SerializeField]
	private FloatEvent _unityOnBeginningReached;

	private double lastClippedPercent = -1.0;

	private bool followStarted;

	public float followSpeed
	{
		get
		{
			return _followSpeed;
		}
		set
		{
			if (_followSpeed != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				_followSpeed = value;
			}
		}
	}

	public double startPosition
	{
		get
		{
			return _startPosition;
		}
		set
		{
			if (value != _startPosition)
			{
				_startPosition = DMath.Clamp01(value);
				if (!followStarted)
				{
					SetPercent(_startPosition);
				}
			}
		}
	}

	public float followDuration
	{
		get
		{
			return _followDuration;
		}
		set
		{
			if (_followDuration != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				_followDuration = value;
			}
		}
	}

	public FollowerSpeedModifier speedModifier => _speedModifier;

	public event Action<double> onEndReached;

	public event Action<double> onBeginningReached;

	protected override void Start()
	{
		base.Start();
		if (follow && autoStartPosition)
		{
			SetPercent(base.spline.Project(GetTransform().position).percent);
		}
	}

	protected override void LateRun()
	{
		base.LateRun();
		if (follow)
		{
			Follow();
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		Evaluate(_result.percent, _result);
		if (follow && !autoStartPosition)
		{
			ApplyMotion();
		}
	}

	private void Follow()
	{
		if (!followStarted)
		{
			if (autoStartPosition)
			{
				Project(GetTransform().position, evalResult);
				SetPercent(evalResult.percent);
			}
			else
			{
				SetPercent(_startPosition);
			}
		}
		followStarted = true;
		switch (followMode)
		{
		case FollowMode.Uniform:
		{
			float num = _followSpeed + _speedModifier.GetSpeed(base.result);
			Move(Time.deltaTime * num);
			break;
		}
		case FollowMode.Time:
			if ((double)_followDuration == 0.0)
			{
				Move(0.0);
			}
			else
			{
				Move((double)Time.deltaTime / (double)_followDuration);
			}
			break;
		}
	}

	public void Restart(double startPosition = 0.0)
	{
		followStarted = false;
		SetPercent(startPosition);
	}

	public override void SetPercent(double percent, bool checkTriggers = false, bool handleJuncitons = false)
	{
		base.SetPercent(percent, checkTriggers, handleJuncitons);
		lastClippedPercent = percent;
	}

	public override void SetDistance(float distance, bool checkTriggers = false, bool handleJuncitons = false)
	{
		base.SetDistance(distance, checkTriggers, handleJuncitons);
		lastClippedPercent = ClipPercent(_result.percent);
		if (base.samplesAreLooped && base.clipFrom == base.clipTo && distance > 0f && lastClippedPercent == 0.0)
		{
			lastClippedPercent = 1.0;
		}
	}

	public void Move(double percent)
	{
		if (percent == 0.0)
		{
			return;
		}
		if (base.sampleCount <= 1)
		{
			if (base.sampleCount == 1)
			{
				_result.CopyFrom(GetSampleRaw(0));
				ApplyMotion();
			}
			return;
		}
		Evaluate(_result.percent, _result);
		double num = _result.percent;
		if (wrapMode == Wrap.Default && lastClippedPercent >= 1.0 && num == 0.0)
		{
			num = 1.0;
		}
		double num2 = num + ((_direction == Spline.Direction.Forward) ? percent : (0.0 - percent));
		bool flag = false;
		bool flag2 = false;
		lastClippedPercent = num2;
		if (_direction == Spline.Direction.Forward && num2 >= 1.0)
		{
			if (num < 1.0)
			{
				flag = true;
			}
			switch (wrapMode)
			{
			case Wrap.Default:
				num2 = 1.0;
				break;
			case Wrap.Loop:
				CheckTriggers(num, 1.0);
				CheckNodes(num, 1.0);
				while (num2 > 1.0)
				{
					num2 -= 1.0;
				}
				num = 0.0;
				break;
			case Wrap.PingPong:
				num2 = DMath.Clamp01(1.0 - (num2 - 1.0));
				num = 1.0;
				_direction = Spline.Direction.Backward;
				break;
			}
		}
		else if (_direction == Spline.Direction.Backward && num2 <= 0.0)
		{
			if (num > 0.0)
			{
				flag2 = true;
			}
			switch (wrapMode)
			{
			case Wrap.Default:
				num2 = 0.0;
				break;
			case Wrap.Loop:
				CheckTriggers(num, 0.0);
				CheckNodes(num, 0.0);
				for (; num2 < 0.0; num2 += 1.0)
				{
				}
				num = 1.0;
				break;
			case Wrap.PingPong:
				num2 = DMath.Clamp01(0.0 - num2);
				num = 0.0;
				_direction = Spline.Direction.Forward;
				break;
			}
		}
		CheckTriggers(num, num2);
		CheckNodes(num, num2);
		Evaluate(num2, _result);
		ApplyMotion();
		if (flag)
		{
			if (this.onEndReached != null)
			{
				this.onEndReached(num);
			}
			_unityOnEndReached.Invoke((float)num);
		}
		else if (flag2)
		{
			if (this.onBeginningReached != null)
			{
				this.onBeginningReached(num);
			}
			_unityOnBeginningReached.Invoke((float)num);
		}
		InvokeTriggers();
		InvokeNodes();
	}

	public void Move(float distance)
	{
		bool flag = false;
		bool flag2 = false;
		float moved = 0f;
		double percent = _result.percent;
		_result.percent = DoTravel(_result.percent, distance, out moved);
		if (percent != _result.percent)
		{
			CheckTriggers(percent, _result.percent);
			CheckNodes(percent, _result.percent);
		}
		if (base.direction == Spline.Direction.Forward)
		{
			if (_result.percent >= 1.0)
			{
				if (percent < 1.0)
				{
					flag = true;
				}
				switch (wrapMode)
				{
				case Wrap.Loop:
					_result.percent = DoTravel(0.0, distance - moved, out moved);
					CheckTriggers(0.0, _result.percent);
					CheckNodes(0.0, _result.percent);
					break;
				case Wrap.PingPong:
					_direction = Spline.Direction.Backward;
					_result.percent = DoTravel(1.0, distance - moved, out moved);
					CheckTriggers(1.0, _result.percent);
					CheckNodes(1.0, _result.percent);
					break;
				}
			}
		}
		else if (_result.percent <= 0.0)
		{
			if (percent > 0.0)
			{
				flag2 = true;
			}
			switch (wrapMode)
			{
			case Wrap.Loop:
				_result.percent = DoTravel(1.0, distance - moved, out moved);
				CheckTriggers(1.0, _result.percent);
				CheckNodes(1.0, _result.percent);
				break;
			case Wrap.PingPong:
				_direction = Spline.Direction.Forward;
				_result.percent = DoTravel(0.0, distance - moved, out moved);
				CheckTriggers(0.0, _result.percent);
				CheckNodes(0.0, _result.percent);
				break;
			}
		}
		Evaluate(_result.percent, _result);
		ApplyMotion();
		if (flag)
		{
			if (this.onEndReached != null)
			{
				this.onEndReached(percent);
			}
			_unityOnEndReached.Invoke((float)percent);
		}
		else if (flag2)
		{
			if (this.onBeginningReached != null)
			{
				this.onBeginningReached(percent);
			}
			_unityOnBeginningReached.Invoke((float)percent);
		}
		InvokeTriggers();
		InvokeNodes();
	}

	protected virtual double DoTravel(double start, float distance, out float moved)
	{
		moved = 0f;
		double num = 0.0;
		if (preserveUniformSpeedWithOffset && _motion.hasOffset)
		{
			return TravelWithOffset(start, distance, _direction, _motion.offset, out moved);
		}
		return Travel(start, distance, _direction, out moved);
	}
}
