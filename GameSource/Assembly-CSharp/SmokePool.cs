using System.Collections.Generic;
using UnityEngine;

public class SmokePool : MonoBehaviour
{
	public enum SmokeType
	{
		POOF,
		JUMP,
		WALL_L,
		WALL_R,
		LAND,
		UIPOOF,
		MULTIJUMP,
		ANIMALCANNON_SMOKE,
		ANIMLACANNON_BLUR
	}

	public SmokeEffect JumpSmokePrefab;

	public int JumpPoolSize = 16;

	public SmokeEffect WallLeftPrefab;

	public int WallLeftSize = 32;

	public SmokeEffect WallRightPrefab;

	public int WallRightSize = 32;

	public SmokeEffect LandDustPrefab;

	public int LandPoolSize = 16;

	public SmokeEffect PoofPrefab;

	public int SmokePoolSize = 32;

	public SmokeEffect UISmokePrefab;

	public int UISmokePoolSize = 32;

	public SmokeEffect MultiJumpSmokePrefab;

	public int MultiJumpPoolSize = 16;

	public SmokeEffect animalCannonCharacterSmokePrefab;

	public int animalCannonCharacterSmokePoolSize = 8;

	public SmokeEffect animalCannonCharacterBlurPrefab;

	public int animalCannonCharacterBlurPoolSize = 8;

	private Queue<SmokeEffect> jumpPool;

	private Queue<SmokeEffect> wallLeftPool;

	private Queue<SmokeEffect> wallRightPool;

	private Queue<SmokeEffect> landPool;

	private Queue<SmokeEffect> poofPool;

	private Queue<SmokeEffect> uiSmokePool;

	private Queue<SmokeEffect> multiJumpSmokePool;

	private Queue<SmokeEffect> animalCannonCharacterSmokePool;

	private Queue<SmokeEffect> animalCannonCharacterBlurPool;

	private bool initialized;

	private static SmokePool instance;

	public static SmokePool Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("SmokePool", typeof(SmokePool)).GetComponent<SmokePool>();
				instance.Initialize();
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else if (instance == null)
		{
			instance = this;
			Initialize();
		}
	}

	public void Initialize()
	{
		if (!initialized)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			jumpPool = new Queue<SmokeEffect>();
			for (int i = 0; i != JumpPoolSize; i++)
			{
				jumpPool.Enqueue(Object.Instantiate(JumpSmokePrefab, base.transform));
			}
			landPool = new Queue<SmokeEffect>();
			for (int j = 0; j != JumpPoolSize; j++)
			{
				landPool.Enqueue(Object.Instantiate(LandDustPrefab, base.transform));
			}
			poofPool = new Queue<SmokeEffect>();
			for (int k = 0; k != JumpPoolSize; k++)
			{
				poofPool.Enqueue(Object.Instantiate(PoofPrefab, base.transform));
			}
			uiSmokePool = new Queue<SmokeEffect>();
			for (int l = 0; l != JumpPoolSize; l++)
			{
				uiSmokePool.Enqueue(Object.Instantiate(UISmokePrefab, base.transform));
			}
			wallLeftPool = new Queue<SmokeEffect>();
			for (int m = 0; m != JumpPoolSize; m++)
			{
				wallLeftPool.Enqueue(Object.Instantiate(WallLeftPrefab, base.transform));
			}
			wallRightPool = new Queue<SmokeEffect>();
			for (int n = 0; n != JumpPoolSize; n++)
			{
				wallRightPool.Enqueue(Object.Instantiate(WallRightPrefab, base.transform));
			}
			multiJumpSmokePool = new Queue<SmokeEffect>();
			for (int num = 0; num != MultiJumpPoolSize; num++)
			{
				multiJumpSmokePool.Enqueue(Object.Instantiate(MultiJumpSmokePrefab, base.transform));
			}
			animalCannonCharacterSmokePool = new Queue<SmokeEffect>();
			for (int num2 = 0; num2 != animalCannonCharacterSmokePoolSize; num2++)
			{
				animalCannonCharacterSmokePool.Enqueue(Object.Instantiate(animalCannonCharacterSmokePrefab, base.transform));
			}
			animalCannonCharacterBlurPool = new Queue<SmokeEffect>();
			for (int num3 = 0; num3 != animalCannonCharacterBlurPoolSize; num3++)
			{
				animalCannonCharacterBlurPool.Enqueue(Object.Instantiate(animalCannonCharacterBlurPrefab, base.transform));
			}
			initialized = true;
		}
	}

	public SmokeEffect SpawnSmoke(SmokeType type, Vector3 position)
	{
		return SpawnSmoke(type, position, 1f, Color.white, -1);
	}

	public SmokeEffect SpawnSmoke(SmokeType type, Vector3 position, float scale)
	{
		return SpawnSmoke(type, position, scale, Color.white, -1);
	}

	public SmokeEffect SpawnSmoke(SmokeType type, Vector3 position, float scale, Color tint)
	{
		return SpawnSmoke(type, position, scale, tint, -1);
	}

	public SmokeEffect SpawnSmoke(SmokeType type, Vector3 position, float scale, Color tint, string layerName)
	{
		return SpawnSmoke(type, position, scale, tint, LayerMask.NameToLayer(layerName));
	}

	public SmokeEffect SpawnSmoke(SmokeType type, Vector3 position, float scale, Color tint, int layer)
	{
		Queue<SmokeEffect> queue;
		switch (type)
		{
		case SmokeType.POOF:
			queue = poofPool;
			break;
		case SmokeType.JUMP:
			queue = jumpPool;
			break;
		case SmokeType.LAND:
			queue = landPool;
			break;
		case SmokeType.UIPOOF:
			queue = uiSmokePool;
			break;
		case SmokeType.WALL_L:
			queue = wallLeftPool;
			break;
		case SmokeType.WALL_R:
			queue = wallRightPool;
			break;
		case SmokeType.MULTIJUMP:
			queue = multiJumpSmokePool;
			break;
		case SmokeType.ANIMALCANNON_SMOKE:
			queue = animalCannonCharacterSmokePool;
			break;
		case SmokeType.ANIMLACANNON_BLUR:
			queue = animalCannonCharacterBlurPool;
			break;
		default:
			return null;
		}
		SmokeEffect smokeEffect = queue.Dequeue();
		if (type == SmokeType.MULTIJUMP)
		{
			smokeEffect.transform.position = position + smokeEffect.PositionOffset;
		}
		else
		{
			smokeEffect.transform.position = position + smokeEffect.PositionOffset * scale;
		}
		smokeEffect.transform.localScale = smokeEffect.DefaultScale * scale;
		smokeEffect.SpriteRenderer.color = tint;
		if (layer >= 0)
		{
			smokeEffect.gameObject.layer = layer;
		}
		queue.Enqueue(smokeEffect);
		smokeEffect.Poof();
		return smokeEffect;
	}

	public SmokeEffect SpawnSmokeTransform(SmokeType type, Transform transform, Color? tint = null)
	{
		Queue<SmokeEffect> queue;
		switch (type)
		{
		case SmokeType.POOF:
			queue = poofPool;
			break;
		case SmokeType.JUMP:
			queue = jumpPool;
			break;
		case SmokeType.LAND:
			queue = landPool;
			break;
		case SmokeType.UIPOOF:
			queue = uiSmokePool;
			break;
		case SmokeType.WALL_L:
			queue = wallLeftPool;
			break;
		case SmokeType.WALL_R:
			queue = wallRightPool;
			break;
		case SmokeType.MULTIJUMP:
			queue = multiJumpSmokePool;
			break;
		case SmokeType.ANIMALCANNON_SMOKE:
			queue = animalCannonCharacterSmokePool;
			break;
		case SmokeType.ANIMLACANNON_BLUR:
			queue = animalCannonCharacterBlurPool;
			break;
		default:
			return null;
		}
		SmokeEffect smokeEffect = queue.Dequeue();
		smokeEffect.transform.position = transform.position;
		smokeEffect.transform.rotation = transform.rotation;
		smokeEffect.transform.localScale = transform.lossyScale;
		if (tint.HasValue)
		{
			smokeEffect.SpriteRenderer.color = tint.Value;
		}
		queue.Enqueue(smokeEffect);
		smokeEffect.Poof();
		return smokeEffect;
	}
}
