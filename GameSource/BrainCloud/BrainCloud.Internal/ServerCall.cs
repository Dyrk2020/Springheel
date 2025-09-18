using System.Collections;

namespace BrainCloud.Internal;

internal class ServerCall
{
	private ServerCallback m_callback;

	private IDictionary m_jsonData;

	private string m_operation;

	private string m_service;

	private string m_id;

	public string PacketID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public string Operation => GetOperation();

	public string Service => GetService();

	public ServerCall(ServiceName service, ServiceOperation operation, IDictionary jsonData, ServerCallback callback)
	{
		m_service = service.Value;
		m_operation = operation.Value;
		m_jsonData = jsonData;
		m_callback = callback;
	}

	public ServerCallback GetCallback()
	{
		return m_callback;
	}

	public string GetOperation()
	{
		return m_operation;
	}

	public string GetService()
	{
		return m_service;
	}

	public IDictionary GetJsonData()
	{
		return m_jsonData;
	}
}
