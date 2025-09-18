using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ChatOnChallengeResponse : BCGSTypedResponse
{
	public string ChallengeInstanceId => response.GetString("challengeInstanceId");

	public ChatOnChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
