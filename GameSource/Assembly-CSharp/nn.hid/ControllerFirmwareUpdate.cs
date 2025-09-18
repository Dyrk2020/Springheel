namespace nn.hid;

public static class ControllerFirmwareUpdate
{
	public static ErrorRange ResultControllerFirmwareUpdateError => new ErrorRange(202, 3200, 3210);

	public static ErrorRange ResultControllerFirmwareUpdateFailed => new ErrorRange(202, 3201, 3202);

	public static Result Show(ControllerFirmwareUpdateArg showControllerFirmwareUpdateArg)
	{
		return default(Result);
	}

	public static Result Show(ControllerFirmwareUpdateArg showControllerFirmwareUpdateArg, bool suspendUnityThreads)
	{
		return Show(showControllerFirmwareUpdateArg);
	}
}
