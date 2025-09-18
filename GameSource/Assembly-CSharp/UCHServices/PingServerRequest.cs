using Relay;

namespace UCHServices;

public class PingServerRequest : AbstractUchServiceRequestWithBody<PingRequest, PingResponse>
{
	private string serviceUrlOverride;

	public override string RequestEndpoint => "health/ping";

	protected override string EndpointURL => serviceUrlOverride;

	public PingServerRequest(Service aService, string aServiceUrlOverride)
		: base(aService, new PingRequest())
	{
		serviceUrlOverride = aServiceUrlOverride;
	}
}
