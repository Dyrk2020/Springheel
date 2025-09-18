using System;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudCustomEntity
{
	private BrainCloudClient _client;

	public BrainCloudCustomEntity(BrainCloudClient client)
	{
		_client = client;
	}

	public void CreateEntity(string entityType, string dataJson, string acl, string timeToLive, bool isOwned, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceDataJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(dataJson);
		dictionary[OperationParam.CustomEntityServiceAcl.Value] = JsonReader.Deserialize<Dictionary<string, object>>(acl);
		dictionary[OperationParam.CustomEntityServiceTimeToLive.Value] = timeToLive;
		dictionary[OperationParam.CustomEntityServiceIsOwned.Value] = isOwned;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.CreateCustomEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated use overload with 2 arguments entityType and context - removal after September 1 2021")]
	public void GetEntityPage(string entityType, int rowsPerPage, string searchJson, string sortJson, bool doCount, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceRowsPerPage.Value] = rowsPerPage;
		dictionary[OperationParam.CustomEntityServiceSearchJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(searchJson);
		dictionary[OperationParam.CustomEntityServiceSortJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(sortJson);
		dictionary[OperationParam.CustomEntityServiceDoCount.Value] = doCount;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.GetCustomEntityPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEntityPage(string entityType, string jsonContext, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonContext);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceContext.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.GetEntityPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEntityPageOffset(string entityType, string context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceContext.Value] = context;
		dictionary[OperationParam.CustomEntityServicePageOffset.Value] = pageOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.GetCustomEntityPageOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadEntity(string entityType, string entityId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.ReadCustomEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementData(string entityType, string entityId, string fieldsJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.CustomEntityServiceFieldsJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fieldsJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.IncrementData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntity(string entityType, string entityId, int version, string dataJson, string acl, string timeToLive, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		dictionary[OperationParam.CustomEntityServiceDataJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(dataJson);
		dictionary[OperationParam.CustomEntityServiceAcl.Value] = JsonReader.Deserialize<Dictionary<string, object>>(acl);
		dictionary[OperationParam.CustomEntityServiceTimeToLive.Value] = timeToLive;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.UpdateCustomEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityFields(string entityType, string entityId, int version, string fieldsJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		dictionary[OperationParam.CustomEntityServiceFieldsJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fieldsJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.UpdateCustomEntityFields, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntityFieldsSharded(string entityType, string entityId, int version, string fieldsJson, string shardKeyJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		dictionary[OperationParam.CustomEntityServiceFieldsJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fieldsJson);
		dictionary[OperationParam.CustomEntityServiceShardKeyJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(shardKeyJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.UpdateCustomEntityFieldsShards, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteEntities(string entityType, string deleteCriteria, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceDeleteCriteria.Value] = JsonReader.Deserialize<Dictionary<string, object>>(deleteCriteria);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.DeleteEntities, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetCount(string entityType, string whereJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceWhereJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(whereJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.GetCount, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteEntity(string entityType, string entityId, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.DeleteCustomEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRandomEntitiesMatching(string entityType, string whereJson, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceWhereJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(whereJson);
		dictionary[OperationParam.CustomEntityServiceMaxReturn.Value] = maxReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.GetRandomEntitiesMatching, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteSingleton(string entityType, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.DeleteSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ReadSingleton(string entityType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.ReadSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateSingletonFields(string entityType, int version, string fieldsJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		dictionary[OperationParam.CustomEntityServiceFieldsJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fieldsJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.UpdateSingletonFields, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateSingleton(string entityType, int version, string dataJson, string acl, string timeToLive, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.CustomEntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.CustomEntityServiceVersion.Value] = version;
		dictionary[OperationParam.CustomEntityServiceDataJson.Value] = JsonReader.Deserialize<Dictionary<string, object>>(dataJson);
		dictionary[OperationParam.CustomEntityServiceAcl.Value] = JsonReader.Deserialize<Dictionary<string, object>>(acl);
		dictionary[OperationParam.CustomEntityServiceTimeToLive.Value] = timeToLive;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.CustomEntity, ServiceOperation.UpdateSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
