using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.BWF.Data;
using Crosstales.BWF.Provider;
using Crosstales.BWF.Util;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Filter;

public class DomainFilter : BaseFilter
{
	public string ReplaceCharacters;

	private List<DomainProvider> _domainProvider = new List<DomainProvider>();

	private readonly List<DomainProvider> _tempDomainProvider;

	private readonly Dictionary<string, Regex> _domainsRegex = new Dictionary<string, Regex>();

	private readonly Dictionary<string, List<Regex>> _debugDomainsRegex = new Dictionary<string, List<Regex>>();

	private bool _ready;

	private bool _readyFirstTime;

	public List<DomainProvider> DomainProvider
	{
		get
		{
			return _domainProvider;
		}
		set
		{
			_domainProvider = value;
			List<DomainProvider> domainProvider = _domainProvider;
			if (domainProvider != null && domainProvider.Count > 0)
			{
				foreach (DomainProvider item in _domainProvider)
				{
					if (item != null)
					{
						if (Config.DEBUG_DOMAINS)
						{
							_debugDomainsRegex.CTAddRange(item.DebugDomainsRegex);
						}
						else
						{
							_domainsRegex.CTAddRange(item.DomainsRegex);
						}
					}
					else if (!BaseHelper.isEditorMode)
					{
						Debug.LogError("DomainProvider is null!");
					}
				}
				return;
			}
			_domainProvider = new List<DomainProvider>();
		}
	}

	public override bool isReady
	{
		get
		{
			bool flag = true;
			if (!_ready)
			{
				List<DomainProvider> tempDomainProvider = _tempDomainProvider;
				if (tempDomainProvider != null && tempDomainProvider.Any((DomainProvider dp) => dp != null && !dp.isReady))
				{
					flag = false;
				}
				if (!_readyFirstTime && flag)
				{
					DomainProvider = _tempDomainProvider;
					if (DomainProvider != null)
					{
						foreach (Source item in from dp in DomainProvider
							where dp != null
							from src in dp.Sources
							where src != null
							where !_sources.ContainsKey(src.SourceName)
							select src)
						{
							_sources.Add(item.SourceName, item);
						}
					}
					_readyFirstTime = true;
				}
			}
			_ready = flag;
			return flag;
		}
	}

	public DomainFilter(List<DomainProvider> domainProvider, string replaceCharacters = "*", bool disableOrdering = false)
		: base(disableOrdering)
	{
		_tempDomainProvider = domainProvider;
		ReplaceCharacters = replaceCharacters;
	}

	public override bool Contains(string text, params string[] sourceNames)
	{
		bool result = false;
		if (isReady)
		{
			if (string.IsNullOrEmpty(text))
			{
				BaseFilter.logContains();
			}
			else if (Config.DEBUG_DOMAINS)
			{
				if (sourceNames == null || sourceNames.Length == 0)
				{
					foreach (List<Regex> value3 in _debugDomainsRegex.Values)
					{
						foreach (Regex item in value3)
						{
							Match match = item.Match(text);
							if (match.Success)
							{
								Debug.Log($"Test string contains a domain: '{match.Value}' detected by regex '{item}'");
								result = true;
								break;
							}
						}
					}
				}
				else
				{
					string[] array = sourceNames;
					foreach (string text2 in array)
					{
						if (_debugDomainsRegex.TryGetValue(text2, out var value))
						{
							foreach (Regex item2 in value)
							{
								Match match2 = item2.Match(text);
								if (match2.Success)
								{
									Debug.Log($"Test string contains a domain: '{match2.Value}' detected by regex '{item2}' from source '{text2}'");
									result = true;
									break;
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
				if (_domainsRegex.Values.Any((Regex domainRegex) => domainRegex.Match(text).Success))
				{
					result = true;
				}
			}
			else
			{
				string[] array = sourceNames;
				foreach (string text3 in array)
				{
					if (_domainsRegex.TryGetValue(text3, out var value2))
					{
						if (value2.Match(text).Success)
						{
							result = true;
							break;
						}
					}
					else
					{
						BaseFilter.logResourceNotFound(text3);
					}
				}
			}
		}
		else
		{
			BaseFilter.logFilterNotReady();
		}
		return result;
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
			else if (Config.DEBUG_DOMAINS)
			{
				if (sourceNames == null || sourceNames.Length == 0)
				{
					foreach (List<Regex> value3 in _debugDomainsRegex.Values)
					{
						foreach (Regex item in value3)
						{
							foreach (Capture item2 in from Match match in item.Matches(text)
								from capture in match.Captures.Cast<Capture>()
								select capture)
							{
								Debug.Log($"Test string contains a domain: '{item2.Value}' detected by regex '{item}'");
								if (!_getAllResult.Contains(item2.Value))
								{
									_getAllResult.Add(item2.Value);
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
						if (_debugDomainsRegex.TryGetValue(text2, out var value))
						{
							foreach (Regex item3 in value)
							{
								foreach (Capture item4 in from Match match in item3.Matches(text)
									from capture in match.Captures.Cast<Capture>()
									select capture)
								{
									Debug.Log($"Test string contains a domain: '{item4.Value}' detected by regex '{item3}'' from source '{text2}'");
									if (!_getAllResult.Contains(item4.Value))
									{
										_getAllResult.Add(item4.Value);
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
				foreach (Capture item5 in from domainRegex in _domainsRegex.Values
					select domainRegex.Matches(text) into matches
					from match in matches.Cast<Match>()
					from capture in match.Captures.Cast<Capture>()
					where !_getAllResult.Contains(capture.Value)
					select capture)
				{
					_getAllResult.Add(item5.Value);
				}
			}
			else
			{
				string[] array = sourceNames;
				foreach (string text3 in array)
				{
					if (_domainsRegex.TryGetValue(text3, out var value2))
					{
						foreach (Capture item6 in from Match match in value2.Matches(text)
							from capture in match.Captures.Cast<Capture>()
							where !_getAllResult.Contains(capture.Value)
							select capture)
						{
							_getAllResult.Add(item6.Value);
						}
					}
					else
					{
						BaseFilter.logResourceNotFound(text3);
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
		string text2 = text;
		if (isReady)
		{
			if (string.IsNullOrEmpty(text))
			{
				BaseFilter.logReplaceAll();
				text2 = string.Empty;
			}
			else if (Config.DEBUG_DOMAINS)
			{
				if (sourceNames == null || sourceNames.Length == 0)
				{
					foreach (List<Regex> value3 in _debugDomainsRegex.Values)
					{
						foreach (Regex item in value3)
						{
							foreach (Capture item2 in from Match match in item.Matches(text)
								from capture in match.Captures.Cast<Capture>()
								select capture)
							{
								Debug.Log($"Test string contains a domain: '{item2.Value}' detected by regex '{item}'");
								text2 = text2.Replace(item2.Value, markOnly ? (prefix + item2.Value + postfix) : (prefix + BaseHelper.CreateString(ReplaceCharacters, item2.Value.Length) + postfix));
							}
						}
					}
				}
				else
				{
					string[] array = sourceNames;
					foreach (string text3 in array)
					{
						if (_debugDomainsRegex.TryGetValue(text3, out var value))
						{
							foreach (Regex item3 in value)
							{
								foreach (Capture item4 in from Match match in item3.Matches(text)
									from capture in match.Captures.Cast<Capture>()
									select capture)
								{
									Debug.Log($"Test string contains a domain: '{item4.Value}' detected by regex '{item3}'' from source '{text3}'");
									text2 = text2.Replace(item4.Value, markOnly ? (prefix + item4.Value + postfix) : (prefix + BaseHelper.CreateString(ReplaceCharacters, item4.Value.Length) + postfix));
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
				text2 = (from domainRegex in _domainsRegex.Values
					from match in domainRegex.Matches(text).Cast<Match>()
					from capture in match.Captures.Cast<Capture>()
					select capture).Aggregate(text2, (string text5, Capture capture) => text5.Replace(capture.Value, markOnly ? (prefix + capture.Value + postfix) : (prefix + BaseHelper.CreateString(ReplaceCharacters, capture.Value.Length) + postfix)));
			}
			else
			{
				string[] array = sourceNames;
				foreach (string text4 in array)
				{
					if (_domainsRegex.TryGetValue(text4, out var value2))
					{
						text2 = (from Match match in value2.Matches(text)
							from capture in match.Captures.Cast<Capture>()
							select capture).Aggregate(text2, (string text5, Capture capture) => text5.Replace(capture.Value, markOnly ? (prefix + capture.Value + postfix) : (prefix + BaseHelper.CreateString(ReplaceCharacters, capture.Value.Length) + postfix)));
					}
					else
					{
						BaseFilter.logResourceNotFound(text4);
					}
				}
			}
		}
		else
		{
			BaseFilter.logFilterNotReady();
		}
		return text2;
	}
}
