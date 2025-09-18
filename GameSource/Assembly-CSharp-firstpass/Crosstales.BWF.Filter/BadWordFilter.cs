using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crosstales.BWF.Data;
using Crosstales.BWF.Model.Enum;
using Crosstales.BWF.Provider;
using Crosstales.BWF.Util;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Filter;

public class BadWordFilter : BaseFilter
{
	public string ReplaceCharacters;

	public ReplaceMode Mode;

	public bool RemoveSpaces;

	public int MaxTextLength = 3;

	public string RemoveCharacters;

	public bool SimpleCheck;

	private readonly List<BadWordProvider> _tempBadWordProviderLTR;

	private readonly List<BadWordProvider> _tempBadWordProviderRTL;

	private readonly Dictionary<string, Regex> _exactBadwordsRegex = new Dictionary<string, Regex>(30);

	private readonly Dictionary<string, List<Regex>> _debugExactBadwordsRegex = new Dictionary<string, List<Regex>>(30);

	private readonly Dictionary<string, List<string>> _simpleBadwords = new Dictionary<string, List<string>>(30);

	private bool _ready;

	private bool _readyFirstTime;

	private List<BadWordProvider> _badWordProviderLTR = new List<BadWordProvider>();

	private List<BadWordProvider> _badWordProviderRTL = new List<BadWordProvider>();

	public List<BadWordProvider> BadWordProviderLTR
	{
		get
		{
			return _badWordProviderLTR;
		}
		set
		{
			_badWordProviderLTR = value;
			List<BadWordProvider> badWordProviderLTR = _badWordProviderLTR;
			if (badWordProviderLTR != null && badWordProviderLTR.Count > 0)
			{
				foreach (BadWordProvider item in _badWordProviderLTR)
				{
					if (item != null)
					{
						if (Config.DEBUG_BADWORDS)
						{
							_debugExactBadwordsRegex.CTAddRange(item.DebugExactBadwordsRegex);
						}
						else
						{
							_exactBadwordsRegex.CTAddRange(item.ExactBadwordsRegex);
						}
						_simpleBadwords.CTAddRange(item.SimpleBadwords);
					}
					else if (!BaseHelper.isEditorMode)
					{
						Debug.LogError("A LTR-BadWordProvider is null!");
					}
				}
				return;
			}
			_badWordProviderLTR = new List<BadWordProvider>();
		}
	}

	public List<BadWordProvider> BadWordProviderRTL
	{
		get
		{
			return _badWordProviderRTL;
		}
		set
		{
			_badWordProviderRTL = value;
			List<BadWordProvider> badWordProviderRTL = _badWordProviderRTL;
			if (badWordProviderRTL != null && badWordProviderRTL.Count > 0)
			{
				foreach (BadWordProvider item in _badWordProviderRTL)
				{
					if (item != null)
					{
						if (Config.DEBUG_BADWORDS)
						{
							_debugExactBadwordsRegex.CTAddRange(item.DebugExactBadwordsRegex);
						}
						else
						{
							_exactBadwordsRegex.CTAddRange(item.ExactBadwordsRegex);
						}
						_simpleBadwords.CTAddRange(item.SimpleBadwords);
					}
					else if (!BaseHelper.isEditorMode)
					{
						Debug.LogError("A RTL-BadWordProvider is null!");
					}
				}
				return;
			}
			_badWordProviderRTL = new List<BadWordProvider>();
		}
	}

	public override bool isReady
	{
		get
		{
			bool flag = true;
			if (!_ready)
			{
				List<BadWordProvider> tempBadWordProviderLTR = _tempBadWordProviderLTR;
				if (tempBadWordProviderLTR != null && tempBadWordProviderLTR.Any((BadWordProvider bp) => bp != null && !bp.isReady))
				{
					flag = false;
				}
				if (flag)
				{
					List<BadWordProvider> tempBadWordProviderRTL = _tempBadWordProviderRTL;
					if (tempBadWordProviderRTL != null && tempBadWordProviderRTL.Any((BadWordProvider bp) => bp != null && !bp.isReady))
					{
						flag = false;
					}
				}
				if (!_readyFirstTime && flag)
				{
					BadWordProviderLTR = _tempBadWordProviderLTR;
					BadWordProviderRTL = _tempBadWordProviderRTL;
					if (BadWordProviderLTR != null)
					{
						foreach (Source item in from bpLTR in BadWordProviderLTR
							where bpLTR != null
							from src in bpLTR.Sources
							where src != null
							where !_sources.ContainsKey(src.SourceName)
							select src)
						{
							_sources.Add(item.SourceName, item);
						}
					}
					if (BadWordProviderRTL != null)
					{
						foreach (Source item2 in from bpRTL in BadWordProviderRTL
							where bpRTL != null
							from src in bpRTL.Sources
							where src != null
							where !_sources.ContainsKey(src.SourceName)
							select src)
						{
							_sources.Add(item2.SourceName, item2);
						}
					}
					_readyFirstTime = true;
				}
			}
			_ready = flag;
			return flag;
		}
	}

	public BadWordFilter(List<BadWordProvider> badWordProviderLTR, List<BadWordProvider> badWordProviderRTL, string replaceCharacters = "*", ReplaceMode mode = ReplaceMode.Default, bool simpleCheck = false, bool removeSpaces = false, bool disableOrdering = false, string removeCharacters = "")
		: base(disableOrdering)
	{
		_tempBadWordProviderLTR = badWordProviderLTR;
		_tempBadWordProviderRTL = badWordProviderRTL;
		ReplaceCharacters = replaceCharacters;
		Mode = mode;
		SimpleCheck = simpleCheck;
		RemoveSpaces = removeSpaces;
		DisableOrdering = disableOrdering;
		RemoveCharacters = removeCharacters;
	}

	public override bool Contains(string text, params string[] sourceNames)
	{
		bool flag = false;
		if (isReady)
		{
			if (string.IsNullOrEmpty(text))
			{
				BaseFilter.logContains();
			}
			else
			{
				string _text = replaceText(text);
				if (Config.DEBUG_BADWORDS)
				{
					if (sourceNames == null || sourceNames.Length == 0)
					{
						if (SimpleCheck)
						{
							foreach (List<string> value5 in _simpleBadwords.Values)
							{
								flag = value5.Any((string simpleWord) => _text.CTContains(simpleWord));
								if (flag)
								{
									Debug.Log("Test string contains a bad word.");
									break;
								}
							}
						}
						else
						{
							foreach (List<Regex> value6 in _debugExactBadwordsRegex.Values)
							{
								foreach (Regex item in value6)
								{
									Match match = item.Match(_text);
									if (match.Success)
									{
										Debug.Log($"Test string contains a bad word: '{match.Value}' detected by regex '{item}'");
										flag = true;
										break;
									}
								}
							}
						}
					}
					else
					{
						for (int num = 0; num < sourceNames.Length; num++)
						{
							if (flag)
							{
								break;
							}
							List<Regex> value2;
							if (SimpleCheck)
							{
								if (_simpleBadwords.TryGetValue(sourceNames[num], out var value))
								{
									flag = value.Any((string simpleWord) => _text.CTContains(simpleWord));
									if (flag)
									{
										Debug.Log("Test string contains a bad word from source '" + sourceNames[num] + "'");
										break;
									}
								}
								else
								{
									BaseFilter.logResourceNotFound(sourceNames[num]);
								}
							}
							else if (_debugExactBadwordsRegex.TryGetValue(sourceNames[num], out value2))
							{
								foreach (Regex item2 in value2)
								{
									Match match = item2.Match(_text);
									if (match.Success)
									{
										Debug.Log($"Test string contains a bad word: '{match.Value}' detected by regex '{item2}' from source '{sourceNames[num]}'");
										flag = true;
										break;
									}
								}
							}
							else
							{
								BaseFilter.logResourceNotFound(sourceNames[num]);
							}
						}
					}
				}
				else if (sourceNames == null || sourceNames.Length == 0)
				{
					if (SimpleCheck)
					{
						if (_simpleBadwords.Values.Any((List<string> words) => words.Any((string simpleWord) => _text.CTContains(simpleWord))))
						{
							flag = true;
						}
					}
					else if (_exactBadwordsRegex.Values.Any((Regex badWordRegex) => badWordRegex.Match(_text).Success))
					{
						flag = true;
					}
				}
				else
				{
					foreach (string text2 in sourceNames)
					{
						Regex value4;
						if (SimpleCheck)
						{
							if (_simpleBadwords.TryGetValue(text2, out var value3))
							{
								if (value3.Any((string simpleWord) => _text.CTContains(simpleWord)))
								{
									flag = true;
									break;
								}
							}
							else
							{
								BaseFilter.logResourceNotFound(text2);
							}
						}
						else if (_exactBadwordsRegex.TryGetValue(text2, out value4))
						{
							Match match = value4.Match(_text);
							if (match.Success)
							{
								flag = true;
								break;
							}
						}
						else
						{
							BaseFilter.logResourceNotFound(text2);
						}
					}
				}
			}
		}
		else
		{
			BaseFilter.logFilterNotReady();
		}
		return flag;
	}

	public override List<string> GetAll(string text, params string[] sourceNames)
	{
		_getAllResult.Clear();
		if (isReady)
		{
			if (string.IsNullOrEmpty(text))
			{
				BaseFilter.logGetAll();
			}
			else
			{
				string _text = replaceText(text);
				if (Config.DEBUG_BADWORDS)
				{
					if (sourceNames == null || sourceNames.Length == 0)
					{
						if (SimpleCheck)
						{
							foreach (string item in from words in _simpleBadwords.Values
								from simpleWord in words
								where _text.CTContains(simpleWord)
								select simpleWord)
							{
								Debug.Log("Test string contains a bad word detected by word '" + item + "'");
								if (!_getAllResult.Contains(item))
								{
									_getAllResult.Add(item);
								}
							}
						}
						else
						{
							foreach (List<Regex> value5 in _debugExactBadwordsRegex.Values)
							{
								foreach (Regex item2 in value5)
								{
									foreach (Capture item3 in from Match match in item2.Matches(_text)
										from capture in match.Captures.Cast<Capture>()
										select capture)
									{
										Debug.Log($"Test string contains a bad word: '{item3.Value}' detected by regex '{item2}'");
										if (!_getAllResult.Contains(item3.Value))
										{
											_getAllResult.Add(item3.Value);
										}
									}
								}
							}
						}
					}
					else
					{
						string[] array = sourceNames;
						foreach (string text2 in array)
						{
							List<Regex> value2;
							if (SimpleCheck)
							{
								if (_simpleBadwords.TryGetValue(text2, out var value))
								{
									foreach (string item4 in value.Where((string simpleWord) => _text.CTContains(simpleWord)))
									{
										Debug.Log("Test string contains a bad word detected by word '" + item4 + "' from source '" + text2 + "'");
										if (!_getAllResult.Contains(item4))
										{
											_getAllResult.Add(item4);
										}
									}
								}
								else
								{
									BaseFilter.logResourceNotFound(text2);
								}
							}
							else if (_debugExactBadwordsRegex.TryGetValue(text2, out value2))
							{
								foreach (Regex item5 in value2)
								{
									foreach (Capture item6 in from Match match in item5.Matches(_text)
										from capture in match.Captures.Cast<Capture>()
										select capture)
									{
										Debug.Log($"Test string contains a bad word: '{item6.Value}' detected by regex '{item5}' from source '{text2}'");
										if (!_getAllResult.Contains(item6.Value))
										{
											_getAllResult.Add(item6.Value);
										}
									}
								}
							}
							else
							{
								BaseFilter.logResourceNotFound(text2);
							}
						}
					}
				}
				else if (sourceNames == null || sourceNames.Length == 0)
				{
					if (SimpleCheck)
					{
						foreach (string item7 in from words in _simpleBadwords.Values
							from simpleWord in words
							where _text.CTContains(simpleWord)
							where !_getAllResult.Contains(simpleWord)
							select simpleWord)
						{
							_getAllResult.Add(item7);
						}
					}
					else
					{
						foreach (Capture item8 in from badWordsResource in _exactBadwordsRegex.Values
							select badWordsResource.Matches(_text) into matches
							from match in matches.Cast<Match>()
							from capture in match.Captures.Cast<Capture>()
							where !_getAllResult.Contains(capture.Value)
							select capture)
						{
							_getAllResult.Add(item8.Value);
						}
					}
				}
				else
				{
					string[] array = sourceNames;
					foreach (string text3 in array)
					{
						Regex value4;
						if (SimpleCheck)
						{
							if (_simpleBadwords.TryGetValue(text3, out var value3))
							{
								foreach (string item9 in from simpleWord in value3
									where _text.CTContains(simpleWord)
									where !_getAllResult.Contains(simpleWord)
									select simpleWord)
								{
									_getAllResult.Add(item9);
								}
							}
							else
							{
								BaseFilter.logResourceNotFound(text3);
							}
						}
						else if (_exactBadwordsRegex.TryGetValue(text3, out value4))
						{
							foreach (Capture item10 in from Match match in value4.Matches(_text)
								from capture in match.Captures.Cast<Capture>()
								where !_getAllResult.Contains(capture.Value)
								select capture)
							{
								_getAllResult.Add(item10.Value);
							}
						}
						else
						{
							BaseFilter.logResourceNotFound(text3);
						}
					}
				}
			}
		}
		else
		{
			BaseFilter.logFilterNotReady();
		}
		if (!DisableOrdering)
		{
			return (from x in _getAllResult.Distinct()
				orderby x
				select x).ToList();
		}
		return _getAllResult;
	}

	public override string ReplaceAll(string text, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		string text2 = string.Empty;
		bool flag = false;
		if (isReady)
		{
			if (string.IsNullOrEmpty(text))
			{
				BaseFilter.logReplaceAll();
			}
			else
			{
				string _text;
				text2 = (_text = replaceText(text));
				if (SimpleCheck)
				{
					foreach (string item in GetAll(_text, sourceNames))
					{
						_text = Regex.Replace(_text, item, BaseHelper.CreateString(ReplaceCharacters, item.Length), RegexOptions.IgnoreCase);
						flag = true;
					}
					text2 = _text;
				}
				else if (Config.DEBUG_BADWORDS)
				{
					if (sourceNames == null || sourceNames.Length == 0)
					{
						foreach (List<Regex> value3 in _debugExactBadwordsRegex.Values)
						{
							foreach (Regex item2 in value3)
							{
								foreach (Capture item3 in from Match match in item2.Matches(_text)
									from capture in match.Captures.Cast<Capture>()
									select capture)
								{
									Debug.Log($"Test string contains a bad word: '{item3.Value}' detected by regex '{item2}'");
									text2 = replaceCapture(text2, item3, markOnly, prefix, postfix, text2.Length - _text.Length);
									flag = true;
								}
							}
						}
					}
					else
					{
						string[] array = sourceNames;
						foreach (string text3 in array)
						{
							if (_debugExactBadwordsRegex.TryGetValue(text3, out var value))
							{
								foreach (Regex item4 in value)
								{
									foreach (Capture item5 in from Match match in item4.Matches(_text)
										from capture in match.Captures.Cast<Capture>()
										select capture)
									{
										Debug.Log($"Test string contains a bad word: '{item5.Value}' detected by regex '{item4}'' from source '{text3}'");
										text2 = replaceCapture(text2, item5, markOnly, prefix, postfix, text2.Length - _text.Length);
										flag = true;
									}
								}
							}
							else
							{
								BaseFilter.logResourceNotFound(text3);
							}
						}
					}
				}
				else if (sourceNames == null || sourceNames.Length == 0)
				{
					foreach (Capture item6 in from badWordsResource in _exactBadwordsRegex.Values
						select badWordsResource.Matches(_text) into matches
						from match in matches.Cast<Match>()
						from capture in match.Captures.Cast<Capture>()
						select capture)
					{
						text2 = replaceCapture(text2, item6, markOnly, prefix, postfix, text2.Length - _text.Length);
						flag = true;
					}
				}
				else
				{
					string[] array = sourceNames;
					foreach (string text4 in array)
					{
						if (_exactBadwordsRegex.TryGetValue(text4, out var value2))
						{
							foreach (Capture item7 in from Match match in value2.Matches(_text)
								from capture in match.Captures.Cast<Capture>()
								select capture)
							{
								text2 = replaceCapture(text2, item7, markOnly, prefix, postfix, text2.Length - _text.Length);
								flag = true;
							}
						}
						else
						{
							BaseFilter.logResourceNotFound(text4);
						}
					}
				}
			}
		}
		else
		{
			BaseFilter.logFilterNotReady();
		}
		if (!flag)
		{
			return text;
		}
		return text2;
	}

	private string replaceCapture(string text, Capture capture, bool markOnly, string prefix, string postfix, int offset)
	{
		StringBuilder stringBuilder = new StringBuilder(text);
		string value = (markOnly ? (prefix + capture.Value + postfix) : (prefix + BaseHelper.CreateString(ReplaceCharacters, capture.Value.Length) + postfix));
		stringBuilder.Remove(capture.Index + offset, capture.Value.Length);
		stringBuilder.Insert(capture.Index + offset, value);
		return stringBuilder.ToString();
	}

	protected string replaceText(string input)
	{
		string text = input;
		if (RemoveSpaces)
		{
			text = replaceSpacesBetweenLetters(text, MaxTextLength);
		}
		if (!string.IsNullOrEmpty(RemoveCharacters))
		{
			text = removeChars(text, RemoveCharacters);
		}
		switch (Mode)
		{
		case ReplaceMode.LeetSpeak:
			text = replaceLeetToText(text);
			break;
		case ReplaceMode.LeetSpeakAdvanced:
			text = replaceLeetAdvancedToText(text);
			text = replaceLeetToText(text);
			break;
		case ReplaceMode.NonLettersOrDigits:
			text = replaceNonLettersOrDigits(text);
			break;
		}
		return text;
	}

	private static string replaceNonLettersOrDigits(string input)
	{
		return new string(Array.FindAll(input.ToCharArray(), (char c) => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == ',' || c == '?' || c == '!' || c == '-' || c == ';' || c == ':' || c == '"' || c == '\'' || c == '.'));
	}

	private static string replaceSpacesBetweenLetters(string text, int maxTextLength = 4)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		string[] array = text.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			if (text2.Length <= maxTextLength)
			{
				stringBuilder.Append(text2);
				for (int j = i + 1; j < array.Length; j++)
				{
					string text3 = array[j];
					if (text3.Length > maxTextLength)
					{
						break;
					}
					i = j;
					stringBuilder.Append(text3);
				}
			}
			else
			{
				stringBuilder.Append(text2);
			}
			if (i < array.Length - 1)
			{
				stringBuilder.Append(" ");
			}
		}
		return stringBuilder.ToString();
	}

	private static string removeChars(string input, string removeChars)
	{
		return input.CTRemoveChars(removeChars.ToCharArray());
	}

	private static string replaceLeetToText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return input.Replace("@", "a").Replace("4", "a").Replace("^", "a")
			.Replace("8", "b")
			.Replace("©", "c")
			.Replace('¢', 'c')
			.Replace("€", "e")
			.Replace("3", "e")
			.Replace("£", "e")
			.Replace("ƒ", "f")
			.Replace("6", "g")
			.Replace("9", "g")
			.Replace("#", "h")
			.Replace("1", "i")
			.Replace("!", "i")
			.Replace("|", "i")
			.Replace("0", "o")
			.Replace("2", "r")
			.Replace("®", "r")
			.Replace("$", "s")
			.Replace("5", "s")
			.Replace("§", "s")
			.Replace("7", "t")
			.Replace("+", "t")
			.Replace("†", "t")
			.Replace("¥", "y");
	}

	private static string replaceLeetAdvancedToText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return input.Replace("|-|", "h").Replace("}{", "h").Replace("]-[", "h")
			.Replace("/-/", "h")
			.Replace(")-(", "h")
			.Replace("][", "i")
			.Replace("|<", "k")
			.Replace("|{", "k")
			.Replace("|(", "k")
			.Replace("|_", "l")
			.Replace("][_", "l")
			.Replace("/\\/\\", "m")
			.Replace("/v\\", "m")
			.Replace("|V|", "m")
			.Replace("]V[", "m")
			.Replace("|\\/|", "m")
			.Replace("|\\|", "n")
			.Replace("/\\/", "n")
			.Replace("/V", "n")
			.Replace("()", "o")
			.Replace("|°", "p")
			.Replace("|>", "p")
			.Replace("µ", "u")
			.Replace("|_|", "u")
			.Replace("\\/\\/", "w")
			.Replace("\\/", "v")
			.Replace("><", "x")
			.Replace(")(", "x")
			.Replace("/\\", "a")
			.Replace("/-\\", "a");
	}
}
