using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crosstales.BWF.Data;
using Crosstales.BWF.Filter;
using Crosstales.BWF.Model.Enum;
using Crosstales.BWF.Provider;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.BWF.Manager;

[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_manager_1_1_bad_word_manager.html")]
[DisallowMultipleComponent]
public class BadWordManager : BaseManager<BadWordManager, BadWordFilter>
{
	[Header("Specific Settings")]
	[SerializeField]
	[FormerlySerializedAs("ReplaceChars")]
	[Tooltip("Replace characters for bad words (default: *).")]
	private string replaceChars = "*";

	[Tooltip("Replace mode operations on the input string (default: Default).")]
	[SerializeField]
	private ReplaceMode mode;

	[SerializeField]
	[Tooltip("Remove unnecessary spaces between letters in the input string (default: false).")]
	private bool removeSpaces;

	[SerializeField]
	[Tooltip("Maximal text length for the space detection (default: 3).")]
	private int maxTextLength = 3;

	public string removeChars;

	[Tooltip("Use simple detection algorithm. This is the way to check for Chinese, Japanese, Korean and Thai bad words (default: false).")]
	[FormerlySerializedAs("SimpleCheck")]
	[SerializeField]
	private bool simpleCheck;

	[FormerlySerializedAs("BadWordProviderLTR")]
	[Tooltip("List of all left-to-right providers.")]
	[SerializeField]
	[Header("Bad Word Providers")]
	private List<BadWordProvider> badWordProviderLTR;

	[FormerlySerializedAs("BadWordProviderRTL")]
	[Tooltip("List of all right-to-left providers.")]
	[SerializeField]
	private List<BadWordProvider> badWordProviderRTL;

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

	public ReplaceMode Mode
	{
		get
		{
			return _filter?.Mode ?? mode;
		}
		set
		{
			_filter.Mode = (mode = value);
		}
	}

	public bool RemoveSpaces
	{
		get
		{
			return _filter?.RemoveSpaces ?? removeSpaces;
		}
		set
		{
			_filter.RemoveSpaces = (removeSpaces = value);
		}
	}

	public int MaxTextLength
	{
		get
		{
			return _filter?.MaxTextLength ?? maxTextLength;
		}
		set
		{
			_filter.MaxTextLength = (maxTextLength = value);
		}
	}

	public string RemoveChars
	{
		get
		{
			return _filter?.RemoveCharacters ?? removeChars;
		}
		set
		{
			_filter.RemoveCharacters = (removeChars = value);
		}
	}

	public bool SimpleCheck
	{
		get
		{
			return _filter?.SimpleCheck ?? simpleCheck;
		}
		set
		{
			_filter.SimpleCheck = (simpleCheck = value);
		}
	}

	public List<BadWordProvider> BadWordProviderLTR
	{
		get
		{
			return badWordProviderLTR;
		}
		set
		{
			badWordProviderLTR = value;
		}
	}

	public List<BadWordProvider> BadWordProviderRTL
	{
		get
		{
			return badWordProviderRTL;
		}
		set
		{
			badWordProviderRTL = value;
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
		if (Singleton<BadWordManager>.Instance == this)
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
		if (mode != Mode)
		{
			Mode = mode;
		}
		if (removeSpaces != RemoveSpaces)
		{
			RemoveSpaces = removeSpaces;
		}
		if (removeChars != RemoveChars)
		{
			RemoveChars = removeChars;
		}
		if (simpleCheck != SimpleCheck)
		{
			SimpleCheck = simpleCheck;
		}
	}

	protected override void OnApplicationQuit()
	{
		_worker.CTAbort();
		base.OnApplicationQuit();
	}

	public static void ResetObject()
	{
		Singleton<BadWordManager>.DeleteInstance();
	}

	public void Load()
	{
		_filter = new BadWordFilter(BadWordProviderLTR, BadWordProviderRTL, ReplaceChars, Mode, SimpleCheck, RemoveSpaces, base.DisableOrdering, RemoveChars);
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
