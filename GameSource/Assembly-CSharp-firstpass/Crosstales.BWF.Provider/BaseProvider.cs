using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.BWF.Data;
using Crosstales.BWF.Util;
using UnityEngine;

namespace Crosstales.BWF.Provider;

[ExecuteInEditMode]
public abstract class BaseProvider : MonoBehaviour, IProvider
{
	[Header("Regex Options")]
	[Tooltip("Option1 (default: RegexOptions.IgnoreCase).")]
	public RegexOptions RegexOption1 = RegexOptions.IgnoreCase;

	[Tooltip("Option2 (default: RegexOptions.CultureInvariant).")]
	public RegexOptions RegexOption2 = RegexOptions.CultureInvariant;

	[Tooltip("Option3 (default: RegexOptions.None).")]
	public RegexOptions RegexOption3;

	[Tooltip("Option4 (default: RegexOptions.None).")]
	public RegexOptions RegexOption4;

	[Tooltip("Option5 (default: RegexOptions.None).")]
	public RegexOptions RegexOption5;

	[Tooltip("All sources for this provider.")]
	[Header("Sources")]
	[ContextMenuItem("Create Source", "createSource")]
	public List<Source> Sources;

	[Header("Load Behaviour")]
	[Tooltip("Clears all existing bad words on 'Load' (default: true).")]
	public bool ClearOnLoad = true;

	protected readonly List<string> coRoutines = new List<string>();

	protected bool _loading;

	public int RegexCount => Sources?.Sum((Source src) => (src != null) ? src.RegexCount : 0) ?? 0;

	public bool isReady { get; set; }

	public abstract void Load();

	public abstract void Save();

	public List<string> Verify(Source source)
	{
		List<string> list = new List<string>();
		if (source != null && source.RegexCount > 0)
		{
			string[] regexes = source.Regexes;
			foreach (string text in regexes)
			{
				try
				{
					new Regex(text, RegexOption1 | RegexOption2 | RegexOption3 | RegexOption4 | RegexOption5);
				}
				catch (Exception arg)
				{
					list.Add($"Error in regex '{text}' from source '{source.SourceName}': {arg}");
				}
			}
		}
		else
		{
			list.Add("Source contains no regexes!");
		}
		return list;
	}

	protected abstract void init();

	private void Awake()
	{
		Load();
	}

	protected void logNoResourcesAdded()
	{
		Debug.LogWarning("No 'Resources' for " + base.name + " added!" + Environment.NewLine + "If you want to use this functionality, please add your desired 'Resources'.", this);
	}

	protected void createSource()
	{
		Helper.CreateSource();
	}
}
