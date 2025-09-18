using System.Collections.Generic;
using UnityEngine;

public class WaterPool : MonoBehaviour
{
	private const string PLAYER_TAG = "Solid_Player";

	private static readonly string[] PLAYER_LAYER = new string[2] { "Player", "NonLocalPlayer" };

	private static readonly string[] HAZARD_LAYER = new string[1] { "PlayerOnlyPhysics" };

	public WaterSplash SplashPrefab;

	private int poolSize = 10;

	private List<GameObject> splashingObjects;

	private Dictionary<WaterSplash, GameObject> splashPool;

	private List<Collider2D> collidersInTrigger = new List<Collider2D>();

	private BoxCollider2D boxCollider;

	private List<Collider2D> oldObjectsInTrigger = new List<Collider2D>();

	private void Start()
	{
		boxCollider = GetComponent<BoxCollider2D>();
		splashPool = new Dictionary<WaterSplash, GameObject>();
		for (int i = 0; i != poolSize; i++)
		{
			splashPool.Add(Object.Instantiate(SplashPrefab), null);
		}
		splashingObjects = new List<GameObject>();
	}

	private void Update()
	{
		WaterSplash[] array = new WaterSplash[splashPool.Count];
		splashPool.Keys.CopyTo(array, 0);
		WaterSplash[] array2 = array;
		foreach (WaterSplash waterSplash in array2)
		{
			if (!waterSplash.Splashing)
			{
				removeSplash(waterSplash);
			}
		}
	}

	public void OnCollisionEnter2D(Collision2D c)
	{
		createSplash(c.gameObject, c.contacts[0].point);
	}

	public void OnTriggerEnter2D(Collider2D c)
	{
	}

	private void CheckForSplash(Collider2D c)
	{
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		int num = Physics2D.RaycastNonAlloc(c.transform.position, Vector2.down, Placeable.raycastResultCache, float.MaxValue, LayerMask.GetMask(HAZARD_LAYER));
		for (int i = 0; i != num; i++)
		{
			RaycastHit2D raycastHit2D = Placeable.raycastResultCache[i];
			if (raycastHit2D.collider == boxCollider)
			{
				createSplash(c.gameObject, raycastHit2D.point);
				return;
			}
		}
		Physics2D.queriesHitTriggers = queriesHitTriggers;
	}

	private void FixedUpdate()
	{
		UpdateCollidersInTrigger();
	}

	private void UpdateCollidersInTrigger()
	{
		oldObjectsInTrigger.Clear();
		oldObjectsInTrigger.AddRange(collidersInTrigger);
		collidersInTrigger.Clear();
		Bounds bounds = boxCollider.CalculateScaledBounds();
		Collider2D[] array = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f, LayerMask.GetMask(PLAYER_LAYER));
		foreach (Collider2D collider2D in array)
		{
			if (collider2D.CompareTag("Solid_Player"))
			{
				collidersInTrigger.Add(collider2D);
			}
		}
		foreach (Collider2D item in collidersInTrigger)
		{
			if (!oldObjectsInTrigger.Contains(item))
			{
				CheckForSplash(item);
			}
		}
	}

	private void removeSplash(WaterSplash splash)
	{
		GameObject gameObject = splashPool[splash];
		if (gameObject != null)
		{
			if (splashingObjects.Contains(gameObject))
			{
				splashingObjects.Remove(gameObject);
			}
			splashPool[splash] = null;
		}
	}

	private void createSplash(GameObject obj)
	{
		createSplash(obj, obj.transform.position);
	}

	private void createSplash(GameObject obj, Vector3 position)
	{
		GameObject topParent = getTopParent(obj);
		if (splashingObjects.Contains(topParent))
		{
			return;
		}
		splashingObjects.Add(topParent);
		WaterSplash waterSplash = null;
		WaterSplash[] array = new WaterSplash[splashPool.Count];
		splashPool.Keys.CopyTo(array, 0);
		WaterSplash[] array2 = array;
		foreach (WaterSplash waterSplash2 in array2)
		{
			if (!waterSplash2.Splashing)
			{
				removeSplash(waterSplash2);
				waterSplash = waterSplash2;
				break;
			}
		}
		if (waterSplash != null)
		{
			waterSplash.transform.position = position;
			waterSplash.Splash();
			splashPool[waterSplash] = topParent;
		}
	}

	private GameObject getTopParent(GameObject obj)
	{
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
		}
		return obj;
	}
}
