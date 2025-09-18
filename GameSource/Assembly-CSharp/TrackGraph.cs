using System.Collections.Generic;
using UnityEngine;

public class TrackGraph : MonoBehaviour
{
	public enum trackValue
	{
		off,
		xAxis,
		yAxis,
		xSpeed,
		ySpeed,
		xAccel,
		yAccel,
		LeftRight,
		UpDown
	}

	public bool averageMode;

	public List<GameObject> trackedObjects;

	public int selector;

	public trackValue trackedValueType;

	public Character.Animals whoIs = Character.Animals.CHICKEN;

	private Character.Animals lastWhoIs;

	public int graphPoints;

	public float lineWidth = 0.5f;

	public Material lineMaterial;

	public Color lineColor;

	public float scaleY;

	public float offsetY;

	private Vector3 center;

	private Vector3 size;

	public bool ControlGraph;

	public Vector2 GraphMove;

	public Vector2 GraphScale;

	private float lastValue;

	private float[] data;

	private float average;

	private bool isCharacter;

	private Character character;

	private LineRenderer lineRenderer;

	private GameObject UICamera;

	public SpriteRenderer background;

	private Vector3 lastSpeed;

	private Vector3 currentSpeed;

	private Vector3 lastPosition;

	private void Start()
	{
		GameObject gameObject;
		if (!(gameObject = GameObject.Find("graphHolder")))
		{
			gameObject = new GameObject("graphHolder");
		}
		GameObject gameObject2 = new GameObject();
		UICamera = GameObject.Find("UiCamera");
		if ((bool)UICamera)
		{
			gameObject.transform.parent = UICamera.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.localPosition = new Vector3(0f, 0f, gameObject.transform.localPosition.z - 0.01f);
		}
		background.transform.parent = gameObject.transform;
		gameObject2.transform.parent = gameObject.transform;
		background.transform.localPosition = Vector3.zero;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.layer = LayerMask.NameToLayer("UI");
		gameObject2.transform.localPosition = Vector3.zero;
		lineRenderer = gameObject2.AddComponent<LineRenderer>();
		lineRenderer.positionCount = graphPoints;
		data = new float[graphPoints];
		lineRenderer.useWorldSpace = false;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
		lineRenderer.material = lineMaterial;
		lineRenderer.startColor = lineColor;
		lineRenderer.endColor = lineColor;
		Character[] array = Object.FindObjectsOfType<Character>();
		for (int i = 0; i < array.Length; i++)
		{
			trackedObjects.Add(array[i].gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (ControlGraph)
		{
			background.transform.localPosition = new Vector3(GraphMove.x, GraphMove.y, 0f);
			background.transform.localScale = GraphScale;
		}
		center = background.transform.localPosition;
		size = background.bounds.extents;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
		if (trackedValueType == trackValue.off)
		{
			lineRenderer.enabled = false;
			return;
		}
		lineRenderer.enabled = true;
		if (whoIs != lastWhoIs)
		{
			isCharacter = false;
			character = null;
			for (int i = 0; i < trackedObjects.Count; i++)
			{
				if (trackedObjects[i].GetComponent<Character>().CharacterSprite == whoIs)
				{
					selector = i;
					character = trackedObjects[i].GetComponent<Character>();
					isCharacter = true;
				}
			}
			lastWhoIs = whoIs;
		}
		average = 0f;
		for (int j = 0; j < graphPoints - 1; j++)
		{
			average += data[j];
			data[j] = data[j + 1];
		}
		float num;
		switch (trackedValueType)
		{
		case trackValue.yAxis:
			num = trackedObjects[selector].transform.position.y;
			break;
		case trackValue.xAxis:
			num = trackedObjects[selector].transform.position.x;
			break;
		case trackValue.xSpeed:
			num = lastValue - trackedObjects[selector].transform.position.x;
			lastValue = trackedObjects[selector].transform.position.x;
			break;
		case trackValue.ySpeed:
			num = 0f - (lastValue - trackedObjects[selector].transform.position.y);
			lastValue = trackedObjects[selector].transform.position.y;
			break;
		case trackValue.xAccel:
			currentSpeed.x = lastPosition.x - trackedObjects[selector].transform.position.x;
			num = lastSpeed.x - currentSpeed.x;
			lastSpeed.x = currentSpeed.x;
			lastPosition.x = trackedObjects[selector].transform.position.x;
			break;
		case trackValue.yAccel:
			currentSpeed.y = lastPosition.y - trackedObjects[selector].transform.position.y;
			num = lastSpeed.y - currentSpeed.y;
			lastSpeed.y = currentSpeed.y;
			lastPosition.y = trackedObjects[selector].transform.position.y;
			break;
		case trackValue.LeftRight:
			num = ((!isCharacter) ? 0f : ((!(character.left > 0f)) ? ((!(character.right > 0f)) ? 0f : character.right) : (0f - character.left)));
			break;
		case trackValue.UpDown:
			num = ((!isCharacter) ? 0f : ((!(character.up > 0f)) ? ((!(character.down > 0f)) ? 0f : (0f - character.down)) : character.up));
			break;
		default:
			num = 0f;
			break;
		}
		data[graphPoints - 1] = num;
		average /= graphPoints;
		for (int k = 0; k < graphPoints; k++)
		{
			float x = Mathf.Lerp(center.x - size.x, center.x + size.x, (float)k / (float)data.Length);
			float value = (averageMode ? ((data[k] - average) * scaleY + offsetY) : (data[k] * scaleY + offsetY));
			float z = 0f;
			float y = Mathf.Clamp(value, center.y - size.y, center.y + size.y);
			lineRenderer.SetPosition(k, new Vector3(x, y, z));
		}
	}
}
