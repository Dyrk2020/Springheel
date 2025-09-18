using System.Collections.Generic;
using UnityEngine;

public class DoomsdayLava : MonoBehaviour
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	public Direction lavaDirection;

	public Vector2 extraPadding = Vector2.zero;

	public Vector2 levelSize = new Vector2(120f, 80f);

	public Vector2 lavaStart = new Vector2(0f, 0f);

	public Vector2 lavaEnd = new Vector2(0f, 80f);

	public float lavaRot;

	public float waveElementWidth = 18.72f;

	public float lavaSpeedMultiplier = 1f;

	public Vector3[] subwaveOffsets;

	public float[] subwaveScales;

	public float[] subwaveAnimationSpeeds;

	public GameObject[] subwaves;

	public GameObject[] waveElements;

	public Color[] waveColors;

	public GameObject[] bottomSprites;

	public List<GameObject> generatedElements = new List<GameObject>();

	public BoxCollider2D hazardCollider;

	private ColliderOverStart overStartObj;

	private AkGameObj wwiseGameObject;

	public bool OverStartZone
	{
		get
		{
			if (!(overStartObj == null))
			{
				return overStartObj.OverStartZone;
			}
			return false;
		}
	}

	public void Initialize()
	{
		overStartObj = GetComponentInChildren<ColliderOverStart>();
		int num = 0;
		int index = 1;
		Direction direction = lavaDirection;
		if ((uint)(direction - 2) <= 1u)
		{
			num = 1;
			index = 0;
		}
		if (LobbyManager.instance != null)
		{
			Bounds cameraBounds = LobbyManager.instance.CurrentGameController.LevelLayout.GetCameraBounds();
			Vector3 extents = cameraBounds.extents;
			extents[num] += 100f + extraPadding[num];
			extents[index] += 2f + extraPadding[index];
			cameraBounds.extents = extents;
			levelSize = new Vector2(cameraBounds.size.x, cameraBounds.size.y);
			switch (lavaDirection)
			{
			case Direction.Up:
				lavaStart = new Vector2(cameraBounds.min.x, cameraBounds.min.y);
				lavaEnd = new Vector2(cameraBounds.min.x, cameraBounds.max.y);
				lavaRot = 0f;
				break;
			case Direction.Down:
				lavaStart = new Vector2(cameraBounds.max.x, cameraBounds.max.y);
				lavaEnd = new Vector2(cameraBounds.max.x, cameraBounds.min.y);
				lavaRot = 180f;
				break;
			case Direction.Left:
				lavaStart = new Vector2(cameraBounds.max.x, cameraBounds.min.y);
				lavaEnd = new Vector2(cameraBounds.min.x, cameraBounds.min.y);
				lavaRot = 90f;
				break;
			case Direction.Right:
				lavaStart = new Vector2(cameraBounds.min.x, cameraBounds.max.y);
				lavaEnd = new Vector2(cameraBounds.max.x, cameraBounds.max.y);
				lavaRot = -90f;
				break;
			}
			Vector3 position = base.transform.position;
			position.x = lavaStart.x;
			position.y = lavaStart.y;
			base.transform.position = position;
			base.transform.rotation = Quaternion.AngleAxis(lavaRot, Vector3.forward);
			wwiseGameObject = GetComponent<AkGameObj>();
			wwiseGameObject.m_positionOffsetData.positionOffset.x = 0f - position.x;
		}
		for (int i = 0; i < subwaves.Length; i++)
		{
			GameObject gameObject = subwaves[i];
			GameObject gameObject2 = waveElements[i];
			SpriteRenderer[] componentsInChildren = gameObject2.GetComponentsInChildren<SpriteRenderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].color = waveColors[i];
			}
			gameObject2.GetComponent<Animator>().speed = subwaveAnimationSpeeds[i];
			int num2 = Mathf.CeilToInt((levelSize[num] - subwaveOffsets[i].x) / (waveElementWidth * subwaveScales[i]));
			for (int k = 1; k < num2; k++)
			{
				GameObject gameObject3 = Object.Instantiate(gameObject2);
				gameObject3.GetComponent<Animator>().speed = subwaveAnimationSpeeds[i];
				gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject3.transform.localPosition = new Vector3(waveElementWidth * (float)k, 0f, 0f);
				generatedElements.Add(gameObject3);
			}
			gameObject.transform.localPosition = subwaveOffsets[i];
			gameObject.transform.localScale = new Vector3(subwaveScales[i], subwaveScales[i], 1f);
			SpriteRenderer component = bottomSprites[i].GetComponent<SpriteRenderer>();
			component.color = waveColors[i];
			float num3 = (float)num2 * waveElementWidth * subwaveScales[i];
			float x = num3 * 1.5625f;
			float num4 = levelSize[(num + 1) % 2];
			if (i < 2)
			{
				num4 = 5f;
			}
			float y = num4 * 1.5625f;
			component.transform.localScale = new Vector3(x, y, 1f);
			component.transform.localPosition = new Vector3(num3 / 2f, (0f - num4) / 2f, 0f) + subwaveOffsets[i];
			if (i == 2)
			{
				hazardCollider.offset = component.transform.localPosition;
				hazardCollider.size = new Vector2(num3, num4);
			}
		}
		AkSoundEngine.PostEvent("ENV_DoomsdayLava_Start", base.gameObject);
	}

	private void OnDestroy()
	{
		AkSoundEngine.PostEvent("ENV_DoomsdayLava_Stop", base.gameObject);
	}

	public void ResetPrefab()
	{
		foreach (GameObject generatedElement in generatedElements)
		{
			Object.DestroyImmediate(generatedElement.gameObject);
		}
		GameObject[] array = subwaves;
		foreach (GameObject obj in array)
		{
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
		}
		array = bottomSprites;
		foreach (GameObject obj2 in array)
		{
			obj2.transform.localPosition = Vector3.zero;
			obj2.transform.localScale = Vector3.one;
		}
		hazardCollider.offset = Vector2.zero;
		hazardCollider.size = Vector2.one;
		generatedElements.Clear();
	}

	private void FixedUpdate()
	{
		if (!GameState.GetInstance().Paused)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, lavaEnd, lavaSpeedMultiplier * Modifiers.GetInstance().DoomsdayLavaRiseSpeed * Time.deltaTime);
			int index = 1;
			Direction direction = lavaDirection;
			if ((uint)(direction - 2) <= 1u)
			{
				index = 0;
			}
			AkSoundEngine.SetRTPCValue("Lava_Height", Mathf.InverseLerp(lavaStart[index], lavaEnd[index], base.transform.position[index]) * 100f, base.gameObject);
		}
	}
}
