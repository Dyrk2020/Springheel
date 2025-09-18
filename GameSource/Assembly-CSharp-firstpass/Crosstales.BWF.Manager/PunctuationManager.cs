using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Crosstales.BWF.Filter;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.BWF.Manager;

[DisallowMultipleComponent]
[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_manager_1_1_punctuation_manager.html")]
public class PunctuationManager : BaseManager<PunctuationManager, PunctuationFilter>
{
	[Tooltip("Defines the number of allowed punctuation letters in a row (default: 3).")]
	[FormerlySerializedAs("PunctuationCharsNumber")]
	[Header("Specific Settings")]
	[SerializeField]
	private int punctuationCharsNumber = 3;

	[Header("Events")]
	public OnContainsCompleted OnContainsCompleted;

	public OnGetAllCompleted OnGetAllCompleted;

	public OnReplaceAllCompleted OnReplaceAllCompleted;

	private Thread _worker;

	public int PunctuationCharsNumber
	{
		get
		{
			return _filter?.CharacterNumber ?? punctuationCharsNumber;
		}
		set
		{
			_filter.CharacterNumber = (punctuationCharsNumber = ((value < 1) ? 1 : value));
		}
	}

	protected override OnContainsCompleted onContainsCompleted => OnContainsCompleted;

	protected override OnGetAllCompleted onGetAllCompleted => OnGetAllCompleted;

	protected override OnReplaceAllCompleted onReplaceAllCompleted => OnReplaceAllCompleted;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<PunctuationManager>.Instance == this)
		{
			Load();
		}
	}

	protected override void OnApplicationQuit()
	{
		_worker.CTAbort();
		base.OnApplicationQuit();
	}

	private void OnValidate()
	{
		if (punctuationCharsNumber < 1)
		{
			punctuationCharsNumber = 1;
		}
		if (punctuationCharsNumber != PunctuationCharsNumber)
		{
			PunctuationCharsNumber = punctuationCharsNumber;
		}
	}

	public static void ResetObject()
	{
		Singleton<PunctuationManager>.DeleteInstance();
	}

	public void Load()
	{
		_filter = new PunctuationFilter(punctuationCharsNumber, base.DisableOrdering);
	}

	public bool Contains(string text)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(text) && _filter != null)
		{
			result = _filter.Contains(text);
		}
		return result;
	}

	public void ContainsAsync(string text)
	{
		StartCoroutine(containsAsync(text));
	}

	public List<string> GetAll(string text)
	{
		List<string> result = new List<string>();
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.GetAll(text);
		}
		return result;
	}

	public void GetAllAsync(string text)
	{
		StartCoroutine(getAllAsync(text));
	}

	public string ReplaceAll(string text, bool markOnly = false, string prefix = "", string postfix = "")
	{
		string result = text;
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.ReplaceAll(text, markOnly, prefix, postfix);
		}
		return result;
	}

	public void ReplaceAllAsync(string text, bool markOnly = false, string prefix = "", string postfix = "")
	{
		StartCoroutine(replaceAllAsync(text, markOnly, prefix, postfix));
	}

	public string Mark(string text, bool replace = false, string prefix = "<b><color=red>", string postfix = "</color></b>")
	{
		string result = text;
		if (!string.IsNullOrEmpty(text))
		{
			result = _filter?.Mark(text, replace, prefix, postfix);
		}
		return result;
	}

	private IEnumerator containsAsync(string text)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			bool result = true;
			_worker = new Thread((ThreadStart)delegate
			{
				result = Contains(text);
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

	private IEnumerator getAllAsync(string text)
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			List<string> result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				result = GetAll(text);
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

	private IEnumerator replaceAllAsync(string text, bool markOnly = false, string prefix = "", string postfix = "")
	{
		Thread worker = _worker;
		if (worker == null || !worker.IsAlive)
		{
			string result = null;
			_worker = new Thread((ThreadStart)delegate
			{
				result = ReplaceAll(text, markOnly, prefix, postfix);
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
