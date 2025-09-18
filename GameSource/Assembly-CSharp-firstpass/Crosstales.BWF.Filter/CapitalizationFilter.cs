using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Filter;

public class CapitalizationFilter : BaseFilter
{
	private int _characterNumber;

	public Regex RegularExpression { get; private set; }

	public int CharacterNumber
	{
		get
		{
			return _characterNumber;
		}
		set
		{
			_characterNumber = ((value < 1) ? 1 : value);
			RegularExpression = new Regex($"\\b\\w*[A-ZÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝ]{{{_characterNumber + 1},}}\\w*\\b", RegexOptions.CultureInvariant);
		}
	}

	public override bool isReady => true;

	public CapitalizationFilter(int capitalizationCharsNumber = 3, bool disableOrdering = false)
		: base(disableOrdering)
	{
		CharacterNumber = capitalizationCharsNumber;
	}

	public override bool Contains(string text, params string[] sourceNames)
	{
		bool result = false;
		if (string.IsNullOrEmpty(text))
		{
			BaseFilter.logContains();
		}
		else
		{
			result = RegularExpression.Match(text).Success;
		}
		return result;
	}

	public override List<string> GetAll(string text, params string[] sourceNames)
	{
		_getAllResult.Clear();
		if (string.IsNullOrEmpty(text))
		{
			BaseFilter.logGetAll();
		}
		else
		{
			foreach (Capture item in from Match match in RegularExpression.Matches(text)
				from capture in match.Captures.Cast<Capture>()
				select capture)
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.Log("Test string contains an excessive capital word: '" + item.Value + "'");
				}
				if (!_getAllResult.Contains(item.Value))
				{
					_getAllResult.Add(item.Value);
				}
			}
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
		if (string.IsNullOrEmpty(text))
		{
			BaseFilter.logReplaceAll();
			text2 = string.Empty;
		}
		else
		{
			foreach (Capture item in from Match match in RegularExpression.Matches(text)
				from capture in match.Captures.Cast<Capture>()
				select capture)
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.Log("Test string contains an excessive capital word: '" + item.Value + "'");
				}
				text2 = text2.Replace(item.Value, markOnly ? (prefix + item.Value + postfix) : (prefix + item.Value.ToLowerInvariant() + postfix));
			}
		}
		return text2;
	}
}
