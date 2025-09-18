namespace nn.audio;

public static class AudioDevice
{
	public enum AudioDeviceName
	{
		ExternalOutput,
		BuiltInSpeakerOutput,
		TvOutput
	}

	public const float OutputVolumeMax = 128f;

	public const float OutputVolumeMin = 0f;

	public static bool SetOutputVolume(AudioDeviceName deviceName, float volume)
	{
		return false;
	}

	public static float GetOutputVolume(AudioDeviceName deviceName)
	{
		return 0f;
	}
}
