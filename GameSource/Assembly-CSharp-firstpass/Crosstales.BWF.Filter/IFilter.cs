using System.Collections.Generic;
using Crosstales.BWF.Data;

namespace Crosstales.BWF.Filter;

public interface IFilter
{
	List<Source> Sources { get; }

	bool isReady { get; }

	bool Contains(string text, params string[] sourceNames);

	List<string> GetAll(string text, params string[] sourceNames);

	string ReplaceAll(string text, bool markOnly = false, string prefix = "", string postfix = "", params string[] sourceNames);

	string Mark(string text, bool replace = false, string prefix = "<b><color=red>", string postfix = "</color></b>", params string[] sourceNames);

	string Unmark(string text, string prefix = "<b><color=red>", string postfix = "</color></b>");
}
