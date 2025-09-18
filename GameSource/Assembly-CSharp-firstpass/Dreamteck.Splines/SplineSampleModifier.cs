using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class SplineSampleModifier
{
	[Serializable]
	public class Key
	{
		[SerializeField]
		private double _featherStart;

		[SerializeField]
		private double _featherEnd;

		[SerializeField]
		private double _centerStart = 0.25;

		[SerializeField]
		private double _centerEnd = 0.75;

		[SerializeField]
		internal SplineSampleModifier modifier;

		public AnimationCurve interpolation;

		public float blend = 1f;

		public double start
		{
			get
			{
				return _featherStart;
			}
			set
			{
				if (value != _featherStart)
				{
					_featherStart = DMath.Clamp01(value);
				}
			}
		}

		public double end
		{
			get
			{
				return _featherEnd;
			}
			set
			{
				if (value != _featherEnd)
				{
					_featherEnd = DMath.Clamp01(value);
				}
			}
		}

		public double centerStart
		{
			get
			{
				return _centerStart;
			}
			set
			{
				if (value != _centerStart)
				{
					_centerStart = DMath.Clamp01(value);
					if (_centerStart > _centerEnd)
					{
						_centerStart = _centerEnd;
					}
				}
			}
		}

		public double centerEnd
		{
			get
			{
				return _centerEnd;
			}
			set
			{
				if (value != _centerEnd)
				{
					_centerEnd = DMath.Clamp01(value);
					if (_centerEnd < _centerStart)
					{
						_centerEnd = _centerStart;
					}
				}
			}
		}

		public double globalCenterStart
		{
			get
			{
				return LocalToGlobalPercent(centerStart);
			}
			set
			{
				centerStart = DMath.Clamp01(GlobalToLocalPercent(value));
			}
		}

		public double globalCenterEnd
		{
			get
			{
				return LocalToGlobalPercent(centerEnd);
			}
			set
			{
				centerEnd = DMath.Clamp01(GlobalToLocalPercent(value));
			}
		}

		public double position
		{
			get
			{
				double num = DMath.Lerp(_centerStart, _centerEnd, 0.5);
				if (start > end)
				{
					double num2 = DMath.Lerp(_featherStart, _featherEnd, num);
					double num3 = 1.0 - _featherStart;
					double num4 = num * (num3 + _featherEnd);
					num2 = _featherStart + num4;
					if (num2 > 1.0)
					{
						num2 -= 1.0;
					}
					return num2;
				}
				return DMath.Lerp(_featherStart, _featherEnd, num);
			}
			set
			{
				double num = value - position;
				start += num;
				end += num;
			}
		}

		internal Key(double f, double t, SplineSampleModifier modifier)
		{
			this.modifier = modifier;
			start = f;
			end = t;
			interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		}

		private double GlobalToLocalPercent(double t)
		{
			if (_featherStart > _featherEnd)
			{
				if (t > _featherStart)
				{
					return DMath.InverseLerp(_featherStart, _featherStart + (1.0 - _featherStart) + _featherEnd, t);
				}
				if (t < _featherEnd)
				{
					return DMath.InverseLerp(0.0 - (1.0 - _featherStart), _featherEnd, t);
				}
				return 0.0;
			}
			return DMath.InverseLerp(_featherStart, _featherEnd, t);
		}

		private double LocalToGlobalPercent(double t)
		{
			if (_featherStart > _featherEnd)
			{
				t = DMath.Lerp(_featherStart, _featherStart + (1.0 - _featherStart) + _featherEnd, t);
				if (t > 1.0)
				{
					t -= 1.0;
				}
				return t;
			}
			return DMath.Lerp(_featherStart, _featherEnd, t);
		}

		public float Evaluate(double t)
		{
			t = (float)GlobalToLocalPercent(t);
			if (t < _centerStart)
			{
				return interpolation.Evaluate((float)t / (float)_centerStart) * blend;
			}
			if (t > _centerEnd)
			{
				return interpolation.Evaluate(1f - (float)DMath.InverseLerp(_centerEnd, 1.0, t)) * blend;
			}
			return interpolation.Evaluate(1f) * blend;
		}

		public virtual Key Duplicate()
		{
			return new Key(start, end, modifier)
			{
				_centerStart = _centerStart,
				_centerEnd = _centerEnd,
				blend = blend,
				interpolation = DuplicateUtility.DuplicateCurve(interpolation)
			};
		}
	}

	public float blend = 1f;

	public virtual List<Key> GetKeys()
	{
		return new List<Key>();
	}

	public virtual void SetKeys(List<Key> input)
	{
		for (int i = 0; i < input.Count; i++)
		{
			input[i].modifier = this;
		}
	}

	public virtual void Apply(SplineSample result)
	{
	}

	public virtual void Apply(SplineSample source, SplineSample destination)
	{
		destination.CopyFrom(source);
		Apply(destination);
	}
}
