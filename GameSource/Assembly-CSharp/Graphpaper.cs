using System.Collections;
using GameEvent;
using UnityEngine;

public class Graphpaper : MonoBehaviour, IGameEventListener
{
	public ColoredThings[] coloredThings;

	public bool isEnabled;

	public bool updateColorEveryFrame;

	public float fadeTime = 1f;

	public float maxAlpha = 0.7f;

	public CanvasGroup CanvasGroup;

	private void Awake()
	{
		ColoredThings[] array = coloredThings;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].InitilizeColoredThing();
		}
	}

	private void Start()
	{
		ChangeListener(adding: true);
		quickDisableGrid();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	private void Update()
	{
		if (updateColorEveryFrame)
		{
			ColoredThings[] array = coloredThings;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateColorArray();
			}
		}
	}

	public void enableGrid()
	{
		if (!isEnabled && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(fadeAlpha(0f, maxAlpha, fadeTime));
			isEnabled = true;
		}
	}

	public void quickDisableGrid()
	{
		StartCoroutine(fadeAlpha(0f, 0f, 0f));
		isEnabled = false;
	}

	public void disableGrid()
	{
		if (isEnabled && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(fadeAlpha(maxAlpha, 0f, fadeTime));
			isEnabled = false;
		}
	}

	private IEnumerator fadeAlpha(float fromValue, float toValue, float time)
	{
		float timer = 0f;
		float num;
		ColoredThings[] array;
		do
		{
			num = Mathf.Lerp(fromValue, toValue, timer / time);
			array = coloredThings;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateArrayAlpha(num);
			}
			CanvasGroup.alpha = num;
			timer += Time.deltaTime;
			yield return null;
		}
		while (timer <= time);
		num = toValue;
		array = coloredThings;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateArrayAlpha(num);
		}
		CanvasGroup.alpha = num;
	}

	private void ToPlayMode()
	{
		disableGrid();
	}

	private void ToPlaceMode()
	{
		enableGrid();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(StartPhaseEvent)))
		{
			return;
		}
		StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
		GameSettings instance = GameSettings.GetInstance();
		if (instance.GameMode != GameState.GameMode.PARTY || instance.partyBoxMode != PartyBoxMode.Disabled)
		{
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY)
			{
				ToPlayMode();
			}
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
			{
				ToPlaceMode();
			}
		}
	}
}
