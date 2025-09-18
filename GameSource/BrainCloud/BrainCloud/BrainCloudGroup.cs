using System;
using System.Collections;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudGroup
{
	public enum Role
	{
		OWNER,
		ADMIN,
		MEMBER,
		OTHER
	}

	public enum AutoJoinStrategy
	{
		JoinFirstGroup,
		JoinRandomGroup
	}

	private BrainCloudClient _bcClient;

	public BrainCloudGroup(BrainCloudClient client)
	{
		_bcClient = client;
	}

	public void AcceptGroupInvitation(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.AcceptGroupInvitation, success, failure, cbObject, dictionary);
	}

	public void AddGroupMember(string groupId, string profileId, Role role, string jsonAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		dictionary[OperationParam.GroupRole.Value] = role.ToString();
		if (Util.IsOptionalParameterValid(jsonAttributes))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonAttributes);
			dictionary[OperationParam.GroupAttributes.Value] = value;
		}
		SendRequest(ServiceOperation.AddGroupMember, success, failure, cbObject, dictionary);
	}

	public void ApproveGroupJoinRequest(string groupId, string profileId, Role role, string jsonAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		dictionary[OperationParam.GroupRole.Value] = role.ToString();
		if (Util.IsOptionalParameterValid(jsonAttributes))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonAttributes);
			dictionary[OperationParam.GroupAttributes.Value] = value;
		}
		SendRequest(ServiceOperation.ApproveGroupJoinRequest, success, failure, cbObject, dictionary);
	}

	public void AutoJoinGroup(string groupType, AutoJoinStrategy autoJoinStrategy, string dataQueryJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupType.Value] = groupType;
		dictionary[OperationParam.GroupAutoJoinStrategy.Value] = autoJoinStrategy.ToString();
		if (Util.IsOptionalParameterValid(dataQueryJson))
		{
			dictionary[OperationParam.GroupWhere.Value] = dataQueryJson;
		}
		SendRequest(ServiceOperation.AutoJoinGroup, success, failure, cbObject, dictionary);
	}

	public void AutoJoinGroupMulti(string[] groupTypes, AutoJoinStrategy autoJoinStrategy, string dataQueryJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupTypes.Value] = groupTypes;
		dictionary[OperationParam.GroupAutoJoinStrategy.Value] = autoJoinStrategy.ToString();
		if (Util.IsOptionalParameterValid(dataQueryJson))
		{
			dictionary[OperationParam.GroupWhere.Value] = dataQueryJson;
		}
		SendRequest(ServiceOperation.AutoJoinGroupMulti, success, failure, cbObject, dictionary);
	}

	public void CancelGroupInvitation(string groupId, string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		SendRequest(ServiceOperation.CancelGroupInvitation, success, failure, cbObject, dictionary);
	}

	public void CreateGroup(string name, string groupType, bool? isOpenGroup, GroupACL acl, string jsonData, string jsonOwnerAttributes, string jsonDefaultMemberAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(name))
		{
			dictionary[OperationParam.GroupName.Value] = name;
		}
		dictionary[OperationParam.GroupType.Value] = groupType;
		if (isOpenGroup.HasValue)
		{
			dictionary[OperationParam.GroupIsOpenGroup.Value] = isOpenGroup.Value;
		}
		if (acl != null)
		{
			dictionary[OperationParam.GroupAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		}
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		if (!string.IsNullOrEmpty(jsonOwnerAttributes))
		{
			dictionary[OperationParam.GroupOwnerAttributes.Value] = JsonReader.Deserialize(jsonOwnerAttributes);
		}
		if (!string.IsNullOrEmpty(jsonDefaultMemberAttributes))
		{
			dictionary[OperationParam.GroupDefaultMemberAttributes.Value] = JsonReader.Deserialize(jsonDefaultMemberAttributes);
		}
		SendRequest(ServiceOperation.CreateGroup, success, failure, cbObject, dictionary);
	}

	[Obsolete("This has been deprecated, use CreateGroupWithSummaryData instead. Removal on Match 1, 2022")]
	public void CreateGroup(string name, string groupType, bool? isOpenGroup, GroupACL acl, string jsonData, string jsonOwnerAttributes, string jsonDefaultMemberAttributes, string jsonSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(name))
		{
			dictionary[OperationParam.GroupName.Value] = name;
		}
		dictionary[OperationParam.GroupType.Value] = groupType;
		if (isOpenGroup.HasValue)
		{
			dictionary[OperationParam.GroupIsOpenGroup.Value] = isOpenGroup.Value;
		}
		if (acl != null)
		{
			dictionary[OperationParam.GroupAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		}
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		if (!string.IsNullOrEmpty(jsonOwnerAttributes))
		{
			dictionary[OperationParam.GroupOwnerAttributes.Value] = JsonReader.Deserialize(jsonOwnerAttributes);
		}
		if (!string.IsNullOrEmpty(jsonDefaultMemberAttributes))
		{
			dictionary[OperationParam.GroupDefaultMemberAttributes.Value] = JsonReader.Deserialize(jsonDefaultMemberAttributes);
		}
		if (!string.IsNullOrEmpty(jsonSummaryData))
		{
			dictionary[OperationParam.GroupSummaryData.Value] = JsonReader.Deserialize(jsonSummaryData);
		}
		SendRequest(ServiceOperation.CreateGroup, success, failure, cbObject, dictionary);
	}

	public void CreateGroupWithSummaryData(string name, string groupType, bool? isOpenGroup, GroupACL acl, string jsonData, string jsonOwnerAttributes, string jsonDefaultMemberAttributes, string jsonSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(name))
		{
			dictionary[OperationParam.GroupName.Value] = name;
		}
		dictionary[OperationParam.GroupType.Value] = groupType;
		if (isOpenGroup.HasValue)
		{
			dictionary[OperationParam.GroupIsOpenGroup.Value] = isOpenGroup.Value;
		}
		if (acl != null)
		{
			dictionary[OperationParam.GroupAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		}
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		if (!string.IsNullOrEmpty(jsonOwnerAttributes))
		{
			dictionary[OperationParam.GroupOwnerAttributes.Value] = JsonReader.Deserialize(jsonOwnerAttributes);
		}
		if (!string.IsNullOrEmpty(jsonDefaultMemberAttributes))
		{
			dictionary[OperationParam.GroupDefaultMemberAttributes.Value] = JsonReader.Deserialize(jsonDefaultMemberAttributes);
		}
		if (!string.IsNullOrEmpty(jsonSummaryData))
		{
			dictionary[OperationParam.GroupSummaryData.Value] = JsonReader.Deserialize(jsonSummaryData);
		}
		SendRequest(ServiceOperation.CreateGroup, success, failure, cbObject, dictionary);
	}

	public void CreateGroupEntity(string groupId, string entityType, bool? isOwnedByGroupMember, GroupACL acl, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		if (!string.IsNullOrEmpty(entityType))
		{
			dictionary[OperationParam.GroupEntityType.Value] = entityType;
		}
		if (isOwnedByGroupMember.HasValue)
		{
			dictionary[OperationParam.GroupIsOwnedByGroupMember.Value] = isOwnedByGroupMember.Value;
		}
		if (acl != null)
		{
			dictionary[OperationParam.GroupAcl.Value] = JsonReader.Deserialize(acl.ToJsonString());
		}
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		SendRequest(ServiceOperation.CreateGroupEntity, success, failure, cbObject, dictionary);
	}

	public void DeleteGroup(string groupId, long version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupVersion.Value] = version;
		SendRequest(ServiceOperation.DeleteGroup, success, failure, cbObject, dictionary);
	}

	public void DeleteGroupEntity(string groupId, string entityId, long version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupEntityId.Value] = entityId;
		dictionary[OperationParam.GroupVersion.Value] = version;
		SendRequest(ServiceOperation.DeleteGroupEntity, success, failure, cbObject, dictionary);
	}

	public void GetMyGroups(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SendRequest(ServiceOperation.GetMyGroups, success, failure, cbObject, null);
	}

	public void IncrementGroupData(string groupId, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		SendRequest(ServiceOperation.IncrementGroupData, success, failure, cbObject, dictionary);
	}

	public void IncrementGroupEntityData(string groupId, string entityId, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupEntityId.Value] = entityId;
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		SendRequest(ServiceOperation.IncrementGroupEntityData, success, failure, cbObject, dictionary);
	}

	public void InviteGroupMember(string groupId, string profileId, Role role, string jsonAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		dictionary[OperationParam.GroupRole.Value] = role.ToString();
		if (!string.IsNullOrEmpty(jsonAttributes))
		{
			dictionary[OperationParam.GroupAttributes.Value] = JsonReader.Deserialize(jsonAttributes);
		}
		SendRequest(ServiceOperation.InviteGroupMember, success, failure, cbObject, dictionary);
	}

	public void JoinGroup(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.JoinGroup, success, failure, cbObject, dictionary);
	}

	public void LeaveGroup(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.LeaveGroup, success, failure, cbObject, dictionary);
	}

	public void ListGroupsPage(string jsonContext, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupContext.Value] = JsonReader.Deserialize(jsonContext);
		SendRequest(ServiceOperation.ListGroupsPage, success, failure, cbObject, dictionary);
	}

	public void ListGroupsPageByOffset(string context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupContext.Value] = context;
		dictionary[OperationParam.GroupPageOffset.Value] = pageOffset;
		SendRequest(ServiceOperation.ListGroupsPageByOffset, success, failure, cbObject, dictionary);
	}

	public void ListGroupsWithMember(string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		SendRequest(ServiceOperation.ListGroupsWithMember, success, failure, cbObject, dictionary);
	}

	public void ReadGroup(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.ReadGroup, success, failure, cbObject, dictionary);
	}

	public void ReadGroupData(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.ReadGroupData, success, failure, cbObject, dictionary);
	}

	public void ReadGroupEntitiesPage(string jsonContext, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupContext.Value] = JsonReader.Deserialize(jsonContext);
		SendRequest(ServiceOperation.ReadGroupEntitiesPage, success, failure, cbObject, dictionary);
	}

	public void ReadGroupEntitiesPageByOffset(string encodedContext, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupContext.Value] = encodedContext;
		dictionary[OperationParam.GroupPageOffset.Value] = pageOffset;
		SendRequest(ServiceOperation.ReadGroupEntitiesPageByOffset, success, failure, cbObject, dictionary);
	}

	public void ReadGroupEntity(string groupId, string entityId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupEntityId.Value] = entityId;
		SendRequest(ServiceOperation.ReadGroupEntity, success, failure, cbObject, dictionary);
	}

	public void ReadGroupMembers(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.ReadGroupMembers, success, failure, cbObject, dictionary);
	}

	public void RejectGroupInvitation(string groupId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		SendRequest(ServiceOperation.RejectGroupInvitation, success, failure, cbObject, dictionary);
	}

	public void RejectGroupJoinRequest(string groupId, string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		SendRequest(ServiceOperation.RejectGroupJoinRequest, success, failure, cbObject, dictionary);
	}

	public void RemoveGroupMember(string groupId, string profileId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		SendRequest(ServiceOperation.RemoveGroupMember, success, failure, cbObject, dictionary);
	}

	public void UpdateGroupData(string groupId, long version, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupVersion.Value] = version;
		dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		SendRequest(ServiceOperation.UpdateGroupData, success, failure, cbObject, dictionary);
	}

	public void UpdateGroupEntityData(string groupId, string entityId, long version, string jsonData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupEntityId.Value] = entityId;
		dictionary[OperationParam.GroupVersion.Value] = version;
		if (!string.IsNullOrEmpty(jsonData))
		{
			dictionary[OperationParam.GroupData.Value] = JsonReader.Deserialize(jsonData);
		}
		SendRequest(ServiceOperation.UpdateGroupEntity, success, failure, cbObject, dictionary);
	}

	public void UpdateGroupMember(string groupId, string profileId, Role? role, string jsonAttributes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupProfileId.Value] = profileId;
		if (role.HasValue)
		{
			dictionary[OperationParam.GroupRole.Value] = role.Value.ToString();
		}
		if (!string.IsNullOrEmpty(jsonAttributes))
		{
			dictionary[OperationParam.GroupAttributes.Value] = JsonReader.Deserialize(jsonAttributes);
		}
		SendRequest(ServiceOperation.UpdateGroupMember, success, failure, cbObject, dictionary);
	}

	public void UpdateGroupName(string groupId, string name, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupName.Value] = name;
		SendRequest(ServiceOperation.UpdateGroupName, success, failure, cbObject, dictionary);
	}

	public void SetGroupOpen(string groupId, bool isOpenGroup, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupIsOpenGroup.Value] = isOpenGroup;
		SendRequest(ServiceOperation.SetGroupOpen, success, failure, cbObject, dictionary);
	}

	public void UpdateGroupSummaryData(string groupId, int version, string jsonSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.GroupVersion.Value] = version;
		if (!string.IsNullOrEmpty(jsonSummaryData))
		{
			dictionary[OperationParam.GroupSummaryData.Value] = JsonReader.Deserialize(jsonSummaryData);
		}
		SendRequest(ServiceOperation.UpdateGroupSummaryData, success, failure, cbObject, dictionary);
	}

	public void GetRandomGroupsMatching(string jsonWhere, int maxReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(jsonWhere))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonWhere);
			dictionary[OperationParam.GroupWhere.Value] = value;
		}
		dictionary[OperationParam.GroupMaxReturn.Value] = maxReturn;
		SendRequest(ServiceOperation.GetRandomGroupsMatching, success, failure, cbObject, dictionary);
	}

	private void SendRequest(ServiceOperation operation, SuccessCallback success, FailureCallback failure, object cbObject, IDictionary data)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Group, operation, data, callback);
		_bcClient.SendRequest(serviceMessage);
	}
}
