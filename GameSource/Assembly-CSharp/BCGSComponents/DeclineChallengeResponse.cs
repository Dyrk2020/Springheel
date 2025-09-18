using BCGSComponents.DataModels;

namespace BCGSComponents;

public class DeclineChallengeResponse : BCGSTypedResponse
{
	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public DeclineChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
