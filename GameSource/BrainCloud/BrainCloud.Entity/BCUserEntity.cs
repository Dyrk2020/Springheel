using System;
using System.Collections.Generic;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Entity;

public class BCUserEntity : BCEntity
{
	public BCUserEntity(BrainCloudEntity in_bcEntityService)
		: base(in_bcEntityService)
	{
		m_bcEntityService = in_bcEntityService;
	}

	public void CbCreateSuccess(string jsonString, object cbObject)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)JsonReader.Deserialize<Dictionary<string, object>>(jsonString)["data"];
		UpdateTimeStamps(dictionary);
		m_entityId = (string)dictionary["entityId"];
		base.State = EntityState.Ready;
		QueueUpdates();
	}

	public void CbCreateFailure(int statusCode, int reasonCode, string statusMessage, object cbObject)
	{
	}

	public void CbUpdateSuccess(string jsonString, object cbObject)
	{
		Dictionary<string, object> json = (Dictionary<string, object>)JsonReader.Deserialize<Dictionary<string, object>>(jsonString)["data"];
		UpdateTimeStamps(json);
	}

	public void CbUpdateFailure(int statusCode, int reasonCode, string statusMessage, object cbObject)
	{
	}

	public void CbDeleteSuccess(string json, object cbObject)
	{
		base.State = EntityState.Deleted;
	}

	public void CbDeleteFailure(int statusCode, int reasonCode, string statusMessage, object cbObject)
	{
	}

	protected override void CreateEntity(SuccessCallback success, FailureCallback failure)
	{
		string jsonEntityData = ToJsonString();
		string jsonEntityAcl = ((m_acl == null) ? null : m_acl.ToJsonString());
		m_bcEntityService.CreateEntity(m_entityType, jsonEntityData, jsonEntityAcl, (SuccessCallback)Delegate.Combine(new SuccessCallback(CbCreateSuccess), success), (FailureCallback)Delegate.Combine(new FailureCallback(CbCreateFailure), failure), this);
	}

	protected override void UpdateEntity(SuccessCallback success, FailureCallback failure)
	{
		string jsonEntityData = ToJsonString();
		string jsonEntityAcl = ((m_acl == null) ? null : m_acl.ToJsonString());
		m_bcEntityService.UpdateEntity(m_entityId, m_entityType, jsonEntityData, jsonEntityAcl, m_version, (SuccessCallback)Delegate.Combine(new SuccessCallback(CbUpdateSuccess), success), (FailureCallback)Delegate.Combine(new FailureCallback(CbUpdateFailure), failure), this);
	}

	protected override void UpdateSharedEntity(string targetProfileId, SuccessCallback success, FailureCallback failure)
	{
		string jsonEntityData = ToJsonString();
		m_bcEntityService.UpdateSharedEntity(m_entityId, targetProfileId, m_entityType, jsonEntityData, m_version, (SuccessCallback)Delegate.Combine(new SuccessCallback(CbUpdateSuccess), success), (FailureCallback)Delegate.Combine(new FailureCallback(CbUpdateFailure), failure), this);
	}

	protected override void DeleteEntity(SuccessCallback success, FailureCallback failure)
	{
		m_bcEntityService.DeleteEntity(m_entityId, m_version, (SuccessCallback)Delegate.Combine(new SuccessCallback(CbDeleteSuccess), success), (FailureCallback)Delegate.Combine(new FailureCallback(CbDeleteFailure), failure), this);
	}
}
