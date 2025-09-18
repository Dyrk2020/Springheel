using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class CancelBulkJobAdminResponse : BCGSTypedResponse
{
	public class _BulkJob : BCGSTypedResponse
	{
		public long? ActualCount => response.GetLong("actualCount");

		public DateTime? Completed => response.GetDate("completed");

		public DateTime? Created => response.GetDate("created");

		public BCGSData Data => response.GetObject("data");

		public long? DoneCount => response.GetLong("doneCount");

		public long? ErrorCount => response.GetLong("errorCount");

		public long? EstimatedCount => response.GetLong("estimatedCount");

		public string Id => response.GetString("id");

		public string ModuleShortCode => response.GetString("moduleShortCode");

		public BCGSData PlayerQuery => response.GetObject("playerQuery");

		public DateTime? ScheduledTime => response.GetDate("scheduledTime");

		public string Script => response.GetString("script");

		public DateTime? Started => response.GetDate("started");

		public string State => response.GetString("state");

		public _BulkJob(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_BulkJob> BulkJobs => new BCGSEnumerable<_BulkJob>(response.GetObjectList("bulkJobs"), (BCGSData data) => new _BulkJob(data));

	public CancelBulkJobAdminResponse(BCGSData data)
		: base(data)
	{
	}
}
