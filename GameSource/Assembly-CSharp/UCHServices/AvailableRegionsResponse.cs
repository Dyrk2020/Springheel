using System;
using System.Collections.Generic;

namespace UCHServices;

[Serializable]
public class AvailableRegionsResponse
{
	public List<AvailableRegion> availableRegions = new List<AvailableRegion>();
}
