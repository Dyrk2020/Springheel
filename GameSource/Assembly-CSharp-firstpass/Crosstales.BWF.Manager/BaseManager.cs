using System.Collections.Generic;
using Crosstales.BWF.Filter;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Manager;

[ExecuteInEditMode]
public abstract class BaseManager<S, T> : Singleton<S> where S : Singleton<S> where T : BaseFilter
{
	[SerializeField]
	[Tooltip("Disables the ordering of the 'GetAll'-method (prevent possible memory garbage).")]
	private bool disableOrdering;

	protected T _filter;

	public bool DisableOrdering
	{
		get
		{
			return disableOrdering;
		}
		set
		{
			if (_filter != null)
			{
				disableOrdering = value;
				_filter.DisableOrdering = disableOrdering;
			}
		}
	}

	public bool isReady
	{
		get
		{
			if (_filter != null)
			{
				return _filter.isReady;
			}
			return false;
		}
	}

	protected abstract OnContainsCompleted onContainsCompleted { get; }

	protected abstract OnGetAllCompleted onGetAllCompleted { get; }

	protected abstract OnReplaceAllCompleted onReplaceAllCompleted { get; }

	public event ContainsComplete OnContainsComplete;

	public event GetAllComplete OnGetAllComplete;

	public event ReplaceAllComplete OnReplaceAllComplete;

	private void Start()
	{
	}

	public string Unmark(string text, string prefix = "<b><color=red>", string postfix = "</color></b>")
	{
		string result = text;
		if (!string.IsNullOrEmpty(text) && _filter != null)
		{
			result = _filter.Unmark(text, prefix, postfix);
		}
		return result;
	}

	protected void onContainsComplete(string text, bool result)
	{
		if (!BaseHelper.isEditorMode)
		{
			onContainsCompleted?.Invoke(text, result);
		}
		this.OnContainsComplete?.Invoke(text, result);
	}

	protected void onGetAllComplete(string text, List<string> badWords)
	{
		if (!BaseHelper.isEditorMode)
		{
			onGetAllCompleted?.Invoke(text, string.Join(";", badWords.ToArray()));
		}
		this.OnGetAllComplete?.Invoke(text, badWords);
	}

	protected void onReplaceAllComplete(string originalText, string cleanText)
	{
		if (!BaseHelper.isEditorMode)
		{
			onReplaceAllCompleted?.Invoke(originalText, cleanText);
		}
		this.OnReplaceAllComplete?.Invoke(originalText, cleanText);
	}
}
