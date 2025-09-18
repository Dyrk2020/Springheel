using System.Collections.Generic;

namespace BCGSComponents.DataModels;

public interface IBCGSData
{
	IDictionary<string, object> BaseData { get; }
}
