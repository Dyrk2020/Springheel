using Relay;

namespace UCHServices;

public class GetNextAvailableServerRequest : AbstractUchServiceRequestWithBody<Relay.GetNextAvailableServerRequest, GetNextAvailableServerResponse>
{
	public override string RequestEndpoint => "relay/get-next-available";

	public GetNextAvailableServerRequest(Service aService)
		: base(aService, new Relay.GetNextAvailableServerRequest())
	{
	}
}
