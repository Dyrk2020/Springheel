using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.BWF.Model;
using Crosstales.BWF.Util;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Provider;

public abstract class BadWordProvider : BaseProvider
{
	protected readonly List<BadWords> _badwords = new List<BadWords>();

	private Dictionary<string, Regex> _exactBadwordsRegex = new Dictionary<string, Regex>();

	private Dictionary<string, List<Regex>> _debugExactBadwordsRegex = new Dictionary<string, List<Regex>>();

	private Dictionary<string, List<string>> _simpleBadwords = new Dictionary<string, List<string>>();

	private const string EXACT_REGEX_START = "(?<![\\w\\d])";

	private const string EXACT_REGEX_END = "s?(?![\\w\\d])";

	public Dictionary<string, Regex> ExactBadwordsRegex
	{
		get
		{
			return _exactBadwordsRegex;
		}
		protected set
		{
			_exactBadwordsRegex = value;
		}
	}

	public Dictionary<string, List<Regex>> DebugExactBadwordsRegex
	{
		get
		{
			return _debugExactBadwordsRegex;
		}
		protected set
		{
			_debugExactBadwordsRegex = value;
		}
	}

	public Dictionary<string, List<string>> SimpleBadwords
	{
		get
		{
			return _simpleBadwords;
		}
		protected set
		{
			_simpleBadwords = value;
		}
	}

	private void Start()
	{
	}

	public override void Load()
	{
		if (ClearOnLoad)
		{
			_badwords.Clear();
		}
	}

	protected override void init()
	{
		ExactBadwordsRegex.Clear();
		DebugExactBadwordsRegex.Clear();
		SimpleBadwords.Clear();
		if (Config.DEBUG_BADWORDS)
		{
			Debug.Log("++ BadWordProvider started in debug-mode ++", this);
		}
		foreach (BadWords badword in _badwords)
		{
			if (Config.DEBUG_BADWORDS)
			{
				try
				{
					List<Regex> list = new List<Regex>(badword.BadWordList.Count);
					list.AddRange(badword.BadWordList.Select((string line) => new Regex("(?<![\\w\\d])" + line + "s?(?![\\w\\d])", RegexOption1 | RegexOption2 | RegexOption3 | RegexOption4 | RegexOption5)));
					if (!DebugExactBadwordsRegex.ContainsKey(badword.Source.SourceName))
					{
						DebugExactBadwordsRegex.Add(badword.Source.SourceName, list);
					}
				}
				catch (Exception arg)
				{
					Debug.LogError($"Could not generate debug regex for source '{badword.Source.SourceName}': {arg}", this);
					if (BaseConstants.DEV_DEBUG)
					{
						Debug.Log(badword.BadWordList.CTDump(), this);
					}
				}
			}
			else
			{
				try
				{
					if (!ExactBadwordsRegex.ContainsKey(badword.Source.SourceName))
					{
						ExactBadwordsRegex.Add(badword.Source.SourceName, new Regex("(?<![\\w\\d])(" + string.Join("|", badword.BadWordList.ToArray()) + ")s?(?![\\w\\d])", RegexOption1 | RegexOption2 | RegexOption3 | RegexOption4 | RegexOption5));
					}
				}
				catch (Exception arg2)
				{
					Debug.LogError($"Could not generate exact regex for source '{badword.Source.SourceName}': {arg2}", this);
					if (BaseConstants.DEV_DEBUG)
					{
						Debug.Log(badword.BadWordList.CTDump(), this);
					}
				}
			}
			List<string> list2 = new List<string>(badword.BadWordList.Count);
			list2.AddRange(badword.BadWordList);
			if (!SimpleBadwords.ContainsKey(badword.Source.SourceName))
			{
				SimpleBadwords.Add(badword.Source.SourceName, list2);
			}
			if (Config.DEBUG_BADWORDS)
			{
				Debug.Log($"Bad word resource '{badword.Source.SourceName}' loaded and {badword.BadWordList.Count} entries found.", this);
			}
		}
		base.isReady = true;
	}
}
