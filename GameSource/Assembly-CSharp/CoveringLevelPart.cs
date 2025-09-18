using System.Collections;
using GameEvent;
using UnityEngine;

public class CoveringLevelPart : MonoBehaviour, IGameEventListener
{
	private SpriteRenderer bottomSprite;

	private SpriteRenderer topSprite;

	public bool freePlayOnly = true;

	private void Awake()
	{
		if (freePlayOnly && GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			Object.Destroy(this);
			return;
		}
		bottomSprite = GetComponent<SpriteRenderer>();
		if (bottomSprite != null)
		{
			GameObject gameObject = new GameObject("Covering Level Part Top Sprite");
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			gameObject.layer = bottomSprite.gameObject.layer;
			topSprite = gameObject.AddComponent<SpriteRenderer>();
			topSprite.sortingLayerName = "Foreground Background";
			topSprite.sprite = bottomSprite.sprite;
			bottomSprite.sortingLayerName = "Main Background";
			bottomSprite.enabled = false;
		}
		else
		{
			Debug.LogError("Could not find bottom sprite.");
		}
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				StartCoroutine(FadeTopSprite(0f, 1f, 1f));
			}
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				StartCoroutine(FadeTopSprite(1f, 0f, 1f));
			}
		}
	}

	private IEnumerator FadeTopSprite(float alpha0, float alpha1, float duration)
	{
		bottomSprite.enabled = true;
		topSprite.enabled = true;
		topSprite.SetAlpha(alpha0);
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float t = time / duration;
			topSprite.SetAlpha(Mathf.Lerp(alpha0, alpha1, t));
			yield return null;
		}
		if (topSprite.color.a == 0f)
		{
			topSprite.enabled = false;
		}
		else if (topSprite.color.a == 1f)
		{
			bottomSprite.enabled = false;
		}
	}
}
