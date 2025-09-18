using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crosstales.BWF.Data;
using Crosstales.BWF.Manager;
using Crosstales.BWF.Model.Enum;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF;

[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_b_w_f_manager.html")]
[ExecuteInEditMode]
public class BWFManager : Singleton<BWFManager>
{
	public delegate void BWFReady();

	[Tooltip("Disables the ordering of the 'GetAll'-method (prevent possible memory garbage).")]
	public bool DisableOrdering;

	private bool _sentReady;

	private Thread _worker;

	private readonly List<string> _getAllResult = new List<string>();

	[Header("Events")]
	public OnReady OnReady;

	public OnContainsCompleted OnContainsCompleted;

	public OnGetAllCompleted OnGetAllCompleted;

	public OnReplaceAllCompleted OnReplaceAllCompleted;

	public bool isReady
	{
		get
		{
			if (Singleton<BadWordManager>.Instance != null && Singleton<BadWordManager>.Instance.isReady && Singleton<DomainManager>.Instance != null && Singleton<DomainManager>.Instance.isReady && Singleton<CapitalizationManager>.Instance != null && Singleton<CapitalizationManager>.Instance.isReady && Singleton<PunctuationManager>.Instance != null)
			{
				return Singleton<PunctuationManager>.Instance.isReady;
			}
			return false;
		}
	}

	public int TotalRegexCount => Sources().Sum((Source src) => src.RegexCount);

	public event BWFReady OnBWFReady;

	public event ContainsComplete OnContainsComplete;

	public event GetAllComplete OnGetAllComplete;

	public event ReplaceAllComplete OnReplaceAllComplete;

	protected override void OnApplicationQuit()
	{
		_worker.CTAbort();
		base.OnApplicationQuit();
	}

	private void Update()
	{
		if (!_sentReady && isReady)
		{
			_sentReady = true;
			onBWFReady();
		}
	}

	public void Load(ManagerMask mask = ManagerMask.All)
	{
		if ((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<BadWordManager>.Instance.Load();
		}
		if ((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<DomainManager>.Instance.Load();
		}
		if ((mask & ManagerMask.Capitalization) == ManagerMask.Capitalization || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<CapitalizationManager>.Instance.Load();
		}
		if ((mask & ManagerMask.Punctuation) == ManagerMask.Punctuation || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<PunctuationManager>.Instance.Load();
		}
	}

	public List<Source> Sources(ManagerMask mask = ManagerMask.All)
	{
		List<Source> list = new List<Source>(30);
		if ((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All)
		{
			list.AddRange(Singleton<BadWordManager>.Instance.Sources);
		}
		if ((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All)
		{
			list.AddRange(Singleton<DomainManager>.Instance.Sources);
		}
		return (from x in list.Distinct()
			orderby x.SourceName
			select x).ToList();
	}

	public bool Contains(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		contains(out var result, text, mask, sourceNames);
		onContainsComplete(text, result);
		return result;
	}

	public void ContainsAsync(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		StartCoroutine(containsAsync(text, mask, sourceNames));
	}

	public List<string> GetAll(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		getAll(out var result, text, mask, sourceNames);
		onGetAllComplete(text, result);
		return result;
	}

	public void GetAllAsync(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		StartCoroutine(getAllAsync(text, mask, sourceNames));
	}

	public string ReplaceAll(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		return ReplaceAll(text, mask, markOnly: false, string.Empty, string.Empty, sourceNames);
	}

	public string ReplaceAll(string text, ManagerMask mask, bool markOnly, string prefix, string postfix, params string[] sourceNames)
	{
		replaceAll(out var result, text, mask, markOnly, prefix, postfix, sourceNames);
		onReplaceAllComplete(text, result);
		return result;
	}

	public void ReplaceAllAsync(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		ReplaceAllAsync(text, mask, markOnly: false, string.Empty, string.Empty, sourceNames);
	}

	public void ReplaceAllAsync(string text, ManagerMask mask, bool markOnly, string prefix, string postfix, params string[] sourceNames)
	{
		StartCoroutine(replaceAllAsync(text, mask, markOnly, prefix, postfix, sourceNames));
	}

	public string Mark(string text, List<string> unwantedWords, string prefix = "<b><color=red>", string postfix = "</color></b>")
	{
		string text2 = text;
		string _prefix = prefix;
		string _postfix = postfix;
		if (string.IsNullOrEmpty(text))
		{
			if (BaseConstants.DEV_DEBUG)
			{
				Debug.LogWarning("Parameter 'text' is null or empty!" + Environment.NewLine + "=> 'Mark()' will return an empty string.", this);
			}
			text2 = string.Empty;
		}
		else
		{
			if (string.IsNullOrEmpty(prefix))
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.LogWarning("Parameter 'prefix' is null!" + Environment.NewLine + "=> Using an empty string as prefix.", this);
				}
				_prefix = string.Empty;
			}
			if (string.IsNullOrEmpty(postfix))
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.LogWarning("Parameter 'postfix' is null!" + Environment.NewLine + "=> Using an empty string as postfix.", this);
				}
				_postfix = string.Empty;
			}
			if (unwantedWords == null || unwantedWords.Count == 0)
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.LogWarning("Parameter 'unwantedWords' is null or empty!" + Environment.NewLine + "=> 'Mark()' will return the original string.", this);
				}
			}
			else
			{
				text2 = unwantedWords.Where((string unwantedWord) => !string.IsNullOrEmpty(unwantedWord)).Aggregate(text2, (string current, string unwantedWord) => current.Replace(unwantedWord, _prefix + unwantedWord + _postfix));
			}
		}
		return text2;
	}

	public string Mark(string text, bool replace = false, string prefix = "<b><color=red>", string postfix = "</color></b>", ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		string text2 = text ?? string.Empty;
		if ((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All)
		{
			text2 = Singleton<BadWordManager>.Instance.Mark(text2, replace, prefix, postfix, sourceNames);
		}
		if ((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All)
		{
			text2 = Singleton<DomainManager>.Instance.Mark(text2, replace, prefix, postfix, sourceNames);
		}
		if ((mask & ManagerMask.Capitalization) == ManagerMask.Capitalization || (mask & ManagerMask.All) == ManagerMask.All)
		{
			text2 = Singleton<CapitalizationManager>.Instance.Mark(text2, replace, prefix, postfix);
		}
		if ((mask & ManagerMask.Punctuation) == ManagerMask.Punctuation || (mask & ManagerMask.All) == ManagerMask.All)
		{
			text2 = Singleton<PunctuationManager>.Instance.Mark(text2, replace, prefix, postfix);
		}
		return text2;
	}

	public string Unmark(string text, string prefix = "<b><color=red>", string postfix = "</color></b>")
	{
		string text2 = text ?? string.Empty;
		return Singleton<BadWordManager>.Instance.Unmark(text2, prefix, postfix);
	}

	private IEnumerator containsAsync(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			bool result = true;
			_worker = new Thread((ThreadStart)delegate
			{
				contains(out result, text, mask, sourceNames);
			});
			_worker.Start();
			do
			{
				yield return null;
			}
			while (_worker.IsAlive);
			onContainsComplete(text, result);
		}
	}

	private IEnumerator getAllAsync(string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			List<string> result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				getAll(out result, text, mask, sourceNames);
			});
			_worker.Start();
			do
			{
				yield return null;
			}
			while (_worker.IsAlive);
			onGetAllComplete(text, result);
		}
	}

	private IEnumerator replaceAllAsync(string text, ManagerMask mask = ManagerMask.All, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			string result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				replaceAll(out result, text, mask, markOnly, prefix, postfix, sourceNames);
			});
			_worker.Start();
			do
			{
				yield return null;
			}
			while (_worker.IsAlive);
			onReplaceAllComplete(text, result);
		}
	}

	private static void contains(out bool result, string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		result = (((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All) && Singleton<BadWordManager>.Instance.Contains(text, sourceNames)) || (((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All) && Singleton<DomainManager>.Instance.Contains(text, sourceNames)) || (((mask & ManagerMask.Capitalization) == ManagerMask.Capitalization || (mask & ManagerMask.All) == ManagerMask.All) && Singleton<CapitalizationManager>.Instance.Contains(text)) || (((mask & ManagerMask.Punctuation) == ManagerMask.Punctuation || (mask & ManagerMask.All) == ManagerMask.All) && Singleton<PunctuationManager>.Instance.Contains(text));
	}

	private static void getAll(out List<string> result, string text, ManagerMask mask = ManagerMask.All, params string[] sourceNames)
	{
		Singleton<BWFManager>.Instance._getAllResult.Clear();
		if ((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<BadWordManager>.Instance.DisableOrdering = Singleton<BWFManager>.Instance.DisableOrdering;
			Singleton<BWFManager>.Instance._getAllResult.AddRange(Singleton<BadWordManager>.Instance.GetAll(text, sourceNames));
		}
		if ((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<DomainManager>.Instance.DisableOrdering = Singleton<BWFManager>.Instance.DisableOrdering;
			Singleton<BWFManager>.Instance._getAllResult.AddRange(Singleton<DomainManager>.Instance.GetAll(text, sourceNames));
		}
		if ((mask & ManagerMask.Capitalization) == ManagerMask.Capitalization || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<CapitalizationManager>.Instance.DisableOrdering = Singleton<BWFManager>.Instance.DisableOrdering;
			Singleton<BWFManager>.Instance._getAllResult.AddRange(Singleton<CapitalizationManager>.Instance.GetAll(text));
		}
		if ((mask & ManagerMask.Punctuation) == ManagerMask.Punctuation || (mask & ManagerMask.All) == ManagerMask.All)
		{
			Singleton<PunctuationManager>.Instance.DisableOrdering = Singleton<BWFManager>.Instance.DisableOrdering;
			Singleton<BWFManager>.Instance._getAllResult.AddRange(Singleton<PunctuationManager>.Instance.GetAll(text));
		}
		result = (Singleton<BWFManager>.Instance.DisableOrdering ? Singleton<BWFManager>.Instance._getAllResult : (from x in Singleton<BWFManager>.Instance._getAllResult.Distinct()
			orderby x
			select x).ToList());
	}

	private static void replaceAll(out string result, string text, ManagerMask mask = ManagerMask.All, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		result = text ?? string.Empty;
		if ((mask & ManagerMask.BadWord) == ManagerMask.BadWord || (mask & ManagerMask.All) == ManagerMask.All)
		{
			result = Singleton<BadWordManager>.Instance.ReplaceAll(result, markOnly, prefix, postfix, sourceNames);
		}
		if ((mask & ManagerMask.Domain) == ManagerMask.Domain || (mask & ManagerMask.All) == ManagerMask.All)
		{
			result = Singleton<DomainManager>.Instance.ReplaceAll(result, markOnly, prefix, postfix, sourceNames);
		}
		if ((mask & ManagerMask.Capitalization) == ManagerMask.Capitalization || (mask & ManagerMask.All) == ManagerMask.All)
		{
			result = Singleton<CapitalizationManager>.Instance.ReplaceAll(result, markOnly, prefix, postfix);
		}
		if ((mask & ManagerMask.Punctuation) == ManagerMask.Punctuation || (mask & ManagerMask.All) == ManagerMask.All)
		{
			result = Singleton<PunctuationManager>.Instance.ReplaceAll(result, markOnly, prefix, postfix);
		}
	}

	private void onBWFReady()
	{
		if (!BaseHelper.isEditorMode)
		{
			OnReady?.Invoke();
		}
		this.OnBWFReady?.Invoke();
	}

	private void onContainsComplete(string text, bool result)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnContainsCompleted?.Invoke(text, result);
		}
		this.OnContainsComplete?.Invoke(text, result);
	}

	private void onGetAllComplete(string text, List<string> badWords)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnGetAllCompleted?.Invoke(text, string.Join(";", badWords.ToArray()));
		}
		this.OnGetAllComplete?.Invoke(text, badWords);
	}

	private void onReplaceAllComplete(string originalText, string cleanText)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnReplaceAllCompleted?.Invoke(originalText, cleanText);
		}
		this.OnReplaceAllComplete?.Invoke(originalText, cleanText);
	}
}
