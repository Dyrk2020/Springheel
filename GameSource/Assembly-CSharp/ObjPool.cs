using System.Collections.Generic;
using UnityEngine;

public class ObjPool : MonoBehaviour
{
	public Queue<GameObject> poolQueue = new Queue<GameObject>();

	public static GameObject poolHolder;

	private GameObject pooledObjectPrefab;

	private int poolSize;

	public void Initilize(GameObject pooledObjectPrefab, int poolSize)
	{
		if (poolHolder == null)
		{
			poolHolder = new GameObject("PoolHolder");
		}
		this.pooledObjectPrefab = pooledObjectPrefab;
		IncreasePoolSize(poolSize);
	}

	private void IncreasePoolSize(int numObjects)
	{
		poolSize += numObjects;
		for (int i = 0; i < numObjects; i++)
		{
			GameObject obj = Object.Instantiate(pooledObjectPrefab);
			AddObjToPool(obj);
		}
	}

	public GameObject GetObjFromPool()
	{
		if (poolQueue.Count > 0)
		{
			return poolQueue.Dequeue();
		}
		Debug.LogError("Pool Queue overrun! Doubling pool size...");
		IncreasePoolSize(poolSize);
		return poolQueue.Dequeue();
	}

	public void AddObjToPool(GameObject obj)
	{
		Projectile component = obj.GetComponent<Projectile>();
		if (component != null)
		{
			component.srcPool = this;
			component.Reset();
		}
		obj.SetActive(value: false);
		if (!poolQueue.Contains(obj))
		{
			poolQueue.Enqueue(obj);
		}
		else
		{
			Debug.LogWarning("Pool already contained this object");
		}
		obj.transform.parent = poolHolder.transform;
	}

	private void OnDestroy()
	{
		while (poolQueue.Count > 0)
		{
			GameObject objFromPool = GetObjFromPool();
			if (objFromPool != null)
			{
				Object.Destroy(objFromPool);
			}
		}
	}
}
