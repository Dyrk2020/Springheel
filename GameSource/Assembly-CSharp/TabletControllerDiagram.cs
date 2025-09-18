using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabletControllerDiagram : MonoBehaviour
{
	public TabletSubdialogController subdialogController;

	public RectTransform[] subdialogs;

	private List<RectTransform> subdialogsOnPlatform = new List<RectTransform>();

	private int currentIdx;

	public TabletButton prevControllerButton;

	public TabletButton nextControllerButton;

	public Text pageNumberIndicator;

	private bool isInitialized;

	public void Start()
	{
		if (!isInitialized)
		{
			SetCurrentPageIndex(0);
		}
		if (subdialogsOnPlatform.Count <= 1)
		{
			prevControllerButton.gameObject.SetActive(value: false);
			nextControllerButton.gameObject.SetActive(value: false);
		}
	}

	public void OnClickNext(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			currentIdx = (currentIdx + 1) % subdialogsOnPlatform.Count;
			SetPageIndicator(currentIdx + 1);
			subdialogController.TransitionLeftTo(subdialogsOnPlatform[currentIdx], TabletScreen.TransitionSound.None, disableCurrent: false);
			subdialogController.ClearStack();
			AkSoundEngine.PostEvent("UI_UPad_Options_Controls_Right", base.gameObject);
		}
	}

	public void OnClickPrev(PickCursor pickCursor)
	{
		if (!subdialogController.IsAnimating)
		{
			currentIdx = (currentIdx - 1 + subdialogsOnPlatform.Count) % subdialogsOnPlatform.Count;
			SetPageIndicator(currentIdx + 1);
			subdialogController.TransitionRightTo(subdialogsOnPlatform[currentIdx], disableCurrent: false);
			subdialogController.ClearStack();
			AkSoundEngine.PostEvent("UI_UPad_Options_Controls_Left", base.gameObject);
		}
	}

	public void SetPageIndicator(int newPageNumber)
	{
		if (pageNumberIndicator != null)
		{
			pageNumberIndicator.text = newPageNumber + "/" + subdialogsOnPlatform.Count;
		}
	}

	public void InitializePlatformSubdialogs()
	{
		if (isInitialized)
		{
			return;
		}
		subdialogsOnPlatform.Clear();
		RectTransform[] array = subdialogs;
		foreach (RectTransform rectTransform in array)
		{
			AllowedOnPlatform component = rectTransform.GetComponent<AllowedOnPlatform>();
			if (component != null && component.GetAllowed)
			{
				subdialogsOnPlatform.Add(rectTransform);
			}
		}
		isInitialized = true;
	}

	public void SetCurrentPageIndex(int pageIndex)
	{
		InitializePlatformSubdialogs();
		currentIdx = Mathf.Clamp(pageIndex, 0, subdialogsOnPlatform.Count - 1);
		subdialogController.ForceSubdialog(subdialogsOnPlatform[currentIdx]);
		subdialogController.ClearStack();
		SetPageIndicator(currentIdx + 1);
	}

	public void ResetToFirstPage()
	{
		SetCurrentPageIndex(0);
	}
}
