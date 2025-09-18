using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class TwitchTermMapper : MonoBehaviour
{
	[HideInInspector]
	public static Dictionary<string, Dictionary<string, string>> localizedTerms;

	[HideInInspector]
	public static Dictionary<string, List<string>> AllLanguageShortNames;

	private static bool initialized;

	private void Awake()
	{
	}

	public static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			RebuildAllTerms();
		}
	}

	public static void RebuildAllTerms()
	{
		string currentLanguage = LocalizationManager.CurrentLanguage;
		List<string> allLanguages = LocalizationManager.GetAllLanguages();
		AllLanguageShortNames = new Dictionary<string, List<string>>();
		localizedTerms = new Dictionary<string, Dictionary<string, string>>();
		try
		{
			char[] separator = new char[2] { ' ', '|' };
			using (List<string>.Enumerator enumerator = allLanguages.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string key = (LocalizationManager.CurrentLanguage = enumerator.Current);
					AllLanguageShortNames.Add(key, new List<string>(TwitchChatController.itemShortNames.Length));
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					localizedTerms.Add(key, dictionary);
					HashSet<string> hashSet = new HashSet<string>();
					string[] itemShortNames = TwitchChatController.itemShortNames;
					foreach (string text in itemShortNames)
					{
						if (text.NullOrEmpty())
						{
							AllLanguageShortNames[key].Add(null);
							continue;
						}
						bool flag = !hashSet.Add(text);
						string translation = LocalizationManager.GetTranslation("Twitch Voting/Terms/" + text);
						if (translation.NullOrEmpty())
						{
							AllLanguageShortNames[key].Add(null);
							Debug.LogWarning("Twitch Voting Term \"" + text + "\" has no localization.");
							if (!flag)
							{
								if (!dictionary.ContainsKey(text))
								{
									dictionary.Add(text, text);
								}
								else
								{
									Debug.LogError("ERROR: " + text + " is already in localized terms");
								}
							}
							continue;
						}
						string[] array = translation.Split(separator, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length != 0)
						{
							AllLanguageShortNames[key].Add(array[0]);
						}
						else
						{
							AllLanguageShortNames[key].Add("???");
						}
						if (flag)
						{
							continue;
						}
						string[] array2 = array;
						foreach (string text2 in array2)
						{
							if (dictionary.ContainsKey(text2))
							{
								if (dictionary[text2] != text)
								{
									Debug.LogError("Twitch Voting Term \"" + text2 + "\" is used multiple times for multiple Master Terms");
								}
							}
							else
							{
								dictionary.Add(text2, text);
							}
						}
					}
				}
			}
			LocalizationManager.CurrentLanguage = currentLanguage;
		}
		catch (Exception ex)
		{
			Debug.LogError("Twitch Localization Failed - Resetting language! " + ex.Message + "\n" + ex.StackTrace);
			LocalizationManager.CurrentLanguage = currentLanguage;
		}
	}

	public static string GetMasterTerm(string input)
	{
		foreach (string allLanguage in LocalizationManager.GetAllLanguages())
		{
			if (localizedTerms[allLanguage].TryGetValue(input, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public static string GetLocalizedShortName(int pickableIndex)
	{
		if (pickableIndex >= 0 && pickableIndex < AllLanguageShortNames[LocalizationManager.CurrentLanguage].Count)
		{
			return AllLanguageShortNames[LocalizationManager.CurrentLanguage][pickableIndex];
		}
		return null;
	}
}
