using System;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropdownMenu : MonoBehaviour, IGameEventListener
{
	public delegate bool DropdownEntryFinder(DropdownEntry entry);

	public Text mainLabel;

	public Transform mainBox;

	public Transform popupBox;

	public UnityEvent OnValueChange;

	public DropdownEntry selectedDropdownEntry;

	public CanvasGroup canvasGroup;

	public List<DropdownEntry> dropdownEntries;

	private bool deployed;

	private bool clickDisabled;

	public static bool dropdownDeployed;

	private void Awake()
	{
		dropdownDeployed = false;
		Vector3 localScale = popupBox.localScale;
		localScale.y = 1f;
		popupBox.localScale = localScale;
		deployed = false;
		ChangeListener(adding: true);
		dropdownEntries = new List<DropdownEntry>();
		foreach (Transform item in popupBox)
		{
			DropdownEntry component = item.GetComponent<DropdownEntry>();
			if (component != null)
			{
				dropdownEntries.Add(component);
			}
		}
	}

	private void Start()
	{
		popupBox.gameObject.SetActive(value: false);
		if (selectedDropdownEntry != null && dropdownEntries.Contains(selectedDropdownEntry))
		{
			if (selectedDropdownEntry.labelText.GetComponent<Localize>() != null)
			{
				mainLabel.text = LocalizationManager.GetTranslation(selectedDropdownEntry.labelText.GetComponent<Localize>().Term);
				mainLabel.fontSize = selectedDropdownEntry.labelText.GetComponent<LocalizationFontSizeSwitcher>().GetFontSizeForLanguage(LocalizationManager.CurrentLanguage);
			}
			else
			{
				mainLabel.text = selectedDropdownEntry.labelText.text;
				mainLabel.fontSize = selectedDropdownEntry.labelText.fontSize;
			}
		}
		else
		{
			mainLabel.text = "";
			selectedDropdownEntry = null;
		}
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PickCursorClickedBackgroundEvent>(this, adding);
		GameEventManager.ChangeListener<NoteBookDisplayEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public void OnClickMainBox()
	{
		if (!deployed)
		{
			ShowPopup(show: true);
		}
	}

	private void ShowPopup(bool show)
	{
		deployed = show;
		dropdownDeployed = show;
		mainBox.GetComponent<BoxCollider2D>().enabled = !deployed && !clickDisabled;
		popupBox.gameObject.SetActive(show);
		if (show)
		{
			PickableButton[] buttons = dropdownEntries.ToArray();
			PickableButton.AllowOnlyButtons(buttons);
		}
		else
		{
			PickableButton.ResetMasks();
		}
		RecalculatePopupPosition();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NoteBookDisplayEvent) && deployed && !(e as NoteBookDisplayEvent).Opened)
		{
			ShowPopup(show: false);
		}
		if (type == typeof(PickCursorClickedBackgroundEvent) && deployed)
		{
			ShowPopup(show: false);
		}
		if (type == typeof(LanguageChangeEvent) && selectedDropdownEntry != null && dropdownEntries.Contains(selectedDropdownEntry))
		{
			if (selectedDropdownEntry.labelText.GetComponent<Localize>() != null)
			{
				mainLabel.text = LocalizationManager.GetTranslation(selectedDropdownEntry.labelText.GetComponent<Localize>().Term);
				mainLabel.fontSize = selectedDropdownEntry.labelText.GetComponent<LocalizationFontSizeSwitcher>().GetFontSizeForLanguage(LocalizationManager.CurrentLanguage);
			}
			else
			{
				mainLabel.text = selectedDropdownEntry.labelText.text;
				mainLabel.fontSize = selectedDropdownEntry.labelText.fontSize;
			}
		}
	}

	public void OnClickDropdownEntry(DropdownEntry entry, bool triggerOnChangeEvent)
	{
		ShowPopup(show: false);
		if (selectedDropdownEntry != entry)
		{
			selectedDropdownEntry = entry;
			Localize component = selectedDropdownEntry.labelText.GetComponent<Localize>();
			if (component != null)
			{
				mainLabel.text = LocalizationManager.GetTranslation(component.Term);
			}
			else
			{
				mainLabel.text = selectedDropdownEntry.labelText.text;
			}
			LocalizationFontSizeSwitcher component2 = selectedDropdownEntry.labelText.GetComponent<LocalizationFontSizeSwitcher>();
			if (component2 != null)
			{
				mainLabel.fontSize = component2.GetFontSizeForLanguage(LocalizationManager.CurrentLanguage);
			}
			if (triggerOnChangeEvent && OnValueChange != null)
			{
				OnValueChange.Invoke();
			}
			entry.GetComponent<PickableButton>().ResetScale();
		}
	}

	public DropdownEntry FindFirstDropdownEntryWithCriteria(DropdownEntryFinder finder)
	{
		foreach (DropdownEntry dropdownEntry in dropdownEntries)
		{
			if (finder(dropdownEntry))
			{
				return dropdownEntry;
			}
		}
		return null;
	}

	public bool TryGetEntryByIndex(int idx, out DropdownEntry entry)
	{
		if (idx >= 0 && idx < dropdownEntries.Count)
		{
			entry = dropdownEntries[idx];
			return true;
		}
		Debug.LogError("Dropdown " + base.name + " - Invalid index: " + idx);
		entry = null;
		return false;
	}

	public void SelectEntryByIndex(int idx, bool triggerOnChangeEvent)
	{
		if (TryGetEntryByIndex(idx, out var entry))
		{
			if (entry != null)
			{
				OnClickDropdownEntry(entry, triggerOnChangeEvent);
			}
			else
			{
				Debug.LogError("Dropdown " + base.name + " - Null dropdown entry at index: " + idx);
			}
		}
	}

	private void Update()
	{
		if (deployed)
		{
			RecalculatePopupPosition();
		}
	}

	private void RecalculatePopupPosition()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (componentInParent != null)
		{
			RectTransform component = componentInParent.GetComponent<RectTransform>();
			RectTransform component2 = popupBox.GetComponent<RectTransform>();
			float height = mainBox.GetComponent<RectTransform>().rect.height;
			Vector2 anchoredPosition = component2.anchoredPosition;
			anchoredPosition.y = 0f - height;
			component2.anchoredPosition = anchoredPosition;
			Vector3[] array = new Vector3[4];
			component.GetWorldCorners(array);
			Vector3[] array2 = new Vector3[4];
			component2.GetWorldCorners(array2);
			float y = array[0].y;
			if (array2[0].y < y)
			{
				Vector2 anchoredPosition2 = component2.anchoredPosition;
				anchoredPosition2.y = component2.sizeDelta.y;
				component2.anchoredPosition = anchoredPosition2;
			}
		}
	}

	public void SetClickDisabled(bool disabled)
	{
		clickDisabled = disabled;
		canvasGroup.alpha = (clickDisabled ? 0.4f : 1f);
		mainBox.GetComponent<BoxCollider2D>().enabled = !deployed && !clickDisabled;
	}

	public void ClearEntries()
	{
		foreach (DropdownEntry dropdownEntry in dropdownEntries)
		{
			UnityEngine.Object.Destroy(dropdownEntry.gameObject);
		}
		dropdownEntries.Clear();
		selectedDropdownEntry = null;
		mainLabel.text = "";
	}

	public void AddEntry(GameObject obj)
	{
		obj.transform.SetParent(popupBox.transform, worldPositionStays: false);
		DropdownEntry component = obj.GetComponent<DropdownEntry>();
		if (component != null)
		{
			component.dropdown = this;
			dropdownEntries.Add(component);
		}
	}

	public void OnRefreshVisibility()
	{
		SetClickDisabled(clickDisabled);
	}

	public int GetSelectedEntryIndex()
	{
		if (selectedDropdownEntry != null)
		{
			for (int i = 0; i < dropdownEntries.Count; i++)
			{
				if (dropdownEntries[i] == selectedDropdownEntry)
				{
					return i;
				}
			}
		}
		return -1;
	}
}
