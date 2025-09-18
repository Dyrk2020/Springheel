namespace BrainCloud.Internal;

internal class EndOfBundleMarker : ServerCall
{
	public EndOfBundleMarker()
		: base(ServiceName.HeartBeat, ServiceOperation.Send, null, null)
	{
	}
}
