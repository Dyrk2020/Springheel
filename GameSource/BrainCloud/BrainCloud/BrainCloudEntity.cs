using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudEntity
{
	private BrainCloudClient _client;

	public BrainCloudEntity(BrainCloudClient brainCloudClient)
	{
		_client = brainCloudClient;
	}

	public void CreateEntity(string entityType, string jsonEntityData, string jsonEntityAcl, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.EntityServiceData.Value] = value;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.EntityServiceAcl.Value] = value2;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.Create, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEntitiesByType(string entityType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.ReadByType, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateEntity(string entityId, string entityType, string jsonEntityData, string jsonEntityAcl, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.EntityServiceData.Value] = value;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.EntityServiceAcl.Value] = value2;
		}
		dictionary[OperationParam.EntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.Update, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateSharedEntity(string entityId, string targetProfileId, string entityType, string jsonEntityData, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.EntityServiceTargetPlayerId.Value] = targetProfileId;
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.EntityServiceData.Value] = value;
		dictionary[OperationParam.EntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.UpdateShared, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteEntity(string entityId, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.EntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.Delete, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateSingleton(string entityType, string jsonEntityData, string jsonEntityAcl, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityData);
		dictionary[OperationParam.EntityServiceData.Value] = value;
		if (Util.IsOptionalParameterValid(jsonEntityAcl))
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>(jsonEntityAcl);
			dictionary[OperationParam.EntityServiceAcl.Value] = value2;
		}
		dictionary[OperationParam.EntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.UpdateSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteSingleton(string entityType, int version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		dictionary[OperationParam.EntityServiceVersion.Value] = version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.DeleteSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetEntity(string entityId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.Read, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSingleton(string entityType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityType.Value] = entityType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.ReadSingleton, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSharedEntityForProfileId(string profileId, string entityId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceTargetPlayerId.Value] = profileId;
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.ReadSharedEntity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSharedEntitiesForProfileId(string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceTargetPlayerId.Value] = profileId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.ReadShared, dictionary, callback);
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
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.GetList, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetSharedEntitiesListForProfileId(string profileId, string whereJson, string orderByJson, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceTargetPlayerId.Value] = profileId;
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
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.ReadSharedEntitiesList, dictionary, callback);
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
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.GetListCount, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPage(string jsonContext, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonContext);
		dictionary[OperationParam.GlobalEntityServiceContext.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.GetPage, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPageOffset(string context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GlobalEntityServiceContext.Value] = context;
		dictionary[OperationParam.GlobalEntityServicePageOffset.Value] = pageOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.GetPageOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementUserEntityData(string entityId, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.EntityServiceData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.IncrementUserEntityData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void IncrementSharedUserEntityData(string entityId, string targetProfileId, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EntityServiceEntityId.Value] = entityId;
		dictionary[OperationParam.EntityServiceTargetPlayerId.Value] = targetProfileId;
		if (Util.IsOptionalParameterValid(jsonData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
			dictionary[OperationParam.EntityServiceData.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Entity, ServiceOperation.IncrementSharedUserEntityData, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
