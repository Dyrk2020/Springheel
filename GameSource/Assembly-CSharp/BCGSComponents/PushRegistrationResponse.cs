using BCGSComponents.DataModels;

namespace BCGSComponents;

public class PushRegistrationResponse : BCGSTypedResponse
{
	public string RegistrationId => response.GetString("registrationId");

	public PushRegistrationResponse(BCGSData data)
		: base(data)
	{
	}
}
