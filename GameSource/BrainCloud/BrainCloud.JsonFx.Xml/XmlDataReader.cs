using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.JsonFx.Xml;

public class XmlDataReader : IDataReader
{
	public const string XmlMimeType = "application/xml";

	private readonly XmlReaderSettings Settings;

	private readonly XmlSerializerNamespaces Namespaces;

	public string ContentType => "application/xml";

	public XmlDataReader(XmlReaderSettings settings, XmlSerializerNamespaces namespaces)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		Settings = settings;
		if (namespaces == null)
		{
			namespaces = new XmlSerializerNamespaces();
			namespaces.Add(string.Empty, string.Empty);
		}
		Namespaces = namespaces;
	}

	public object Deserialize(TextReader input, Type type)
	{
		XmlReader xmlReader = XmlReader.Create(input, Settings);
		xmlReader.MoveToContent();
		return new XmlSerializer(type).Deserialize(xmlReader);
	}

	public static XmlReaderSettings CreateSettings()
	{
		return new XmlReaderSettings
		{
			CloseInput = false,
			ConformanceLevel = ConformanceLevel.Auto,
			IgnoreComments = true,
			IgnoreWhitespace = true,
			IgnoreProcessingInstructions = true
		};
	}
}
