using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines;

[AddComponentMenu("Dreamteck/Splines/Morph")]
public class SplineMorph : MonoBehaviour
{
	public enum CycleMode
	{
		Default,
		Loop,
		PingPong
	}

	public enum UpdateMode
	{
		Update,
		FixedUpdate,
		LateUpdate
	}

	[Serializable]
	public class Channel
	{
		public enum Interpolation
		{
			Linear,
			Spherical
		}

		[SerializeField]
		internal SplinePoint[] points = new SplinePoint[0];

		[SerializeField]
		internal float percent = 1f;

		public string name = "";

		public AnimationCurve curve;

		public Interpolation interpolation;
	}

	[HideInInspector]
	public SplineComputer.Space space = SplineComputer.Space.Local;

	[HideInInspector]
	public bool cycle;

	[HideInInspector]
	public CycleMode cycleMode;

	[HideInInspector]
	public UpdateMode cycleUpdateMode;

	[HideInInspector]
	public float cycleDuration = 1f;

	[SerializeField]
	[HideInInspector]
	private SplineComputer _spline;

	private SplinePoint[] points = new SplinePoint[0];

	private float cycleValue;

	private short cycleDirection = 1;

	[FormerlySerializedAs("morphStates")]
	[SerializeField]
	[HideInInspector]
	private Channel[] channels = new Channel[0];

	public SplineComputer spline
	{
		get
		{
			return _spline;
		}
		set
		{
			if (Application.isPlaying && channels.Length != 0 && value.pointCount != channels[0].points.Length)
			{
				value.SetPoints(channels[0].points, space);
			}
			_spline = value;
		}
	}

	private void Reset()
	{
		spline = GetComponent<SplineComputer>();
	}

	private void Update()
	{
		if (cycleUpdateMode == UpdateMode.Update)
		{
			RunUpdate();
		}
	}

	private void FixedUpdate()
	{
		if (cycleUpdateMode == UpdateMode.FixedUpdate)
		{
			RunUpdate();
		}
	}

	private void LateUpdate()
	{
		if (cycleUpdateMode == UpdateMode.LateUpdate)
		{
			RunUpdate();
		}
	}

	private void RunUpdate()
	{
		if (!cycle)
		{
			return;
		}
		if (cycleMode != CycleMode.PingPong)
		{
			cycleDirection = 1;
		}
		cycleValue += Time.deltaTime / cycleDuration * (float)cycleDirection;
		switch (cycleMode)
		{
		case CycleMode.Default:
			if (cycleValue > 1f)
			{
				cycleValue = 1f;
			}
			break;
		case CycleMode.Loop:
			if (cycleValue > 1f)
			{
				cycleValue -= Mathf.Floor(cycleValue);
			}
			break;
		case CycleMode.PingPong:
			if (cycleValue > 1f)
			{
				cycleValue = 1f - (cycleValue - Mathf.Floor(cycleValue));
				cycleDirection = -1;
			}
			else if (cycleValue < 0f)
			{
				cycleValue = 0f - cycleValue - Mathf.Floor(0f - cycleValue);
				cycleDirection = 1;
			}
			break;
		}
		SetWeight(cycleValue, cycleMode == CycleMode.Loop);
	}

	public void SetCycle(float value)
	{
		cycleValue = Mathf.Clamp01(value);
	}

	public void SetWeight(int index, float weight)
	{
		channels[index].percent = Mathf.Clamp01(weight);
		UpdateMorph();
	}

	public void SetWeight(string name, float weight)
	{
		int channelIndex = GetChannelIndex(name);
		channels[channelIndex].percent = Mathf.Clamp01(weight);
		UpdateMorph();
	}

	public void SetWeight(float percent, bool loop = false)
	{
		float num = percent * (float)(loop ? channels.Length : (channels.Length - 1));
		for (int i = 0; i < channels.Length; i++)
		{
			if (Mathf.Abs((float)i - num) > 1f)
			{
				SetWeight(i, 0f);
			}
			else if (num <= (float)i)
			{
				SetWeight(i, 1f - ((float)i - num));
			}
			else
			{
				SetWeight(i, 1f - (num - (float)i));
			}
		}
		if (loop && num >= (float)(channels.Length - 1))
		{
			SetWeight(0, num - (float)(channels.Length - 1));
		}
	}

	public void CaptureSnapshot(string name)
	{
		CaptureSnapshot(GetChannelIndex(name));
	}

	public void CaptureSnapshot(int index)
	{
		if (!(_spline == null))
		{
			if (channels.Length != 0 && _spline.pointCount != channels[0].points.Length && index != 0)
			{
				Debug.LogError("Point count must be the same as " + _spline.pointCount);
				return;
			}
			channels[index].points = _spline.GetPoints(space);
			UpdateMorph();
		}
	}

	public void Clear()
	{
		channels = new Channel[0];
	}

	public SplinePoint[] GetSnapshot(int index)
	{
		return channels[index].points;
	}

	public SplinePoint[] GetSnapshot(string name)
	{
		int channelIndex = GetChannelIndex(name);
		return channels[channelIndex].points;
	}

	public float GetWeight(int index)
	{
		return channels[index].percent;
	}

	public float GetWeight(string name)
	{
		int channelIndex = GetChannelIndex(name);
		return channels[channelIndex].percent;
	}

	public void AddChannel(string name)
	{
		if (!(_spline == null))
		{
			if (channels.Length != 0 && _spline.pointCount != channels[0].points.Length)
			{
				Debug.LogError("Point count must be the same as " + channels[0].points.Length);
				return;
			}
			Channel channel = new Channel();
			channel.points = _spline.GetPoints(space);
			channel.name = name;
			channel.curve = new AnimationCurve();
			channel.curve.AddKey(new Keyframe(0f, 0f, 0f, 1f));
			channel.curve.AddKey(new Keyframe(1f, 1f, 1f, 0f));
			ArrayUtility.Add(ref channels, channel);
			UpdateMorph();
		}
	}

	public void RemoveChannel(string name)
	{
		int channelIndex = GetChannelIndex(name);
		RemoveChannel(channelIndex);
	}

	public void RemoveChannel(int index)
	{
		if (index < 0 || index >= channels.Length)
		{
			return;
		}
		Channel[] array = new Channel[channels.Length - 1];
		for (int i = 0; i < channels.Length; i++)
		{
			if (i != index)
			{
				if (i < index)
				{
					array[i] = channels[i];
				}
				else if (i >= index)
				{
					array[i - 1] = channels[i];
				}
			}
		}
		channels = array;
		UpdateMorph();
	}

	private int GetChannelIndex(string name)
	{
		for (int i = 0; i < channels.Length; i++)
		{
			if (channels[i].name == name)
			{
				return i;
			}
		}
		Debug.Log("Channel not found " + name);
		return 0;
	}

	public int GetChannelCount()
	{
		if (channels == null)
		{
			return 0;
		}
		return channels.Length;
	}

	public Channel GetChannel(int index)
	{
		return channels[index];
	}

	public Channel GetChannel(string name)
	{
		return channels[GetChannelIndex(name)];
	}

	public void UpdateMorph()
	{
		if (_spline == null || channels.Length == 0)
		{
			return;
		}
		if (points.Length != channels[0].points.Length)
		{
			points = new SplinePoint[channels[0].points.Length];
		}
		for (int i = 0; i < channels.Length; i++)
		{
			for (int j = 0; j < points.Length; j++)
			{
				if (i == 0)
				{
					points[j] = channels[0].points[j];
					continue;
				}
				float num = channels[i].curve.Evaluate(channels[i].percent);
				if (channels[i].interpolation == Channel.Interpolation.Linear)
				{
					points[j].position += (channels[i].points[j].position - channels[0].points[j].position) * num;
					points[j].tangent += (channels[i].points[j].tangent - channels[0].points[j].tangent) * num;
					points[j].tangent2 += (channels[i].points[j].tangent2 - channels[0].points[j].tangent2) * num;
					points[j].normal += (channels[i].points[j].normal - channels[0].points[j].normal) * num;
				}
				else
				{
					points[j].position = Vector3.Slerp(points[j].position, points[j].position + (channels[i].points[j].position - channels[0].points[j].position), num);
					points[j].tangent = Vector3.Slerp(points[j].tangent, points[j].tangent + (channels[i].points[j].tangent - channels[0].points[j].tangent), num);
					points[j].tangent2 = Vector3.Slerp(points[j].tangent2, points[j].tangent2 + (channels[i].points[j].tangent2 - channels[0].points[j].tangent2), num);
					points[j].normal = Vector3.Slerp(points[j].normal, points[j].normal + (channels[i].points[j].normal - channels[0].points[j].normal), num);
				}
				points[j].color += (channels[i].points[j].color - channels[0].points[j].color) * num;
				points[j].size += (channels[i].points[j].size - channels[0].points[j].size) * num;
				if (points[j].type == SplinePoint.Type.SmoothMirrored)
				{
					points[j].type = channels[i].points[j].type;
				}
				else if (points[j].type == SplinePoint.Type.SmoothFree && channels[i].points[j].type == SplinePoint.Type.Broken)
				{
					points[j].type = SplinePoint.Type.Broken;
				}
			}
		}
		for (int k = 0; k < points.Length; k++)
		{
			points[k].normal.Normalize();
		}
		_spline.SetPoints(points, space);
	}
}
