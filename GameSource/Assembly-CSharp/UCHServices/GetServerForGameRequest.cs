using Relay;

namespace UCHServices;

public class GetServerForGameRequest : AbstractUchServiceRequestWithBody<GetGameServerRequest, GetGameServerResponse>
{
	public override string RequestEndpoint => "relay/get-game-server";

	public GetServerForGameRequest(Service aService, string aGameId)
		: base(aService, new GetGameServerRequest
		{
			GameId = aGameId
		})
	{
	}
}
