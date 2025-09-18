using System;
using System.IO;

namespace BrainCloud.JsonFx.Json;

public interface IDataReader
{
	string ContentType { get; }

	object Deserialize(TextReader input, Type data);
}
