using System;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class ListTransactionsResponse : BCGSTypedResponse
{
	public class _PlayerTransaction : BCGSTypedResponse
	{
		public class _PlayerTransactionItem : BCGSTypedResponse
		{
			public long? Amount => response.GetLong("amount");

			public long? NewValue => response.GetLong("newValue");

			public string Type => response.GetString("type");

			public _PlayerTransactionItem(BCGSData data)
				: base(data)
			{
			}
		}

		public BCGSEnumerable<_PlayerTransactionItem> Items => new BCGSEnumerable<_PlayerTransactionItem>(response.GetObjectList("items"), (BCGSData data) => new _PlayerTransactionItem(data));

		public string OriginalRequestId => response.GetString("originalRequestId");

		public string PlayerId => response.GetString("playerId");

		public string Reason => response.GetString("reason");

		public DateTime? RevokeDate => response.GetDate("revokeDate");

		public bool? Revoked => response.GetBoolean("revoked");

		public string Script => response.GetString("script");

		public string ScriptType => response.GetString("scriptType");

		public string TransactionId => response.GetString("transactionId");

		public DateTime? When => response.GetDate("when");

		public _PlayerTransaction(BCGSData data)
			: base(data)
		{
		}
	}

	public BCGSEnumerable<_PlayerTransaction> TransactionList => new BCGSEnumerable<_PlayerTransaction>(response.GetObjectList("transactionList"), (BCGSData data) => new _PlayerTransaction(data));

	public ListTransactionsResponse(BCGSData data)
		: base(data)
	{
	}
}
