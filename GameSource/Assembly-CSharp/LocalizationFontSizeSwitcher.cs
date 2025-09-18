using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(Localize))]
public class LocalizationFontSizeSwitcher : MonoBehaviour, ISerializationCallbackReceiver
{
	public int defaultFontSize = -1;

	private Dictionary<string, int> perLanguageSizes = new Dictionary<string, int>();

	[SerializeField]
	private List<string> _keys = new List<string>();

	[SerializeField]
	private List<int> _values = new List<int>();

	private Localize localizeComponent;

	private Text textComponent;

	private bool initialized;

	private void OnEnable()
	{
		CheckInitialized();
	}

	public bool CheckInitialized()
	{
		if (!initialized)
		{
			Initialize();
			return true;
		}
		return false;
	}

	private void Initialize()
	{
		textComponent = GetComponent<Text>();
		localizeComponent = GetComponent<Localize>();
		if (localizeComponent.LocalizeEvent.GetPersistentEventCount() >= 2)
		{
			UnityAction call = OnLocalizationChange;
			localizeComponent.LocalizeEvent.AddListener(call);
			localizeComponent.LocalizeCallBack.Target = null;
			localizeComponent.LocalizeCallBack.MethodName = null;
		}
		initialized = true;
		OnLocalizationChange();
	}

	public bool HasEntryForLanguage(string lang)
	{
		return perLanguageSizes.ContainsKey(lang);
	}

	public void SetFontSizeForLanguage(string lang, int size)
	{
		if (perLanguageSizes.ContainsKey(lang))
		{
			perLanguageSizes[lang] = size;
		}
		else
		{
			perLanguageSizes.Add(lang, size);
		}
		if (lang == "English")
		{
			defaultFontSize = size;
		}
	}

	public void RemoveFontSizeForLanguage(string lang)
	{
		perLanguageSizes.Remove(lang);
	}

	public int GetFontSizeForLanguage(string lang)
	{
		int value = 0;
		if (perLanguageSizes.TryGetValue(lang, out value))
		{
			return value;
		}
		return defaultFontSize;
	}

	public void ClearAll()
	{
		if (!textComponent.resizeTextForBestFit)
		{
			textComponent.fontSize = defaultFontSize;
		}
		perLanguageSizes.Clear();
	}

	public void OnLocalizationChange()
	{
		if (!CheckInitialized() && !textComponent.resizeTextForBestFit)
		{
			textComponent.fontSize = GetFontSizeForLanguage(LocalizationManager.CurrentLanguage);
		}
	}

	public void OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		foreach (KeyValuePair<string, int> perLanguageSize in perLanguageSizes)
		{
			_keys.Add(perLanguageSize.Key);
			_values.Add(perLanguageSize.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		perLanguageSizes = new Dictionary<string, int>();
		for (int i = 0; i != Mathf.Min(_keys.Count, _values.Count); i++)
		{
			perLanguageSizes.Add(_keys[i], _values[i]);
		}
	}

	public void SetDict(Dictionary<string, int> dict)
	{
		int value = -1;
		if (!dict.TryGetValue("English", out value))
		{
			value = textComponent.fontSize;
		}
		defaultFontSize = value;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> item in dict)
		{
			if (item.Key == "English" || item.Value != defaultFontSize)
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		perLanguageSizes = dictionary;
	}
}
