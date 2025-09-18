using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WordFilter : MonoBehaviour
{
	public class WordFilterQuery
	{
		public string text;

		public UnityAction<string> OnFilteredTextReturned;
	}

	public class WordFilterBatch
	{
		public List<WordFilterQuery> queries = new List<WordFilterQuery>(128);
	}

	private static bool fakeWordFilterInEditor = false;

	private Text textComponent;

	private string lastCensoredText;

	private bool waitingForCensor;

	private float originalAlpha;

	public bool IgnoreInitialText;

	public List<string> Whitelist = new List<string>();

	private static WordFilterCache cache = new WordFilterCache();

	private static List<WordFilterQuery> batchedQueryList = new List<WordFilterQuery>(128);

	private static readonly char[] printfParams = new char[16]
	{
		'c', 'd', 'i', 'e', 'E', 'f', 'g', 'G', 'o', 's',
		'u', 'x', 'X', 'p', 'n', '%'
	};

	public static bool PlatformHasWordFilter => false;

	private void Start()
	{
		if (PlatformHasWordFilter)
		{
			textComponent = GetComponent<Text>();
			if (IgnoreInitialText)
			{
				lastCensoredText = textComponent.text;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void LateUpdate()
	{
		if (waitingForCensor)
		{
			if (textComponent.color.a != 0f)
			{
				textComponent.SetAlpha(0f);
			}
		}
		else
		{
			if (!(textComponent != null))
			{
				return;
			}
			if (!textComponent.text.NullOrEmpty() && !StringInWhitelist(textComponent.text))
			{
				if (lastCensoredText != null && !(lastCensoredText != textComponent.text))
				{
					return;
				}
				originalAlpha = textComponent.color.a;
				textComponent.SetAlpha(0f);
				waitingForCensor = true;
				FilterText(textComponent.text, delegate(string returnedString)
				{
					if (this != null)
					{
						lastCensoredText = returnedString;
						if (textComponent != null)
						{
							textComponent.text = returnedString;
							textComponent.SetAlpha(originalAlpha);
						}
						waitingForCensor = false;
					}
				});
			}
			else
			{
				lastCensoredText = null;
			}
		}
	}

	public bool StringInWhitelist(string str)
	{
		if (!Whitelist.Contains(str))
		{
			return str.Equals(LocalizationManager.GetTranslation("UndergroundComputer/Stats/Anonymous"));
		}
		return true;
	}

	public void FilterText(string text, UnityAction<string> OnFilteredTextReturned)
	{
		FilterText(WorkerThreadManager.Instance, text, OnFilteredTextReturned);
	}

	public static void FilterText(MonoBehaviour coroutineRunner, string text, UnityAction<string> OnFilteredTextReturned)
	{
		if (text.NullOrEmpty())
		{
			OnFilteredTextReturned(text);
			return;
		}
		string textOut = null;
		if (cache.TryGetCachedString(text, out textOut))
		{
			OnFilteredTextReturned(textOut);
		}
		else
		{
			OnFilteredTextReturned(text);
		}
	}

	private static IEnumerator FakeWordFilter(string text, UnityAction<string> OnFilteredTextReturned)
	{
		for (float timer = 0f; timer < 0.2f; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == 'a')
			{
				stringBuilder.Append('*');
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		OnFilteredTextReturned(stringBuilder.ToString());
	}

	public static void CachePreFilteredText(string textIn)
	{
		cache.CacheWordFilterResult(textIn, textIn);
	}

	private static void AddBatchedWordFilterQuery(string text, UnityAction<string> OnFilteredTextReturned)
	{
		batchedQueryList.Add(new WordFilterQuery
		{
			text = text,
			OnFilteredTextReturned = OnFilteredTextReturned
		});
	}

	public static void ProcessBatchedWordFilterQueue()
	{
		if (batchedQueryList.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<WordFilterBatch> list = new List<WordFilterBatch>();
		int num = 0;
		int length = Environment.NewLine.Length;
		int num2 = 1024;
		int num3 = 0;
		list.Add(new WordFilterBatch());
		for (int i = 0; i < batchedQueryList.Count; i++)
		{
			int num4 = batchedQueryList[i].text.Length + length;
			if (num4 > num2)
			{
				Debug.LogError("Skipped word filter query due to excessive length");
				continue;
			}
			if (num3 + num4 > num2)
			{
				FlushBatch(list[num], stringBuilder);
				list.Add(new WordFilterBatch());
				num++;
			}
			stringBuilder.AppendLine(batchedQueryList[i].text);
			num3 += num4;
			list[num].queries.Add(batchedQueryList[i]);
		}
		if (stringBuilder.Length > 0)
		{
			FlushBatch(list[num], stringBuilder);
		}
		batchedQueryList.Clear();
	}

	private static void FlushBatch(WordFilterBatch batch, StringBuilder sb)
	{
		Debug.LogError("ERROR! This platform doesn't have word filter batching.");
		sb.Length = 0;
	}

	private static void ProcessBatchedWordFilterResponse(WordFilterBatch batch, string filteredText)
	{
		int num = 0;
		foreach (WordFilterQuery query in batch.queries)
		{
			int length = query.text.Length;
			query.OnFilteredTextReturned(filteredText.Substring(num, length));
			num += length + Environment.NewLine.Length;
		}
	}
}
