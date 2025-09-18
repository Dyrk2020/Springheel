using System.Text;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using UnityEngine.Networking;

namespace UCHServices;

public abstract class AbstractUCHServiceRequest<T> where T : IMessage<T>, new()
{
	protected Service mService;

	private string mUrl;

	protected virtual string EndpointURL => mService.Settings.endpointURL;

	protected virtual int ConnectionPort => mService.Settings.connectionPort;

	public string Url
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(mUrl))
			{
				return mUrl;
			}
			mUrl = BuildCallURL();
			return mUrl;
		}
	}

	public abstract string RequestEndpoint { get; }

	public virtual string HttpMethod => "GET";

	public virtual string ContentType
	{
		get
		{
			if (!mService.Settings.consumesJson)
			{
				return "application/x-protobuf";
			}
			return "application/json";
		}
	}

	public AbstractUCHServiceRequest(Service aService)
	{
		mService = aService;
	}

	protected void SetContentTypeHeader(UnityWebRequest aRequest)
	{
		aRequest.SetRequestHeader("Content-Type", ContentType);
	}

	private string BuildCallURL()
	{
		ServiceSettings settings = mService.Settings;
		return string.Concat("http" + (settings.useSSL ? "s" : "") + "://" + EndpointURL + ":" + ConnectionPort + "/", RequestEndpoint);
	}

	public UnityWebRequest GenerateRequest()
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(Url, HttpMethod);
		UploadHandler uploadHandler = GenerateUploadHandler();
		if (uploadHandler != null)
		{
			unityWebRequest.uploadHandler = uploadHandler;
		}
		DownloadHandler downloadHandler = GenerateDownloadHandler();
		if (downloadHandler != null)
		{
			unityWebRequest.downloadHandler = downloadHandler;
		}
		SetContentTypeHeader(unityWebRequest);
		return unityWebRequest;
	}

	private UploadHandler GenerateUploadHandler()
	{
		byte[] array;
		if (mService.Settings.consumesJson)
		{
			string text = BodyToJson();
			if (text.Length <= 2)
			{
				return null;
			}
			array = Encoding.UTF8.GetBytes(text);
		}
		else
		{
			array = BodyToProtobuf();
			if (array.Length == 0)
			{
				return null;
			}
		}
		return new UploadHandlerRaw(array)
		{
			contentType = ContentType
		};
	}

	protected virtual string BodyToJson()
	{
		return string.Empty;
	}

	protected virtual byte[] BodyToProtobuf()
	{
		return new byte[0];
	}

	protected virtual DownloadHandler GenerateDownloadHandler()
	{
		return new DownloadHandlerBuffer();
	}

	public async UniTask<T> SendAsync<T>() where T : IMessage<T>, new()
	{
		return await HttpRequestCaller.AsyncSendHttpRequest(this as AbstractUCHServiceRequest<T>);
	}
}
