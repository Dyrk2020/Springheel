using Relay;

namespace UCHServices;

public class GetIpRequest : AbstractUchServiceRequestWithBody<Relay.GetIpRequest, GetIpResponse>
{
	public override string RequestEndpoint => "health/get-ip";

	public GetIpRequest(Service aService)
		: base(aService, new Relay.GetIpRequest())
	{
	}
}
