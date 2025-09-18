using BCGSComponents.DataModels;

namespace BCGSComponents;

public class AcceptChallengeResponse : BCGSTypedResponse
{
	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public AcceptChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
