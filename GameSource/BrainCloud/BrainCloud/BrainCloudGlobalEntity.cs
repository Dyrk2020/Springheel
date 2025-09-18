using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudGlobalEntity
{
	private BrainCloudClient _client;

	public BrainCloudGlobalEntity(BrainCloudClient client)
	{
		_client = client;
	}

	public void CreateEntity(string entityType, long timeToLive, string jsonEntityAcl, string jsonEntityData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.GlobalEntityServiceTimeToLive.Value] = timeToLive;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.GlobalEntityServiceData.Value] = value;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.GlobalEntityServiceAcl.Value] = value2;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.Create, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void CreateEntityWithIndexedId(string entityType, string indexedId, long timeToLive, string jsonEntityAcl, string jsonEntityData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.GlobalEntityServiceIndexedId.Value] = indexedId;
		dictionary[OperationParam.GlobalEntityServiceTimeToLive.Value] = timeToLive;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.GlobalEntityServiceData.Value] = value;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.GlobalEntityServiceAcl.Value] = value2;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.CreateWithIndexedId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntity(string entityId, int version, string jsonEntityData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.GlobalEntityServiceData.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.Update, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityAcl(string entityId, int version, string jsonEntityAcl, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.GlobalEntityServiceAcl.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.UpdateAcl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityTimeToLive(string entityId, int version, long timeToLive, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		dictionary[OperationParam.GlobalEntityServiceTimeToLive.Value] = timeToLive;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.UpdateTimeToLive, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteEntity(string entityId, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.Delete, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadEntity(string entityId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.Read, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetList(string whereJson, string orderByJson, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(whereJson))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(whereJson);
			dictionary[OperationParam.GlobalEntityServiceWhere.Value] = value;
		}
		if (Util.IsOptionalParameterValid(orderByJson))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(orderByJson);
			dictionary[OperationParam.GlobalEntityServiceOrderBy.Value] = value2;
		}
		dictionary[OperationParam.GlobalEntityServiceMaxReturn.Value] = maxReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetList, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetListByIndexedId(string entityIndexedId, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceIndexedId.Value] = entityIndexedId;
		dictionary[OperationParam.GlobalEntityServiceMaxReturn.Value] = maxReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetListByIndexedId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetListCount(string whereJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(whereJson))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(whereJson);
			dictionary[OperationParam.GlobalEntityServiceWhere.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetListCount, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPage(string jsonContext, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonContext);
		dictionary[OperationParam.GlobalEntityServiceContext.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPageOffset(string context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceContext.Value] = context;
		dictionary[OperationParam.GlobalEntityServicePageOffset.Value] = pageOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetPageOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementGlobalEntityData(string entityId, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.GlobalEntityServiceData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.IncrementGlobalEntityData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRandomEntitiesMatching(string whereJson, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(whereJson))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(whereJson);
			dictionary[OperationParam.GlobalEntityServiceWhere.Value] = value;
		}
		dictionary[OperationParam.GlobalEntityServiceMaxReturn.Value] = maxReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.GetRandomEntitiesMatching, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityIndexedId(string entityId, long version, string entityIndexedId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		dictionary[OperationParam.GlobalEntityServiceIndexedId.Value] = entityIndexedId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.UpdateEntityIndexedId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityOwnerAndAcl(string entityId, long version, string ownerId, ACL acl, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		dictionary[OperationParam.OwnerId.Value] = ownerId;
		dictionary[OperationParam.GlobalEntityServiceAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.UpdateEntityOwnerAndAcl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void MakeSystemEntity(string entityId, long version, ACL acl, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.GlobalEntityServiceVersion.Value] = version;
		dictionary[OperationParam.GlobalEntityServiceAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.GlobalEntity, ServiceOperation.MakeSystemEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
