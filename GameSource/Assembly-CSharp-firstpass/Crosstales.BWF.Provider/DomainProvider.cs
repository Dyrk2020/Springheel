using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.BWF.Model;
using Crosstales.BWF.Util;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Provider;

public abstract class DomainProvider : BaseProvider
{
	protected readonly List<Domains> _domains = new List<Domains>();

	private Dictionary<string, Regex> _domainsRegex = new Dictionary<string, Regex>();

	private Dictionary<string, List<Regex>> _debugDomainsRegex = new Dictionary<string, List<Regex>>();

	private const string DOMAIN_REGEGX_START = "\\b{0,1}((ht|f)tp(s?)\\:\\/\\/)?[\\w\\-\\.\\@]*[\\.]";

	private const string DOMAIN_REGEGX_END = "(:\\d{1,5})?(\\/|\\b)";

	public Dictionary<string, Regex> DomainsRegex
	{
		get
		{
			return _domainsRegex;
		}
		protected set
		{
			_domainsRegex = value;
		}
	}

	public Dictionary<string, List<Regex>> DebugDomainsRegex
	{
		get
		{
			return _debugDomainsRegex;
		}
		protected set
		{
			_debugDomainsRegex = value;
		}
	}

	private void Start()
	{
	}

	public override void Load()
	{
		if (ClearOnLoad)
		{
			_domains.Clear();
		}
	}

	protected override void init()
	{
		DomainsRegex.Clear();
		if (Config.DEBUG_DOMAINS)
		{
			Debug.Log("++ DomainProvider started in debug-mode ++", this);
		}
		foreach (Domains domain in _domains)
		{
			if (Config.DEBUG_DOMAINS)
			{
				try
				{
					List<Regex> list = new List<Regex>(domain.DomainList.Count);
					list.AddRange(domain.DomainList.Select((string line) => new Regex("\\b{0,1}((ht|f)tp(s?)\\:\\/\\/)?[\\w\\-\\.\\@]*[\\.]" + line + "(:\\d{1,5})?(\\/|\\b)", RegexOption1 | RegexOption2 | RegexOption3 | RegexOption4 | RegexOption5)));
					if (!DebugDomainsRegex.ContainsKey(domain.Source.SourceName))
					{
						DebugDomainsRegex.Add(domain.Source.SourceName, list);
					}
				}
				catch (Exception arg)
				{
					Debug.LogError($"Could not generate debug regex for source '{domain.Source.SourceName}': {arg}", this);
					if (BaseConstants.DEV_DEBUG)
					{
						Debug.Log(domain.DomainList.CTDump(), this);
					}
				}
			}
			else
			{
				try
				{
					if (!DomainsRegex.ContainsKey(domain.Source.SourceName))
					{
						DomainsRegex.Add(domain.Source.SourceName, new Regex("\\b{0,1}((ht|f)tp(s?)\\:\\/\\/)?[\\w\\-\\.\\@]*[\\.](" + string.Join("|", domain.DomainList.ToArray()) + ")(:\\d{1,5})?(\\/|\\b)", RegexOption1 | RegexOption2 | RegexOption3 | RegexOption4 | RegexOption5));
					}
				}
				catch (Exception arg2)
				{
					Debug.LogError($"Could not generate exact regex for source '{domain.Source.SourceName}': {arg2}", this);
					if (BaseConstants.DEV_DEBUG)
					{
						Debug.Log(domain.DomainList.CTDump(), this);
					}
				}
			}
			if (Config.DEBUG_DOMAINS)
			{
				Debug.Log($"Domain resource '{domain.Source}' loaded and {domain.DomainList.Count} entries found.", this);
			}
		}
		base.isReady = true;
	}
}
