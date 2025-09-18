using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GetCodeToggleGroup : MonoBehaviour
{
	public List<GetCodeToggleButton> toggleButtons;

	public Color selectedColor;

	public Color unselectedColor;

	public GetCodeToggleButton selectedToggleButton;

	public UnityEvent OnValueChange;

	public void OnClickToggleButton(GetCodeToggleButton clickedButton)
	{
		if (selectedToggleButton == clickedButton)
		{
			selectedToggleButton = null;
		}
		else
		{
			selectedToggleButton = clickedButton;
		}
		UpdateBGColors();
		OnValueChange.Invoke();
	}

	public void SelectButtonByIndex(int idx, bool triggerEvent)
	{
		if (idx == -1)
		{
			selectedToggleButton = null;
		}
		else
		{
			selectedToggleButton = toggleButtons[idx];
		}
		UpdateBGColors();
		if (triggerEvent)
		{
			OnValueChange.Invoke();
		}
	}

	public int GetCurrentValue()
	{
		if (selectedToggleButton == null)
		{
			return -1;
		}
		return selectedToggleButton.toggleValue;
	}

	private void UpdateBGColors()
	{
		foreach (GetCodeToggleButton toggleButton in toggleButtons)
		{
			if (toggleButton != selectedToggleButton)
			{
				toggleButton.SetBGColor(unselectedColor);
			}
			else
			{
				toggleButton.SetBGColor(selectedColor);
			}
		}
	}

	public void DeselectCurrentToggle(bool triggerEvent)
	{
		selectedToggleButton = null;
		UpdateBGColors();
		if (triggerEvent)
		{
			OnValueChange.Invoke();
		}
	}

	public void SetInteractable(bool interactable)
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.alpha = (interactable ? 1f : 0.5f);
		}
		foreach (GetCodeToggleButton toggleButton in toggleButtons)
		{
			toggleButton.GetComponent<BoxCollider2D>().enabled = interactable;
		}
	}
}
