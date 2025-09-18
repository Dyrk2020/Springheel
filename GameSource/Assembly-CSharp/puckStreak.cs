using System.Collections.Generic;
using UnityEngine;

public class puckStreak : MonoBehaviour
{
	public Vector3 moveDirection;

	public float puckWidth = 6f;

	public float speed = 10f;

	public float minStreakTime;

	public float maxStreakTime;

	public float spawnStartTime;

	public float maxStreakLength;

	protected float timeSinceSpawn;

	protected float streakTime;

	public puckSprite puckSprite;

	public List<Vector3> linePoints;

	public int lineSegments = 5;

	private LineRenderer lineRenderer;

	private Vector3 startpointA;

	private Vector3 endpointA;

	protected int linePointNumber = 3;

	protected bool updatedStarted;

	private void Update()
	{
		if (!updatedStarted)
		{
			return;
		}
		timeSinceSpawn += Time.deltaTime;
		if (puckSprite != null && linePoints[linePoints.Count - 1] != puckSprite.gameObject.transform.position)
		{
			linePoints.Add(puckSprite.gameObject.transform.position);
		}
		if (timeSinceSpawn < streakTime)
		{
			for (int i = 0; i < lineSegments; i++)
			{
				Vector3 value = Vector3.Lerp(startpointA, endpointA, (timeSinceSpawn + (streakTime - timeSinceSpawn) / (float)lineSegments * (float)i) / streakTime);
				linePoints[i] = value;
			}
		}
		else
		{
			linePoints.RemoveAt(0);
			linePoints.RemoveAt(0);
			if (linePoints.Count <= 4)
			{
				Object.Destroy(base.gameObject);
			}
		}
		DrawLine();
	}

	public void startPuck(Vector3 startpoint, Vector3 endPoint, int sortingLayerID, int sortingOrder)
	{
		streakTime = Mathf.Lerp(minStreakTime, maxStreakTime, (endPoint - startpoint).magnitude / maxStreakLength);
		for (int i = 0; i < lineSegments; i++)
		{
			Vector3 item = Vector3.Lerp(startpoint, endPoint, (timeSinceSpawn + (streakTime - timeSinceSpawn) / (float)lineSegments * (float)i) / streakTime);
			linePoints.Add(item);
		}
		linePoints.Add(Vector3.Lerp(startpoint, endPoint, 0.999f));
		linePoints.Add(endPoint);
		startpointA = startpoint;
		endpointA = endPoint;
		timeSinceSpawn = 0f;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.sortingLayerID = sortingLayerID;
		lineRenderer.sortingOrder = sortingOrder;
		DrawLine();
		updatedStarted = true;
	}

	private void DrawLine()
	{
		lineRenderer.positionCount = linePoints.Count;
		for (int i = 0; i < linePoints.Count; i++)
		{
			lineRenderer.SetPosition(i, linePoints[i]);
		}
	}
}
