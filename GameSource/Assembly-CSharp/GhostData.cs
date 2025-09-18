using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GhostData
{
	public enum GhostEvent
	{
		Invalid,
		Position,
		AnimState,
		SecondaryAnim,
		Flipped,
		Zombie,
		Jetpack,
		Coin,
		Stopwatch
	}

	public struct GhostDataPoint : IEnumerable<KeyValuePair<GhostEvent, object>>, IEnumerable
	{
		public float timestamp;

		public float frameTimestamp;

		public Vector3 position;

		public Vector3 framePosition;

		public bool valid;

		private Dictionary<GhostEvent, object> eventVals;

		public bool interpolated;

		public void AddData(GhostEvent eventType, object data)
		{
			eventVals.Add(eventType, data);
		}

		public IEnumerator<KeyValuePair<GhostEvent, object>> GetEnumerator()
		{
			return eventVals.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return eventVals.GetEnumerator();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0:F3}: ({1,5:F2},{2,-5:F2})", timestamp, position.x, position.y);
			foreach (KeyValuePair<GhostEvent, object> eventVal in eventVals)
			{
				stringBuilder.AppendFormat(" - {0}: {1}", eventVal.Key, eventVal.Value);
			}
			return stringBuilder.ToString();
		}

		public GhostDataPoint(float timestamp, Vector3 position, bool valid = true)
		{
			this.timestamp = timestamp;
			frameTimestamp = timestamp;
			this.position = position;
			framePosition = position;
			this.valid = valid;
			interpolated = false;
			eventVals = new Dictionary<GhostEvent, object>();
		}

		public GhostDataPoint(float timestamp, float frameTimestamp, Vector3 position, Vector3 framePosition, bool valid = true)
		{
			this.timestamp = timestamp;
			this.frameTimestamp = frameTimestamp;
			this.position = position;
			this.framePosition = framePosition;
			this.valid = valid;
			interpolated = true;
			eventVals = new Dictionary<GhostEvent, object>();
		}

		public GhostDataPoint(GhostDataPoint orig)
		{
			timestamp = orig.timestamp;
			frameTimestamp = orig.frameTimestamp;
			position = orig.position;
			framePosition = orig.framePosition;
			valid = orig.valid;
			interpolated = true;
			eventVals = new Dictionary<GhostEvent, object>();
			foreach (KeyValuePair<GhostEvent, object> eventVal in orig.eventVals)
			{
				eventVals.Add(eventVal.Key, eventVal.Value);
			}
		}

		public void ResetData(float timestamp, Vector3 position, bool valid = true)
		{
			this.timestamp = timestamp;
			frameTimestamp = timestamp;
			this.position = position;
			framePosition = position;
			this.valid = valid;
			interpolated = false;
			eventVals.Clear();
		}

		public void ResetData(float timestamp, float frameTimestamp, Vector3 position, Vector3 framePosition, bool valid = true)
		{
			this.timestamp = timestamp;
			this.frameTimestamp = frameTimestamp;
			this.position = position;
			this.framePosition = framePosition;
			this.valid = valid;
			interpolated = true;
			eventVals.Clear();
		}
	}

	protected static Dictionary<GhostEvent, Type> typeRef = new Dictionary<GhostEvent, Type>
	{
		{
			GhostEvent.Invalid,
			typeof(bool)
		},
		{
			GhostEvent.Position,
			typeof(Vector3)
		},
		{
			GhostEvent.AnimState,
			typeof(int)
		},
		{
			GhostEvent.SecondaryAnim,
			typeof(int)
		},
		{
			GhostEvent.Flipped,
			typeof(int)
		},
		{
			GhostEvent.Zombie,
			typeof(bool)
		},
		{
			GhostEvent.Jetpack,
			typeof(bool)
		},
		{
			GhostEvent.Coin,
			typeof(int)
		},
		{
			GhostEvent.Stopwatch,
			typeof(bool)
		}
	};

	private List<GhostDataPoint> dataPoints;

	private int lastIndex;

	private float lastTime;

	private static GhostDataPoint invalid = new GhostDataPoint(0f, Vector3.zero, valid: false);

	private Character.Animals animal;

	private int[] outfits;

	private string playerName;

	public int[] Outfits => outfits;

	public string PlayerName => playerName;

	public GhostDataPoint LastDataPoint
	{
		get
		{
			if (dataPoints.Count > 0)
			{
				return dataPoints[dataPoints.Count - 1];
			}
			return invalid;
		}
	}

	public static Type GetEventDataType(GhostEvent eventType)
	{
		if (typeRef.ContainsKey(eventType))
		{
			return typeRef[eventType];
		}
		return typeof(bool);
	}

	public GhostData()
	{
		dataPoints = new List<GhostDataPoint>();
	}

	public GhostData GetCopy()
	{
		return new GhostData
		{
			dataPoints = new List<GhostDataPoint>(dataPoints),
			animal = animal,
			outfits = outfits,
			playerName = playerName
		};
	}

	public void SetCharacterInfo(Character.Animals animal, int[] outfitsWorn, string username)
	{
		this.animal = animal;
		outfits = outfitsWorn;
		playerName = username;
	}

	public void PopulateData(IEnumerable<GhostDataPoint> ghostData)
	{
		dataPoints = new List<GhostDataPoint>(ghostData);
	}

	public GhostDataPoint[] GetData()
	{
		return dataPoints.ToArray();
	}

	public void Reset()
	{
		lastIndex = 0;
		lastTime = 0f;
		dataPoints.Clear();
	}

	public void AddGhostData(GhostDataPoint dataPoint)
	{
		dataPoints.Add(dataPoint);
	}

	public GhostDataPoint GetDataForTime(float timestamp, bool interpolate = true)
	{
		if (dataPoints.Count == 0)
		{
			return invalid;
		}
		GhostDataPoint result = invalid;
		GhostDataPoint ghostDataPoint = invalid;
		for (int i = 0; i < dataPoints.Count; i++)
		{
			if (dataPoints[i].timestamp <= timestamp)
			{
				result = dataPoints[i];
				continue;
			}
			ghostDataPoint = dataPoints[i];
			break;
		}
		if (!result.valid)
		{
			return result;
		}
		if (!interpolate)
		{
			return result;
		}
		if (!ghostDataPoint.valid)
		{
			return result;
		}
		float t = (timestamp - result.timestamp) / (ghostDataPoint.timestamp - result.timestamp);
		GhostDataPoint result2 = new GhostDataPoint(timestamp, result.timestamp, Vector3.Lerp(result.position, ghostDataPoint.position, t), result.position);
		foreach (KeyValuePair<GhostEvent, object> item in result)
		{
			result2.AddData(item.Key, item.Value);
		}
		return result2;
	}

	public string printData()
	{
		StringBuilder stringBuilder = new StringBuilder("Ghost data:");
		foreach (GhostDataPoint dataPoint in dataPoints)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(dataPoint.ToString());
		}
		return stringBuilder.ToString();
	}
}
