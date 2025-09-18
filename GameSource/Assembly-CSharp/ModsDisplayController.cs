using System;
using System.Collections;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class ModsDisplayController : MonoBehaviour, IGameEventListener
{
	public Canvas modCanvas;

	public CanvasGroup modCanvasGroup;

	public Text modListText;

	public GameObject rulesHeaderText;

	public Text rulesListText;

	public RectTransform centeredContainer;

	public RectTransform container;

	private IEnumerator anim;

	private bool fadingStarted;

	private bool TabletShown
	{
		get
		{
			InventoryBook inventoryBook = null;
			if (LobbyManager.instance.CurrentLevelSelectController != null)
			{
				inventoryBook = LobbyManager.instance.CurrentLevelSelectController.GameRuleBook;
			}
			else if (LobbyManager.instance.CurrentGameController != null)
			{
				inventoryBook = LobbyManager.instance.CurrentGameController.InventoryBook;
			}
			if (inventoryBook != null && inventoryBook.Visible && inventoryBook.ScreenMode)
			{
				return inventoryBook.CurrentScreenpage == inventoryBook.TabletPage;
			}
			return false;
		}
	}

	private void Awake()
	{
		ChangeListener(adding: true);
		modCanvas.gameObject.SetActive(value: false);
		modListText.text = "";
		rulesHeaderText.gameObject.SetActive(value: false);
		rulesListText.text = "";
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<ModifiersChangedEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	public void OnStartPhaseEnded()
	{
		Modifiers instance = Modifiers.GetInstance();
		GameSettings instance2 = GameSettings.GetInstance();
		if (instance.AppliedAndNonDefault || instance2.HaveNonDefaultRules)
		{
			DoFadeInOut(0.5f, 5f, 0.5f);
		}
		else
		{
			modCanvas.gameObject.SetActive(value: false);
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.START)
		{
			OnStartPhaseEnded();
		}
		if (type == typeof(ModifiersChangedEvent))
		{
			DoFadeInOut(0.5f, 5f, 0.5f);
		}
	}

	private void Update()
	{
		if (modCanvas.worldCamera == null)
		{
			modCanvas.worldCamera = GetComponentInParent<Camera>();
		}
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
		Modifiers instance = Modifiers.GetInstance();
		GameSettings instance2 = GameSettings.GetInstance();
		bool flag = LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null;
		bool flag2 = false;
		if (TabletShown)
		{
			flag2 = ((!flag) ? (instance.AppliedAndNonDefault || instance2.HaveNonDefaultRules) : instance.IsNonDefault);
		}
		if (flag2 && !fadingStarted)
		{
			DoFadeInOut(0.5f, 1f, 0.5f);
		}
	}

	public void DoFadeInOut(float fadeInTime, float stayTime, float fadeOutTime)
	{
		Modifiers instance = Modifiers.GetInstance();
		bool flag = LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null;
		modListText.text = instance.GetCurrentModifierListString(flag);
		string rulesListString = GameSettings.GetInstance().GetRulesListString(flag);
		if (rulesListString != null)
		{
			rulesHeaderText.gameObject.SetActive(value: true);
			rulesListText.gameObject.SetActive(value: true);
			rulesListText.text = rulesListString;
		}
		else
		{
			rulesHeaderText.gameObject.SetActive(value: false);
			rulesListText.gameObject.SetActive(value: false);
			rulesListText.text = "";
		}
		fadingStarted = true;
		anim = FadeInOut(fadeInTime, stayTime, fadeOutTime);
		anim.MoveNext();
	}

	private IEnumerator FadeInOut(float fadeInTime, float stayTime, float fadeOutTime)
	{
		modCanvas.gameObject.SetActive(value: true);
		UpdateContainerAnchors();
		IEnumerator a;
		if (modCanvasGroup.alpha < 1f)
		{
			float duration = (1f - modCanvasGroup.alpha) * fadeInTime;
			CanvasGroupAlphaTweener canvasGroupAlphaTweener = new CanvasGroupAlphaTweener(modCanvasGroup, 0f, 1f, duration, Easings.Functions.CubicEaseIn);
			canvasGroupAlphaTweener.useUnscaledDeltaTime = true;
			a = canvasGroupAlphaTweener.Animate();
			while (a.MoveNext())
			{
				yield return null;
			}
		}
		for (float t = 0f; t < stayTime; t += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		while (TabletShown)
		{
			yield return null;
		}
		fadingStarted = false;
		yield return null;
		CanvasGroupAlphaTweener canvasGroupAlphaTweener2 = new CanvasGroupAlphaTweener(modCanvasGroup, 1f, 0f, fadeOutTime, Easings.Functions.CubicEaseOut);
		canvasGroupAlphaTweener2.useUnscaledDeltaTime = true;
		a = canvasGroupAlphaTweener2.Animate();
		while (a.MoveNext())
		{
			yield return null;
		}
		modCanvas.gameObject.SetActive(value: false);
	}

	private void UpdateContainerAnchors()
	{
		Canvas.ForceUpdateCanvases();
		float y = centeredContainer.sizeDelta.y;
		if (container.sizeDelta.y > y)
		{
			container.pivot = new Vector2(0.5f, 0.5f);
			container.anchorMax = new Vector2(0.5f, 0.5f);
			container.anchorMin = new Vector2(0.5f, 0.5f);
			container.anchoredPosition = Vector2.zero;
		}
		else
		{
			container.pivot = new Vector2(0.5f, 1f);
			container.anchorMax = new Vector2(0.5f, 1f);
			container.anchorMin = new Vector2(0.5f, 1f);
			container.anchoredPosition = Vector2.zero;
		}
	}
}
