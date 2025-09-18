using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crosstales.BWF.Data;
using Crosstales.BWF.Filter;
using Crosstales.BWF.Provider;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.BWF.Manager;

[DisallowMultipleComponent]
[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_manager_1_1_domain_manager.html")]
public class DomainManager : BaseManager<DomainManager, DomainFilter>
{
	[FormerlySerializedAs("ReplaceChars")]
	[SerializeField]
	[Tooltip("Replace characters for domains (default: *).")]
	[Header("Specific Settings")]
	private string replaceChars = "*";

	[SerializeField]
	[Tooltip("List of all domain providers.")]
	[Header("Domain Providers")]
	[FormerlySerializedAs("DomainProvider")]
	private List<DomainProvider> domainProvider;

	[Header("Events")]
	public OnContainsCompleted OnContainsCompleted;

	public OnGetAllCompleted OnGetAllCompleted;

	public OnReplaceAllCompleted OnReplaceAllCompleted;

	private Thread _worker;

	public string ReplaceChars
	{
		get
		{
			return _filter?.ReplaceCharacters ?? replaceChars;
		}
		set
		{
			_filter.ReplaceCharacters = (replaceChars = value);
		}
	}

	public List<DomainProvider> DomainProvider
	{
		get
		{
			return domainProvider;
		}
		set
		{
			domainProvider = value;
		}
	}

	public List<Source> Sources => _filter?.Sources;

	public int TotalRegexCount => Sources.Sum((Source src) => src.RegexCount);

	protected override OnContainsCompleted onContainsCompleted => OnContainsCompleted;

	protected override OnGetAllCompleted onGetAllCompleted => OnGetAllCompleted;

	protected override OnReplaceAllCompleted onReplaceAllCompleted => OnReplaceAllCompleted;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<DomainManager>.Instance == this)
		{
			Load();
		}
	}

	private void OnValidate()
	{
		if (replaceChars != ReplaceChars)
		{
			ReplaceChars = replaceChars;
		}
	}

	protected override void OnApplicationQuit()
	{
		_worker.CTAbort();
		base.OnApplicationQuit();
	}

	public static void ResetObject()
	{
		Singleton<DomainManager>.DeleteInstance();
	}

	public void Load()
	{
		_filter = new DomainFilter(domainProvider, replaceChars, base.DisableOrdering);
	}

	public bool Contains(string text, params string[] sourceNames)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(text) && _filter != null)
		{
			result = _filter.Contains(text, sourceNames);
		}
		return result;
	}

	public void ContainsAsync(string text, params string[] sourceNames)
	{
		StartCoroutine(containsAsync(text, sourceNames));
	}

	public List<string> GetAll(string text, params string[] sourceNames)
	{
		List<string> result = new List<string>();
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.GetAll(text, sourceNames);
		}
		return result;
	}

	public void GetAllAsync(string text, params string[] sourceNames)
	{
		StartCoroutine(getAllAsync(text, sourceNames));
	}

	public string ReplaceAll(string text, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		string result = text;
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.ReplaceAll(text, markOnly, prefix, postfix, sourceNames);
		}
		return result;
	}

	public void ReplaceAllAsync(string text, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		StartCoroutine(replaceAllAsync(text, markOnly, prefix, postfix, sourceNames));
	}

	public string Mark(string text, bool replace = false, string prefix = "<b><color=red>", string postfix = "</color></b>", params string[] sourceNames)
	{
		string result = text;
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.Mark(text, replace, prefix, postfix, sourceNames);
		}
		return result;
	}

	private IEnumerator containsAsync(string text, params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			bool result = true;
			_worker = new Thread((ThreadStart)delegate
			{
				result = Contains(text, sourceNames);
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

	private IEnumerator getAllAsync(string text, params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			List<string> result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				result = GetAll(text, sourceNames);
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

	private IEnumerator replaceAllAsync(string text, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			string result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				result = ReplaceAll(text, markOnly, prefix, postfix, sourceNames);
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
}
