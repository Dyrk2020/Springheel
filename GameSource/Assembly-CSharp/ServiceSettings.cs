using UnityEngine;

public class ServiceSettings
{
	[Tooltip("Weither we should be using JSON to communicate with the server. Defaults to Protobuf if this is false.")]
	public bool consumesJson;

	[Tooltip("Endpoint for the service endpoint.")]
	public string endpointURL = "127.0.0.1";

	[Tooltip("Port to use for HTTP requests and Websocket connection.")]
	public int connectionPort = 443;

	[Tooltip("Are we using SSL to encrypt our communications? Will use wss and https connections if true.")]
	public bool useSSL = true;
}
