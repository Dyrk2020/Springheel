using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TabletScreen : TabletStyledObject
{
	public enum TransitionSound
	{
		None,
		Submenu,
		Modal
	}

	public Tablet tablet;

	public CanvasGroup canvasGroup;

	private IEnumerator transitionAnim;

	public bool transitioning;

	public virtual void OnTransitionInBegin()
	{
	}

	public virtual void OnTransitionInEnd()
	{
	}

	public virtual void OnTransitionOutBegin()
	{
	}

	public virtual void OnTransitionOutEnd()
	{
	}

	public virtual void Update()
	{
		if (transitionAnim != null && !transitionAnim.MoveNext())
		{
			transitionAnim = null;
		}
	}

	public void TransitionOut()
	{
		OnTransitionOutBegin();
		transitioning = true;
		transitionAnim = AnimateTransitionOut();
	}

	private IEnumerator AnimateTransitionOut()
	{
		float t = 0f;
		float transitionTime = 0.5f;
		while (t < transitionTime)
		{
			t += Time.deltaTime;
			canvasGroup.alpha = Mathf.Clamp01(1f - t / transitionTime);
			yield return null;
		}
		canvasGroup.alpha = 0f;
		transitioning = false;
		OnTransitionOutEnd();
		base.gameObject.SetActive(value: false);
	}

	public void TransitionIn(TabletScreen previousScreen)
	{
		base.gameObject.SetActive(value: true);
		canvasGroup.alpha = 1f;
		OnTransitionInBegin();
		transitioning = true;
		transitionAnim = AnimateTransitionIn(previousScreen);
	}

	private IEnumerator AnimateTransitionIn(TabletScreen previousScreen)
	{
		if (previousScreen != null)
		{
			while (previousScreen.transitioning)
			{
				yield return null;
			}
		}
		transitioning = false;
		OnTransitionInEnd();
	}

	public override void ResetStyles()
	{
		Image component = GetComponent<Image>();
		if (component != null)
		{
			component.color = colorScheme.bgColor;
		}
		TabletStyledObject[] componentsInChildren = GetComponentsInChildren<TabletStyledObject>(includeInactive: true);
		foreach (TabletStyledObject tabletStyledObject in componentsInChildren)
		{
			if (tabletStyledObject != this)
			{
				tabletStyledObject.colorScheme = colorScheme;
				tabletStyledObject.ResetStyles();
			}
		}
	}

	public virtual void OnClickBurger(PickCursor pickCursor)
	{
		tablet.OpenBurgerMenu(pickCursor);
	}

	public virtual bool OnPressBack(PickCursor pickCursor)
	{
		return false;
	}

	public virtual bool OnRotateLeft(PickCursor pickCursor)
	{
		return false;
	}

	public virtual bool OnRotateRight(PickCursor pickCursor)
	{
		return false;
	}

	public virtual void OpenModalOverlay(TabletRule overlayType)
	{
		tablet.modalOverlay.Initialize(overlayType, OnModalOverlayClosed);
	}

	public virtual void OnModalOverlayClosed()
	{
	}

	public virtual void OnCursorScroll(Vector2 scrollAmount)
	{
	}
}
