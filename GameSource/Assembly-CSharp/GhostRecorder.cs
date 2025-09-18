using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GhostRecorder : MonoBehaviour
{
	private Character trackedChar;

	private bool tracking;

	private bool paused;

	private float timeOffset;

	private GhostData ghostData;

	private GhostData.GhostDataPoint nextDataPoint;

	private float pauseTime;

	private Dictionary<GhostData.GhostEvent, object> previousValues;

	private float maxGhostDistance = 1f;

	private float maxDistSq;

	public bool IsTracking => tracking;

	public Character TrackedCharacter => trackedChar;

	public bool IsPaused => paused;

	public GhostData CurrentGhostData => ghostData;

	private void Awake()
	{
		ghostData = new GhostData();
		nextDataPoint = new GhostData.GhostDataPoint(0f, Vector3.zero, valid: false);
		maxDistSq = maxGhostDistance * maxGhostDistance;
	}

	private void Update()
	{
		if (!tracking || paused || !(trackedChar != null))
		{
			return;
		}
		float num = Time.realtimeSinceStartup - timeOffset;
		if (num < 0f)
		{
			Debug.LogError("Negative timestamp! " + num);
			return;
		}
		Vector3 position = trackedChar.transform.position;
		nextDataPoint.ResetData(num, position);
		bool flag = false;
		if ((position - ghostData.LastDataPoint.position).sqrMagnitude > maxDistSq)
		{
			flag = true;
		}
		foreach (KeyValuePair<GhostData.GhostEvent, object> previousValue in previousValues)
		{
			switch (previousValue.Key)
			{
			case GhostData.GhostEvent.AnimState:
				if ((Character.AnimState)previousValue.Value != trackedChar.CurrentAnim)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.CurrentAnim);
					flag = true;
				}
				break;
			case GhostData.GhostEvent.SecondaryAnim:
				if ((Character.SecondaryAnimState)previousValue.Value != trackedChar.SecondaryAnim)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.SecondaryAnim);
					flag = true;
				}
				break;
			case GhostData.GhostEvent.Flipped:
				if ((int)previousValue.Value != trackedChar.FlipSpriteX)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.FlipSpriteX);
					flag = true;
				}
				break;
			case GhostData.GhostEvent.Zombie:
				if ((bool)previousValue.Value != trackedChar.isZombie)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.isZombie);
					flag = true;
				}
				break;
			case GhostData.GhostEvent.Jetpack:
				if ((bool)previousValue.Value != trackedChar.Jetpacking)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.Jetpacking);
					flag = true;
				}
				break;
			case GhostData.GhostEvent.Coin:
				if ((int)previousValue.Value != trackedChar.CoinsCollected)
				{
					nextDataPoint.AddData(previousValue.Key, trackedChar.CoinsCollected);
					flag = true;
					Debug.Log("GHOST: Added coin keyframe: " + previousValue.Value?.ToString() + "->" + trackedChar.CoinsCollected);
				}
				break;
			case GhostData.GhostEvent.Stopwatch:
				if ((bool)previousValue.Value != Timekeeper.Slowing)
				{
					nextDataPoint.AddData(previousValue.Key, Timekeeper.Slowing);
					flag = true;
					Debug.Log("GHOST: Added stopwatch keyframe: " + previousValue.Value?.ToString() + "->" + Timekeeper.Slowing);
				}
				break;
			default:
				Debug.LogWarning("Bad item in ghost recorder's list of previous values: " + previousValue.Key.ToString() + " - " + previousValue.Value);
				break;
			case GhostData.GhostEvent.Position:
				break;
			}
		}
		if (flag)
		{
			ghostData.AddGhostData(new GhostData.GhostDataPoint(nextDataPoint));
			updatePreviousValues();
		}
	}

	private void updatePreviousValues()
	{
		if (trackedChar == null)
		{
			previousValues.Clear();
			return;
		}
		previousValues[GhostData.GhostEvent.AnimState] = trackedChar.CurrentAnim;
		previousValues[GhostData.GhostEvent.Coin] = trackedChar.CoinsCollected;
		previousValues[GhostData.GhostEvent.Flipped] = trackedChar.FlipSpriteX;
		previousValues[GhostData.GhostEvent.Jetpack] = trackedChar.Jetpacking;
		previousValues[GhostData.GhostEvent.SecondaryAnim] = trackedChar.SecondaryAnim;
		previousValues[GhostData.GhostEvent.Stopwatch] = Timekeeper.Slowing;
		previousValues[GhostData.GhostEvent.Zombie] = trackedChar.isZombie;
	}

	private void resetPreviousValues()
	{
	}

	public void TrackCharacter(Character characterToTrack)
	{
		if (ghostData != null)
		{
			ghostData.Reset();
		}
		trackedChar = characterToTrack;
	}

	public void StartTracking(Character characterToTrack)
	{
		TrackCharacter(characterToTrack);
		StartTracking();
	}

	public void StartTracking()
	{
		Debug.Log("Resetting tracking");
		if (trackedChar == null)
		{
			Debug.LogError("Ghost recorder cannot start tracking. No character to track");
			return;
		}
		if (tracking)
		{
			Debug.LogWarning("Ghost recorder is already tracking.");
			return;
		}
		ghostData.Reset();
		tracking = true;
		paused = false;
		timeOffset = Time.realtimeSinceStartup;
		nextDataPoint.ResetData(0f, trackedChar.transform.position);
		nextDataPoint.AddData(GhostData.GhostEvent.AnimState, Character.AnimState.IDLE);
		nextDataPoint.AddData(GhostData.GhostEvent.Coin, 0);
		nextDataPoint.AddData(GhostData.GhostEvent.Flipped, 1);
		nextDataPoint.AddData(GhostData.GhostEvent.Jetpack, false);
		nextDataPoint.AddData(GhostData.GhostEvent.SecondaryAnim, Character.SecondaryAnimState.NONE);
		nextDataPoint.AddData(GhostData.GhostEvent.Stopwatch, false);
		nextDataPoint.AddData(GhostData.GhostEvent.Zombie, false);
		ghostData.AddGhostData(new GhostData.GhostDataPoint(nextDataPoint));
		ghostData.SetCharacterInfo(trackedChar.CharacterSprite, trackedChar.GetOutfitsAsArray(), trackedChar.nameTag.Currentname);
		previousValues = new Dictionary<GhostData.GhostEvent, object>
		{
			{
				GhostData.GhostEvent.AnimState,
				Character.AnimState.IDLE
			},
			{
				GhostData.GhostEvent.Coin,
				0
			},
			{
				GhostData.GhostEvent.Flipped,
				1
			},
			{
				GhostData.GhostEvent.Jetpack,
				false
			},
			{
				GhostData.GhostEvent.SecondaryAnim,
				Character.SecondaryAnimState.NONE
			},
			{
				GhostData.GhostEvent.Stopwatch,
				false
			},
			{
				GhostData.GhostEvent.Zombie,
				false
			}
		};
	}

	public void UpdateCharacter()
	{
		ghostData.SetCharacterInfo(trackedChar.CharacterSprite, trackedChar.GetOutfitsAsArray(), trackedChar.nameTag.Currentname);
	}

	public void PauseTracking()
	{
		paused = true;
		pauseTime = Time.realtimeSinceStartup;
	}

	public void ResumeTracking()
	{
		if (!tracking)
		{
			Debug.LogWarning("Resumed ghost recorder was not tracking");
		}
		timeOffset += Time.realtimeSinceStartup - pauseTime;
		paused = false;
	}

	public void Reset()
	{
		ghostData.Reset();
		timeOffset = Time.realtimeSinceStartup;
		nextDataPoint.ResetData(0f, trackedChar.transform.position);
		nextDataPoint.AddData(GhostData.GhostEvent.AnimState, trackedChar.CurrentAnim);
		nextDataPoint.AddData(GhostData.GhostEvent.SecondaryAnim, trackedChar.SecondaryAnim);
		nextDataPoint.AddData(GhostData.GhostEvent.Flipped, trackedChar.FlipSpriteX);
		ghostData.AddGhostData(new GhostData.GhostDataPoint(nextDataPoint));
	}

	public void StopTracking()
	{
		tracking = false;
		paused = false;
		pauseTime = Time.realtimeSinceStartup;
		trackedChar = null;
	}

	public string PrintCurrentData()
	{
		if (ghostData != null)
		{
			return ghostData.printData();
		}
		return "Ghost data:\nnull";
	}

	public byte[] SerializeData()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("GhostInfo");
		XmlElement xmlElement2 = xmlDocument.CreateElement("CharacterInfo");
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "PlayerName", ghostData.PlayerName);
		if (ghostData.Outfits != null)
		{
			foreach (object value in Enum.GetValues(typeof(Outfit.OutfitType)))
			{
				int num = (int)value;
				if (num < ghostData.Outfits.Length && num >= 0)
				{
					QuickSaver.AddAttribute(xmlDocument, xmlElement2, value.ToString(), ghostData.Outfits[num].ToString());
				}
			}
		}
		xmlElement.AppendChild(xmlElement2);
		XmlElement xmlElement3 = xmlDocument.CreateElement("GhostData");
		GhostData.GhostDataPoint[] data = ghostData.GetData();
		for (int i = 0; i < data.Length; i++)
		{
			GhostData.GhostDataPoint ghostDataPoint = data[i];
			XmlElement xmlElement4 = xmlDocument.CreateElement("datapoint");
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "valid", ghostDataPoint.valid.ToString());
			if (ghostDataPoint.valid)
			{
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "timestamp", ghostDataPoint.timestamp.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "frameTimestamp", ghostDataPoint.frameTimestamp.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "posX", ghostDataPoint.position.x.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "posY", ghostDataPoint.position.y.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "posZ", ghostDataPoint.position.z.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "framePosX", ghostDataPoint.framePosition.x.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "framePosY", ghostDataPoint.framePosition.y.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "framePosZ", ghostDataPoint.framePosition.z.ToString());
				QuickSaver.AddAttribute(xmlDocument, xmlElement4, "interpolated", ghostDataPoint.interpolated.ToString());
				foreach (KeyValuePair<GhostData.GhostEvent, object> item in ghostDataPoint)
				{
					if (GhostData.GetEventDataType(item.Key) == typeof(Vector3))
					{
						Vector3 vector = (Vector3)item.Value;
						QuickSaver.AddAttribute(xmlDocument, xmlElement4, item.Key.ToString() + "X", vector.x.ToString());
						QuickSaver.AddAttribute(xmlDocument, xmlElement4, item.Key.ToString() + "Y", vector.y.ToString());
						QuickSaver.AddAttribute(xmlDocument, xmlElement4, item.Key.ToString() + "Z", vector.z.ToString());
					}
					QuickSaver.AddAttribute(xmlDocument, xmlElement4, item.Key.ToString(), item.Value.ToString());
				}
			}
			xmlElement3.AppendChild(xmlElement4);
		}
		xmlElement.AppendChild(xmlElement3);
		xmlDocument.AppendChild(xmlElement);
		return QuickSaver.GetCompressedBytesFromXmlDoc(xmlDocument);
	}
}
