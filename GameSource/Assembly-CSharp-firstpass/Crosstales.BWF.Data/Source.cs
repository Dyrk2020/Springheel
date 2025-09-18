using System;
using System.Text;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.BWF.Data;

[Serializable]
[CreateAssetMenu(fileName = "New Source", menuName = "Bad Word Filter PRO/Source", order = 1000)]
[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_data_1_1_source.html")]
public class Source : ScriptableObject
{
	[FormerlySerializedAs("Name")]
	[SerializeField]
	[Header("Information")]
	[Tooltip("Name of the source.")]
	private string sourceName = string.Empty;

	[SerializeField]
	[FormerlySerializedAs("Culture")]
	[Tooltip("Culture of the source (ISO 639-1).")]
	private string culture = string.Empty;

	[FormerlySerializedAs("Description")]
	[Tooltip("Description for the source (optional).")]
	[SerializeField]
	private string description = string.Empty;

	[FormerlySerializedAs("Icon")]
	[SerializeField]
	[Tooltip("Icon to represent the source (e.g. country flag, optional)")]
	private Sprite icon;

	[FormerlySerializedAs("URL")]
	[SerializeField]
	[Header("Settings")]
	[Tooltip("URL of a text file containing all regular expressions for this source. Add also the protocol-type ('http://', 'file://' etc.).")]
	private string url = string.Empty;

	[Tooltip("Text file containing all regular expressions for this source.")]
	[FormerlySerializedAs("Resource")]
	[SerializeField]
	private TextAsset resource;

	[Tooltip("Indicates if the 'Resource' is used as fallback in case the URL could not be loaded.")]
	[SerializeField]
	private bool isResourceFallback;

	public string SourceName
	{
		get
		{
			return sourceName;
		}
		set
		{
			sourceName = value;
		}
	}

	public string Culture
	{
		get
		{
			return culture;
		}
		set
		{
			culture = value;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	public Sprite Icon
	{
		get
		{
			return icon;
		}
		set
		{
			icon = value;
		}
	}

	public string URL
	{
		get
		{
			return url;
		}
		set
		{
			url = value;
		}
	}

	public TextAsset Resource
	{
		get
		{
			return resource;
		}
		set
		{
			resource = value;
		}
	}

	public bool IsResourceFallback
	{
		get
		{
			return isResourceFallback;
		}
		set
		{
			isResourceFallback = value;
		}
	}

	public int RegexCount
	{
		get
		{
			string[] regexes = Regexes;
			if (regexes == null)
			{
				return 0;
			}
			return regexes.Length;
		}
	}

	public string[] Regexes { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetType().Name);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_START);
		stringBuilder.Append("Name='");
		stringBuilder.Append(sourceName);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Culture='");
		stringBuilder.Append(culture);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Description='");
		stringBuilder.Append(description);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Icon='");
		stringBuilder.Append(icon);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("URL='");
		stringBuilder.Append(url);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Resource='");
		stringBuilder.Append(resource);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER_END);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_END);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Source source = (Source)obj;
		if (sourceName == source.sourceName && culture == source.culture && description == source.description && url == source.url)
		{
			return resource == source.resource;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (sourceName != null)
		{
			num += sourceName.GetHashCode();
		}
		if (culture != null)
		{
			num += culture.GetHashCode();
		}
		if (description != null)
		{
			num += description.GetHashCode();
		}
		if (url != null)
		{
			num += url.GetHashCode();
		}
		if (resource != null)
		{
			num += resource.GetHashCode();
		}
		return num;
	}
}
