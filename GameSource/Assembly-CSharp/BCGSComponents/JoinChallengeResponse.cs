using BCGSComponents.DataModels;

namespace BCGSComponents;

public class JoinChallengeResponse : BCGSTypedResponse
{
	public bool? Joined => response.GetBoolean("joined");

	public JoinChallengeResponse(BCGSData data)
		: base(data)
	{
	}
}
