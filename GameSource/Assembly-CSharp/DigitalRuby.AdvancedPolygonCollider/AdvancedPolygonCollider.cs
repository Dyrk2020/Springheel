using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalRuby.AdvancedPolygonCollider;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class AdvancedPolygonCollider : MonoBehaviour
{
	[Serializable]
	public struct ArrayWrapper
	{
		[SerializeField]
		public Vector2[] Array;
	}

	[Serializable]
	public struct ListWrapper
	{
		[SerializeField]
		public List<ArrayWrapper> List;
	}

	[Serializable]
	public struct CacheEntry
	{
		[SerializeField]
		public CacheKey Key;

		[SerializeField]
		public ListWrapper Value;
	}

	[Serializable]
	public struct CacheKey
	{
		[SerializeField]
		public Texture2D Texture;

		[SerializeField]
		public Rect Rect;

		public override int GetHashCode()
		{
			return Texture.GetHashCode() * Rect.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is CacheKey cacheKey)
			{
				if (Texture == cacheKey.Texture)
				{
					return Rect == cacheKey.Rect;
				}
				return false;
			}
			return false;
		}
	}

	[Tooltip("Pixels with alpha greater than this count as solid.")]
	[Range(0f, 254f)]
	public byte AlphaTolerance = 20;

	[Tooltip("Points further away than this number of pixels will be consolidated.")]
	[Range(0f, 64f)]
	public int DistanceThreshold = 8;

	[Range(0.5f, 2f)]
	[Tooltip("Scale of the polygon.")]
	public float Scale = 1f;

	[Tooltip("Whether to decompse vertices into convex only polygons.")]
	public bool Decompose;

	[Tooltip("Whether or not create a collider only out of the biggest polygon.")]
	public bool BiggestOnly;

	[Tooltip("Whether to live update everything when in play mode. Typically for performance this can be false, but if you plan on making changes to the sprite or parameters at runtime, you will want to set this to true.")]
	public bool RunInPlayMode;

	[Tooltip("True to use the cache, false otherwise. The cache is populated in editor and play mode and uses the most recent geometry for a texture and rect regardless of other parameters. When ignoring the cache, values will not be added to the cache either. Cache is only useful if you will be changing your sprite at run-time (i.e. animation)")]
	public bool UseCache;

	private SpriteRenderer spriteRenderer;

	private PolygonCollider2D polygonCollider;

	private bool dirty;

	[HideInInspector]
	[SerializeField]
	private byte lastAlphaTolerance;

	[HideInInspector]
	[SerializeField]
	private float lastScale;

	[SerializeField]
	[HideInInspector]
	private int lastDistanceThreshold;

	[SerializeField]
	[HideInInspector]
	private bool lastDecompose;

	[SerializeField]
	[HideInInspector]
	private Sprite lastSprite;

	[HideInInspector]
	[SerializeField]
	private Rect lastRect;

	[HideInInspector]
	[SerializeField]
	private Vector2 lastOffset = new Vector2(-99999f, -99999f);

	[HideInInspector]
	[SerializeField]
	private float lastPixelsPerUnit;

	[HideInInspector]
	[SerializeField]
	private bool lastFlipX;

	[HideInInspector]
	[SerializeField]
	private bool lastFlipY;

	private static readonly Dictionary<CacheKey, List<Vector2[]>> cache = new Dictionary<CacheKey, List<Vector2[]>>();

	[Tooltip("All the cached objects from the editor. Do not modify this data.")]
	[SerializeField]
	private List<CacheEntry> editorCache = new List<CacheEntry>();

	private readonly TextureConverter geometryDetector = new TextureConverter();

	public int VerticesCount
	{
		get
		{
			if (!(polygonCollider == null))
			{
				return polygonCollider.GetTotalPointCount();
			}
			return 0;
		}
	}

	private void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (CacheEntry item in editorCache)
		{
			List<Vector2[]> list = new List<Vector2[]>();
			cache[item.Key] = list;
			foreach (ArrayWrapper item2 in item.Value.List)
			{
				list.Add(item2.Array);
			}
		}
	}

	private void Start()
	{
		polygonCollider = GetComponent<PolygonCollider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void UpdateDirtyState()
	{
		if (spriteRenderer.sprite != lastSprite)
		{
			lastSprite = spriteRenderer.sprite;
			dirty = true;
		}
		if (spriteRenderer.sprite != null)
		{
			if (lastOffset != spriteRenderer.sprite.pivot)
			{
				lastOffset = spriteRenderer.sprite.pivot;
				dirty = true;
			}
			if (lastRect != spriteRenderer.sprite.rect)
			{
				lastRect = spriteRenderer.sprite.rect;
				dirty = true;
			}
			if (lastPixelsPerUnit != spriteRenderer.sprite.pixelsPerUnit)
			{
				lastPixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
				dirty = true;
			}
			if (lastFlipX != spriteRenderer.flipX)
			{
				lastFlipX = spriteRenderer.flipX;
				dirty = true;
			}
			if (lastFlipY != spriteRenderer.flipY)
			{
				lastFlipY = spriteRenderer.flipY;
				dirty = true;
			}
		}
		if (AlphaTolerance != lastAlphaTolerance)
		{
			lastAlphaTolerance = AlphaTolerance;
			dirty = true;
		}
		if (Scale != lastScale)
		{
			lastScale = Scale;
			dirty = true;
		}
		if (DistanceThreshold != lastDistanceThreshold)
		{
			lastDistanceThreshold = DistanceThreshold;
			dirty = true;
		}
		if (Decompose != lastDecompose)
		{
			lastDecompose = Decompose;
			dirty = true;
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (!RunInPlayMode)
			{
				return;
			}
		}
		else if (!UseCache)
		{
			editorCache.Clear();
		}
		UpdateDirtyState();
		if (dirty)
		{
			RecalculatePolygon();
		}
	}

	public void RecalculatePolygon()
	{
		if (spriteRenderer.sprite != null)
		{
			PolygonParameters p = new PolygonParameters
			{
				AlphaTolerance = AlphaTolerance,
				Decompose = Decompose,
				DistanceThreshold = DistanceThreshold,
				Rect = spriteRenderer.sprite.rect,
				Offset = spriteRenderer.sprite.pivot,
				Texture = spriteRenderer.sprite.texture,
				XMultiplier = spriteRenderer.sprite.rect.width * 0.5f / spriteRenderer.sprite.pixelsPerUnit,
				YMultiplier = spriteRenderer.sprite.rect.height * 0.5f / spriteRenderer.sprite.pixelsPerUnit,
				UseCache = UseCache
			};
			UpdatePolygonCollider(ref p);
		}
	}

	public void UpdatePolygonCollider(ref PolygonParameters p)
	{
		if (spriteRenderer.sprite == null || spriteRenderer.sprite.texture == null)
		{
			return;
		}
		dirty = false;
		if (Application.isPlaying && p.UseCache)
		{
			CacheKey key = new CacheKey
			{
				Texture = p.Texture,
				Rect = p.Rect
			};
			if (cache.TryGetValue(key, out var value))
			{
				polygonCollider.pathCount = value.Count;
				for (int i = 0; i < value.Count; i++)
				{
					polygonCollider.SetPath(i, value[i]);
				}
			}
			else
			{
				Debug.LogWarning("No cached data found. Not generating new collider " + spriteRenderer.sprite.name);
			}
		}
		else
		{
			PopulateCollider(polygonCollider, ref p);
		}
	}

	public void PopulateCollider(PolygonCollider2D collider, ref PolygonParameters p)
	{
		try
		{
			if (p.Texture.format != TextureFormat.ARGB32 && p.Texture.format != TextureFormat.BGRA32 && p.Texture.format != TextureFormat.RGBA32 && p.Texture.format != TextureFormat.RGB24 && p.Texture.format != TextureFormat.Alpha8 && p.Texture.format != TextureFormat.RGBAFloat && p.Texture.format != TextureFormat.RGBAHalf && p.Texture.format != TextureFormat.RGB565)
			{
				Debug.LogWarning("Advanced Polygon Collider works best with a non-compressed texture in ARGB32, BGRA32, RGB24, RGBA4444, RGB565, RGBAFloat or RGBAHalf format");
			}
			int num = (int)p.Rect.width;
			int blockHeight = (int)p.Rect.height;
			int x = (int)p.Rect.x;
			int y = (int)p.Rect.y;
			Color[] pixels = p.Texture.GetPixels(x, y, num, blockHeight, 0);
			List<Vertices> list = geometryDetector.DetectVertices(pixels, num, p.AlphaTolerance);
			if (BiggestOnly)
			{
				list = new List<Vertices> { list.OrderByDescending((Vertices v) => v.Count).First() };
			}
			int pathIndex = 0;
			List<Vector2[]> list2 = new List<Vector2[]>();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				ProcessVertices(collider, list[num2], list2, ref p, ref pathIndex);
			}
			if (p.UseCache)
			{
				CacheKey key = new CacheKey
				{
					Texture = p.Texture,
					Rect = p.Rect
				};
				cache[key] = list2;
			}
			Debug.Log("Updated polygon.");
		}
		catch (Exception ex)
		{
			Debug.LogError("Error creating collider: " + ex);
		}
	}

	private List<Vector2[]> ProcessVertices(PolygonCollider2D collider, Vertices v, List<Vector2[]> list, ref PolygonParameters p, ref int pathIndex)
	{
		Vector2 offset = p.Offset;
		float num = (spriteRenderer.flipX ? (-1f) : 1f);
		float num2 = (spriteRenderer.flipY ? (-1f) : 1f);
		if (p.DistanceThreshold > 1)
		{
			v = SimplifyTools.DouglasPeuckerSimplify(v, p.DistanceThreshold);
		}
		if (p.Decompose)
		{
			List<List<Vector2>> list2 = BayazitDecomposer.ConvexPartition(v);
			for (int i = 0; i < list2.Count; i++)
			{
				List<Vector2> list3 = list2[i];
				for (int j = 0; j < list3.Count; j++)
				{
					float num3 = 2f * ((list3[j].x - offset.x + 0.5f) / p.Rect.width);
					float num4 = 2f * ((list3[j].y - offset.y + 0.5f) / p.Rect.height);
					list3[j] = new Vector2(num3 * p.XMultiplier * Scale * num, num4 * p.YMultiplier * Scale * num2);
				}
				Vector2[] array = list3.ToArray();
				collider.pathCount = pathIndex + 1;
				collider.SetPath(pathIndex++, array);
				list.Add(array);
			}
		}
		else
		{
			collider.pathCount = pathIndex + 1;
			for (int k = 0; k < v.Count; k++)
			{
				float num5 = 2f * ((v[k].x - offset.x + 0.5f) / p.Rect.width);
				float num6 = 2f * ((v[k].y - offset.y + 0.5f) / p.Rect.height);
				v[k] = new Vector2(num5 * p.XMultiplier * Scale * num, num6 * p.YMultiplier * Scale * num2);
			}
			Vector2[] array2 = v.ToArray();
			collider.SetPath(pathIndex++, array2);
			list.Add(array2);
		}
		return list;
	}
}
