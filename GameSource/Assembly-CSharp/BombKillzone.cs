using System.Collections.Generic;
using UnityEngine;

public class BombKillzone : MonoBehaviour
{
	public List<GameObject> InBlastZone = new List<GameObject>();

	private BoxCollider2D killzoneCollider;

	public bool UseCustomTint;

	public Color TintColor;

	public bool DetectAttachments;

	private void Start()
	{
		if (!UseCustomTint)
		{
			TintColor = GameSettings.GetInstance().negativeColor;
		}
		killzoneCollider = GetComponent<BoxCollider2D>();
	}

	private void addBlock(Placeable p)
	{
		if (!(p != null) || p.PickedUp || p.inDestructible || p.Protected || !p.InteractableInCurrentMode || InBlastZone.Contains(p.gameObject))
		{
			return;
		}
		InBlastZone.Add(p.gameObject);
		if (GameSettings.GetInstance() != null)
		{
			if (!UseCustomTint || p.canSetCustomColor)
			{
				p.AddBombTint(TintColor);
			}
			p.Tint();
		}
	}

	private void removeBlock(Placeable p)
	{
		if (p != null && InBlastZone.Contains(p.gameObject))
		{
			InBlastZone.Remove(p.gameObject);
			p.RemoveBombTint();
			p.Tint();
		}
	}

	private void HandleCollision(Collider2D c, bool add)
	{
		if (!DetectAttachments)
		{
			CollisionTag component = c.GetComponent<CollisionTag>();
			if (component != null && component.ContainsAnyTag((TagComparer.Tag)16809984))
			{
				return;
			}
		}
		Placeable componentInParent = c.GetComponentInParent<Placeable>();
		if (add)
		{
			addBlock(componentInParent);
		}
		else
		{
			removeBlock(componentInParent);
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		HandleCollision(c, add: true);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		HandleCollision(c, add: true);
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		HandleCollision(c, add: false);
	}

	public void Reset()
	{
		foreach (GameObject item in InBlastZone)
		{
			if (!(item == null))
			{
				Placeable componentInParent = item.GetComponentInParent<Placeable>();
				if (componentInParent != null)
				{
					componentInParent.RemoveBombTint();
					componentInParent.Tint();
				}
			}
		}
		InBlastZone.Clear();
	}

	public void RecalculateBlocks()
	{
		Reset();
		Physics2D.queriesHitTriggers = true;
		RaycastHit2D[] raycastResultCache = Placeable.raycastResultCache;
		int num = Physics2D.BoxCastNonAlloc((Vector2)base.transform.position + killzoneCollider.offset, killzoneCollider.size, 0f, Vector2.zero, raycastResultCache);
		for (int i = 0; i != num; i++)
		{
			RaycastHit2D raycastHit2D = raycastResultCache[i];
			Placeable componentInParent = raycastHit2D.transform.GetComponentInParent<Placeable>();
			if (componentInParent != null)
			{
				addBlock(componentInParent);
			}
		}
	}

	private void OnDestroy()
	{
		Reset();
	}
}
