using System.Collections.Generic;
using UnityEngine;

public class WordFilterCache
{
	public class FilterCacheEntry
	{
		public string textIn;

		public string textOut;

		public float lastUsed;
	}

	private Dictionary<string, FilterCacheEntry> cachedEntries = new Dictionary<string, FilterCacheEntry>();

	private const int MaxCachedEntries = 256;

	private const int MaxCharsToCache = 32;

	public void CacheWordFilterResult(string textIn, string textOut)
	{
		if (textIn.Length > 32)
		{
			return;
		}
		FilterCacheEntry value = null;
		if (cachedEntries.TryGetValue(textIn, out value))
		{
			value.lastUsed = Time.time;
			return;
		}
		cachedEntries.Add(textIn, new FilterCacheEntry
		{
			textIn = textIn,
			textOut = textOut,
			lastUsed = Time.time
		});
		if (cachedEntries.Count > 256)
		{
			ClearOldestEntry();
		}
	}

	public bool TryGetCachedString(string textIn, out string textOut)
	{
		if (textIn.Length > 32)
		{
			textOut = null;
			return false;
		}
		FilterCacheEntry value = null;
		if (cachedEntries.TryGetValue(textIn, out value))
		{
			textOut = value.textOut;
			value.lastUsed = Time.time;
			return true;
		}
		textOut = null;
		return false;
	}

	private void ClearOldestEntry()
	{
		if (cachedEntries.Count == 0)
		{
			return;
		}
		FilterCacheEntry filterCacheEntry = null;
		foreach (KeyValuePair<string, FilterCacheEntry> cachedEntry in cachedEntries)
		{
			if (filterCacheEntry == null)
			{
				filterCacheEntry = cachedEntry.Value;
			}
			else if (cachedEntry.Value.lastUsed < filterCacheEntry.lastUsed)
			{
				filterCacheEntry = cachedEntry.Value;
			}
		}
		if (filterCacheEntry != null)
		{
			cachedEntries.Remove(filterCacheEntry.textIn);
		}
	}
}
