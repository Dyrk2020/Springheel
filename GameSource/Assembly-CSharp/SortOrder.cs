using UnityEngine;

public class SortOrder
{
	public class SpriteInfo
	{
		public SpriteRenderer sprite;

		protected int relativeOrder;

		public SpriteInfo(SpriteRenderer sr, int num)
		{
			sprite = sr;
			relativeOrder = num;
		}

		public void newSortOrder(int orderNumber)
		{
			sprite.sortingOrder = orderNumber + relativeOrder;
		}
	}

	protected SpriteInfo[] spriteInfos;

	protected ParticleSystemRenderer particles;

	protected int particlesLayerInt;

	public int currentBaseOrder;

	public SortOrder(GameObject go, bool limitToArtSprites = false)
	{
		Intitialize(go, limitToArtSprites);
	}

	public void Intitialize(GameObject go, bool limitToArtSprites = false)
	{
		SpriteRenderer[] array;
		if (limitToArtSprites)
		{
			Placeable component = go.GetComponent<Placeable>();
			if (component != null)
			{
				array = component.ArtSprites;
			}
			else
			{
				array = go.GetComponentsInChildren<SpriteRenderer>();
				Debug.Log("Something went wrong with SpriteOrder system");
			}
		}
		else
		{
			array = go.GetComponentsInChildren<SpriteRenderer>();
		}
		particles = go.GetComponentInChildren<ParticleSystemRenderer>();
		if (particles != null)
		{
			particlesLayerInt = particles.sortingOrder;
		}
		currentBaseOrder = 0;
		if (array.Length != 0)
		{
			spriteInfos = new SpriteInfo[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				SpriteRenderer spriteRenderer = array[i];
				spriteInfos[i] = new SpriteInfo(spriteRenderer, spriteRenderer.sortingOrder);
			}
		}
	}

	public void setSortOrder(int orderNumber)
	{
		currentBaseOrder = orderNumber;
		if (spriteInfos != null && spriteInfos.Length != 0)
		{
			SpriteInfo[] array = spriteInfos;
			foreach (SpriteInfo spriteInfo in array)
			{
				if (spriteInfo != null && spriteInfo.sprite != null)
				{
					spriteInfo.newSortOrder(orderNumber);
				}
			}
		}
		if (particles != null)
		{
			particles.sortingOrder = particlesLayerInt + orderNumber;
		}
	}
}
