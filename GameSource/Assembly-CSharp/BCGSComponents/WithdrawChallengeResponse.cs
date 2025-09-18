using BCGSComponents.DataModels;

namespace BCGSComponents;

public class WithdrawChallengeResponse : BCGSTypedResponse
{
	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public WithdrawChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
