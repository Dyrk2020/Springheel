using System.IO;
using Google.Protobuf;

namespace UCHServices;

public abstract class AbstractUchServiceRequestWithBody<T, Y> : AbstractUCHServiceRequest<Y> where T : IMessage<T>, new() where Y : IMessage<Y>, new()
{
	protected T mBody;

	protected override string EndpointURL => GameSettings.GetInstance().SelectedRegion.queryAddress;

	public override string HttpMethod => "POST";

	public AbstractUchServiceRequestWithBody(Service aService, T aBody)
		: base(aService)
	{
		mBody = aBody;
	}

	protected override string BodyToJson()
	{
		return new JsonFormatter(JsonFormatter.Settings.Default).Format(mBody);
	}

	protected override byte[] BodyToProtobuf()
	{
		using MemoryStream output = new MemoryStream();
		mBody.WriteTo(output);
		return mBody.ToByteArray();
	}
}
