using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ScheduleBulkJobAdminResponse : BCGSTypedResponse
{
	public long? EstimatedCount => response.GetLong("estimatedCount");

	public string JobId => response.GetString("jobId");

	public ScheduleBulkJobAdminResponse(BCGSData data)
		: base(data)
	{
	}
}
