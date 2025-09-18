using System;
using System.IO;

namespace BrainCloud.JsonFx.Json;

public class JsonDataReader : IDataReader
{
	public const string JsonMimeType = "application/json";

	public const string JsonFileExtension = ".json";

	private readonly JsonReaderSettings Settings;

	public string ContentType => "application/json";

	public JsonDataReader(JsonReaderSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		Settings = settings;
	}

	public object Deserialize(TextReader input, Type type)
	{
		return new JsonReader(input, Settings).Deserialize(type);
	}

	public static JsonReaderSettings CreateSettings(bool allowNullValueTypes)
	{
		return new JsonReaderSettings
		{
			AllowNullValueTypes = allowNullValueTypes
		};
	}
}
