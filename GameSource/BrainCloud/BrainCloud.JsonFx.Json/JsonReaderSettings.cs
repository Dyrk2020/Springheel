using System;

namespace BrainCloud.JsonFx.Json;

public class JsonReaderSettings
{
	internal readonly TypeCoercionUtility Coercion = new TypeCoercionUtility();

	private bool allowUnquotedObjectKeys;

	private string typeHintName;

	public bool AllowNullValueTypes
	{
		get
		{
			return Coercion.AllowNullValueTypes;
		}
		set
		{
			Coercion.AllowNullValueTypes = value;
		}
	}

	public bool AllowUnquotedObjectKeys
	{
		get
		{
			return allowUnquotedObjectKeys;
		}
		set
		{
			allowUnquotedObjectKeys = value;
		}
	}

	public string TypeHintName
	{
		get
		{
			return typeHintName;
		}
		set
		{
			typeHintName = value;
		}
	}

	internal bool IsTypeHintName(string name)
	{
		if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(typeHintName))
		{
			return StringComparer.Ordinal.Equals(typeHintName, name);
		}
		return false;
	}
}
