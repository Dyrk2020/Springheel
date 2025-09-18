using System.Collections.Generic;

namespace BrainCloud.Internal;

internal class JsonResponseBundleV2
{
	public long packetId;

	public Dictionary<string, object>[] responses;

	public Dictionary<string, object>[] events;
}
