using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletSubdialogController : MonoBehaviour
{
	public enum TransitionDirection
	{
		Left,
		Right
	}

	private IEnumerator anim;

	public float transitionTime = 0.25f;

	public AnimationCurve transitionCurve;

	public Transform currentSubdialog;

	public Transform startingSubdialog;

	public List<Transform> subdialogStack = new List<Transform>();

	public bool SubmenuAudio = true;

	public bool IsOnMainSubdialog => startingSubdialog == currentSubdialog;

	public bool IsAnimating => anim != null;

	private void Awake()
	{
		if (startingSubdialog != null)
		{
			currentSubdialog = startingSubdialog;
		}
		subdialogStack.Clear();
		if (currentSubdialog != null)
		{
			subdialogStack.Add(currentSubdialog);
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(item == currentSubdialog);
		}
	}

	public void TransitionLeftTo(Transform otherDialog, TabletScreen.TransitionSound transitionSound, bool disableCurrent = true)
	{
		switch (transitionSound)
		{
		case TabletScreen.TransitionSound.Submenu:
			if (SubmenuAudio)
			{
				AkSoundEngine.PostEvent("UI_UPad_To_Submenu", base.gameObject);
			}
			break;
		case TabletScreen.TransitionSound.Modal:
			AkSoundEngine.PostEvent("UI_UPad_Modal_To_Modal", base.gameObject);
			break;
		}
		TransitionTo(otherDialog, TransitionDirection.Left, disableCurrent);
	}

	public void TransitionRightTo(Transform otherDialog, TabletScreen.TransitionSound transitionSound, bool disableCurrent = true)
	{
		switch (transitionSound)
		{
		case TabletScreen.TransitionSound.Submenu:
			if (SubmenuAudio)
			{
				AkSoundEngine.PostEvent("UI_UPad_Back_Submenu", base.gameObject);
			}
			break;
		case TabletScreen.TransitionSound.Modal:
			AkSoundEngine.PostEvent("UI_UPad_Modal_To_Modal", base.gameObject);
			break;
		}
		TransitionTo(otherDialog, TransitionDirection.Right, disableCurrent);
	}

	public void TransitionRightTo(Transform otherDialog)
	{
		TransitionRightTo(otherDialog, TabletScreen.TransitionSound.Submenu);
	}

	public void TransitionRightTo(Transform otherDialog, bool disableCurrent)
	{
		TransitionRightTo(otherDialog, TabletScreen.TransitionSound.Submenu, disableCurrent);
	}

	public void TransitionLeftTo(Transform otherDialog)
	{
		TransitionLeftTo(otherDialog, TabletScreen.TransitionSound.Submenu);
	}

	public void TransitionLeftTo(Transform otherDialog, bool disableCurrent)
	{
		TransitionLeftTo(otherDialog, TabletScreen.TransitionSound.Submenu, disableCurrent);
	}

	public void TransitionTo(Transform otherDialog, TransitionDirection direction, bool disableCurrent = true)
	{
		if (!(otherDialog != currentSubdialog))
		{
			return;
		}
		anim = AnimateTransition((RectTransform)currentSubdialog, (RectTransform)otherDialog, direction, disableCurrent);
		switch (direction)
		{
		case TransitionDirection.Right:
			if (subdialogStack.Count > 0 && currentSubdialog == subdialogStack[subdialogStack.Count - 1])
			{
				subdialogStack.RemoveAt(subdialogStack.Count - 1);
			}
			if (subdialogStack.Count > 0 && otherDialog != subdialogStack[subdialogStack.Count - 1])
			{
				subdialogStack.Add(otherDialog);
			}
			if (subdialogStack.Count == 0)
			{
				subdialogStack.Add(otherDialog);
			}
			break;
		case TransitionDirection.Left:
			subdialogStack.Add(otherDialog);
			break;
		}
		currentSubdialog = otherDialog;
	}

	private void Update()
	{
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
	}

	private IEnumerator AnimateTransition(RectTransform dialog0, RectTransform dialog1, TransitionDirection direction, bool disableCurrent = true)
	{
		Vector2 centerPos = Vector2.zero;
		RectTransform rectTransform = (RectTransform)base.transform;
		Vector2 widthShift = new Vector2(rectTransform.rect.width, 0f);
		Vector2 dialog0TargetPos;
		Vector2 dialog1SourcePos;
		if (direction == TransitionDirection.Left || direction != TransitionDirection.Right)
		{
			dialog0TargetPos = centerPos - widthShift;
			dialog1SourcePos = centerPos + widthShift;
		}
		else
		{
			dialog0TargetPos = centerPos + widthShift;
			dialog1SourcePos = centerPos - widthShift;
		}
		dialog0.gameObject.SetActive(value: true);
		dialog1.gameObject.SetActive(value: true);
		float timer = 0f;
		while (timer < transitionTime)
		{
			timer += Time.deltaTime;
			float t = transitionCurve.Evaluate(timer / transitionTime);
			dialog0.anchoredPosition = Vector2.Lerp(centerPos, dialog0TargetPos, t);
			dialog1.anchoredPosition = Vector2.Lerp(dialog1SourcePos, centerPos, t);
			yield return null;
		}
		dialog1.anchoredPosition = centerPos;
		if (disableCurrent)
		{
			dialog0.anchoredPosition = dialog0TargetPos;
			dialog0.gameObject.SetActive(value: false);
		}
		else
		{
			dialog0.anchoredPosition = centerPos - widthShift;
		}
	}

	public void PopSubdialog()
	{
		if (subdialogStack.Count > 1)
		{
			TransitionRightTo(subdialogStack[subdialogStack.Count - 2]);
		}
		else
		{
			Debug.LogError("Cannot pop subdialog stack further.", base.gameObject);
		}
	}

	public void ForceSubdialog(RectTransform subdialog)
	{
		subdialogStack.Clear();
		subdialogStack.Add(subdialog);
		subdialog.anchoredPosition = Vector2.zero;
		currentSubdialog = subdialog;
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(item == subdialog);
		}
	}

	public void ClearStack()
	{
		subdialogStack.Clear();
	}
}
