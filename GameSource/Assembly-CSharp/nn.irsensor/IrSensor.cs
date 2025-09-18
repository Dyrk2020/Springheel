namespace nn.irsensor;

public static class IrSensor
{
	public static ErrorRange ResultIrsensorUnavailable => new ErrorRange(205, 110, 120);

	public static ErrorRange ResultIrsensorUnconnected => new ErrorRange(205, 110, 111);

	public static ErrorRange ResultIrsensorUnsupported => new ErrorRange(205, 111, 112);

	public static ErrorRange ResultIrsensorDeviceError => new ErrorRange(205, 122, 140);

	public static ErrorRange ResultIrsensorFirmwareCheckIncompleted => new ErrorRange(205, 150, 151);

	public static ErrorRange ResultIrsensorNotReady => new ErrorRange(205, 160, 170);

	public static ErrorRange ResultIrsensorDeviceNotReady => new ErrorRange(205, 160, 161);

	public static ErrorRange ResultIrsensorDeviceResourceNotAvailable => new ErrorRange(205, 161, 162);

	public static ErrorRange ResultHandAnalysisError => new ErrorRange(205, 1100, 1200);

	public static ErrorRange ResultHandAnalysisModeIncorrect => new ErrorRange(205, 1101, 1102);
}
