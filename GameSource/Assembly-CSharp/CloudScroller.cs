using System.Collections.Generic;
using UnityEngine;

public class CloudScroller : MonoBehaviour
{
	public float cloudScrollSpeed;

	public float randomSpeedRange;

	public GameObject CloudStart;

	public GameObject CloudEnd;

	public GameObject cloudHolder;

	private cloud[] clouds;

	private List<cloud> dynamicClouds = new List<cloud>();

	protected bool cloudsEnabled = true;

	private void Start()
	{
		Transform[] componentsInChildren = cloudHolder.GetComponentsInChildren<Transform>();
		clouds = new cloud[componentsInChildren.Length - 1];
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			clouds[i - 1]._transform = componentsInChildren[i];
			clouds[i - 1].randomSpeed = Random.Range(0f, randomSpeedRange) * 0.01f + cloudScrollSpeed * 0.01f;
		}
	}

	private void Update()
	{
		if (GameState.DebugMode && Input.GetKeyDown(KeyCode.C))
		{
			cloudsEnabled = !cloudsEnabled;
		}
		if (cloudsEnabled)
		{
			ScrollBackgroundClouds();
			ScrollDynamicClouds();
		}
	}

	private void ScrollDynamicClouds()
	{
		for (int num = dynamicClouds.Count - 1; num >= 0; num--)
		{
			ScrollDynamicCloudAtIndex(num);
		}
	}

	private void ScrollDynamicCloudAtIndex(int index)
	{
		cloud c = dynamicClouds[index];
		if (c._transform == null || c._transform.parent != cloudHolder.transform)
		{
			dynamicClouds.RemoveAt(index);
		}
		else
		{
			ScrollCloud(c);
		}
	}

	private void ScrollBackgroundClouds()
	{
		cloud[] array = clouds;
		foreach (cloud c in array)
		{
			ScrollCloud(c);
		}
	}

	private void ScrollCloud(cloud c)
	{
		c._transform.Translate(c.randomSpeed * Time.deltaTime, 0f, 0f, Space.World);
		if (c._transform.position.x > CloudEnd.transform.position.x)
		{
			c._transform.position = new Vector3(CloudStart.transform.position.x, c._transform.position.y, c._transform.position.z);
		}
		if (c._transform.position.x < CloudStart.transform.position.x)
		{
			c._transform.position = new Vector3(CloudEnd.transform.position.x, c._transform.position.y, c._transform.position.z);
		}
	}

	public void RegisterCloud(Transform cloudTransform)
	{
		if (cloudTransform == null)
		{
			return;
		}
		foreach (cloud dynamicCloud in dynamicClouds)
		{
			if (dynamicCloud._transform == cloudTransform)
			{
				return;
			}
		}
		cloud item = new cloud
		{
			_transform = cloudTransform,
			randomSpeed = Random.Range(0f, randomSpeedRange) * 0.01f + cloudScrollSpeed * 0.01f
		};
		dynamicClouds.Add(item);
	}

	public void UnregisterCloud(Transform cloudTransform)
	{
		if (!(cloudTransform == null))
		{
			dynamicClouds.RemoveAll((cloud c) => c._transform == cloudTransform);
		}
	}
}
