using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TabletToggleButtonGroup : MonoBehaviour
{
	[Serializable]
	public class ToggleButtonData
	{
		public TabletButton button;

		public int value;

		public bool shouldDisable;
	}

	public ToggleButtonData[] toggleButtons;

	public int selectedIndex = -1;

	public int defaultIndex = -1;

	public UnityEvent OnValueChanged;

	public int Value => toggleButtons[selectedIndex].value;

	private void Awake()
	{
	}

	public void ResetButtons()
	{
		List<ToggleButtonData> list = new List<ToggleButtonData>();
		int num = 0;
		TabletButton[] componentsInChildren = GetComponentsInChildren<TabletButton>();
		foreach (TabletButton tabletButton in componentsInChildren)
		{
			if (num == defaultIndex)
			{
				tabletButton.buttonType = TabletButton.ButtonType.Simple;
				tabletButton.SetInteractable(interactable: false);
			}
			else
			{
				tabletButton.buttonType = TabletButton.ButtonType.Transparent;
				tabletButton.SetInteractable(interactable: true);
			}
			tabletButton.OnClick.RemoveAllListeners();
			tabletButton.OnClick.AddListener(GetOnClickButtonCallback(tabletButton));
			tabletButton.SetDisabled(disabled: false);
			ToggleButtonData item = new ToggleButtonData
			{
				button = tabletButton,
				value = num++,
				shouldDisable = false
			};
			list.Add(item);
		}
		toggleButtons = list.ToArray();
	}

	private UnityAction<PickCursor> GetOnClickButtonCallback(TabletButton button)
	{
		return delegate
		{
			OnClickButton(button);
		};
	}

	public void OnClickButton(TabletButton button)
	{
		OnClickButtonInternal(button);
	}

	private void OnClickButtonInternal(TabletButton button, bool fireEvent = true)
	{
		for (int i = 0; i < toggleButtons.Length; i++)
		{
			if (!(toggleButtons[i].button == button))
			{
				continue;
			}
			if (selectedIndex != i)
			{
				selectedIndex = i;
				UpdateButtonProperties();
				if (fireEvent)
				{
					OnValueChanged.Invoke();
				}
			}
			break;
		}
	}

	public void Deselect()
	{
		selectedIndex = -1;
		UpdateButtonProperties();
	}

	public void SelectByValue(int val, bool fireEvent = true)
	{
		for (int i = 0; i < toggleButtons.Length; i++)
		{
			if (toggleButtons[i].value != val)
			{
				continue;
			}
			if (selectedIndex != i)
			{
				selectedIndex = i;
				UpdateButtonProperties();
				if (fireEvent)
				{
					OnValueChanged.Invoke();
				}
			}
			return;
		}
		Debug.LogError("TabletToggleButtonGroup " + base.name + " could not select entry with value " + val);
	}

	private void UpdateButtonProperties()
	{
		for (int i = 0; i < toggleButtons.Length; i++)
		{
			ToggleButtonData toggleButtonData = toggleButtons[i];
			if (i == selectedIndex)
			{
				if (toggleButtonData.button.buttonType != TabletButton.ButtonType.Simple)
				{
					toggleButtonData.button.buttonType = TabletButton.ButtonType.Simple;
					toggleButtonData.button.SetInteractable(interactable: false);
					toggleButtonData.button.SetDisabled(toggleButtonData.shouldDisable);
				}
			}
			else if (toggleButtonData.button.buttonType != TabletButton.ButtonType.Transparent)
			{
				toggleButtonData.button.buttonType = TabletButton.ButtonType.Transparent;
				toggleButtonData.button.SetInteractable(interactable: true);
				toggleButtonData.button.SetDisabled(toggleButtonData.shouldDisable);
			}
		}
	}
}
