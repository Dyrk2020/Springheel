using BCGSComponents.DataModels;

namespace BCGSComponents;

public class CreateChallengeResponse : BCGSTypedResponse
{
	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public CreateChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
