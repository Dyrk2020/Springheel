using System;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Entity.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Entity;

public abstract class BCEntity
{
	protected enum EntityState
	{
		New = 0,
		Creating = 1,
		Ready = 2,
		Deleting = 4,
		Deleted = 5
	}

	protected string m_entityId;

	protected string m_entityType;

	protected ACL m_acl;

	protected int m_version = -1;

	protected IDictionary<string, object> m_data = new Dictionary<string, object>();

	private EntityState m_state;

	protected bool m_updateWhenCreated;

	private SuccessCallback m_updateWhenCreatedSuccessCb;

	private FailureCallback m_updateWhenCreatedFailureCb;

	protected DateTime m_createdAt;

	protected DateTime m_updatedAt;

	protected BrainCloudEntity m_bcEntityService;

	public string EntityId => m_entityId;

	public string EntityType
	{
		get
		{
			return m_entityType;
		}
		set
		{
			m_entityType = value;
		}
	}

	public ACL ACL
	{
		get
		{
			return m_acl;
		}
		set
		{
			m_acl = value;
		}
	}

	public DateTime CreatedAt => m_createdAt;

	public DateTime UpdatedAt => m_updatedAt;

	protected EntityState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (value < m_state)
			{
				throw new ArgumentException("Can't transition to a lower state");
			}
			m_state = value;
		}
	}

	public BrainCloudEntity BrainCloudEntityService
	{
		get
		{
			return m_bcEntityService;
		}
		set
		{
			m_bcEntityService = value;
		}
	}

	public object this[string key]
	{
		get
		{
			return m_data[key];
		}
		set
		{
			if (value == null)
			{
				Remove(key);
			}
			else
			{
				m_data[key] = value;
			}
		}
	}

	protected abstract void CreateEntity(SuccessCallback success, FailureCallback failure);

	protected abstract void UpdateEntity(SuccessCallback success, FailureCallback failure);

	protected abstract void UpdateSharedEntity(string targetProfileId, SuccessCallback success, FailureCallback failure);

	protected abstract void DeleteEntity(SuccessCallback success, FailureCallback failure);

	public BCEntity(BrainCloudEntity braincloud)
	{
		m_bcEntityService = braincloud;
	}

	public void StoreAsync(SuccessCallback success = null, FailureCallback failure = null)
	{
		if (m_state != EntityState.Deleting && m_state != EntityState.Deleted)
		{
			if (m_state == EntityState.Creating)
			{
				m_updateWhenCreated = true;
				m_updateWhenCreatedSuccessCb = success;
				m_updateWhenCreatedFailureCb = failure;
			}
			else if (m_state == EntityState.New)
			{
				CreateEntity(success, failure);
				m_state = EntityState.Creating;
			}
			else
			{
				UpdateEntity(success, failure);
			}
		}
	}

	public void StoreAsyncShared(string targetProfileId, SuccessCallback success = null, FailureCallback failure = null)
	{
		if (m_state != EntityState.Deleting && m_state != EntityState.Deleted)
		{
			if (m_state == EntityState.Creating)
			{
				m_updateWhenCreated = true;
				m_updateWhenCreatedSuccessCb = success;
				m_updateWhenCreatedFailureCb = failure;
			}
			else if (m_state == EntityState.New)
			{
				CreateEntity(success, failure);
				m_state = EntityState.Creating;
			}
			else
			{
				UpdateSharedEntity(targetProfileId, success, failure);
			}
		}
	}

	public void DeleteAsync(SuccessCallback success = null, FailureCallback failure = null)
	{
		if (m_state != EntityState.New && m_state != EntityState.Deleting && m_state != EntityState.Deleted)
		{
			DeleteEntity(success, failure);
			m_state = EntityState.Deleting;
		}
	}

	public bool Contains(string key)
	{
		return m_data.ContainsKey(key);
	}

	public void Remove(string key)
	{
		if (m_data.ContainsKey(key))
		{
			m_data.Remove(key);
		}
	}

	public T Get<T>(string key)
	{
		return EntityUtil.GetObjectAsType<T>(m_data[key]);
	}

	protected void UpdateTimeStamps(Dictionary<string, object> json)
	{
		try
		{
			m_createdAt = Util.BcTimeToDateTime((long)json["createdAt"]);
			m_updatedAt = Util.BcTimeToDateTime((long)json["updatedAt"]);
		}
		catch (Exception)
		{
		}
	}

	protected void QueueUpdates()
	{
		if (m_updateWhenCreated)
		{
			StoreAsync(m_updateWhenCreatedSuccessCb, m_updateWhenCreatedFailureCb);
			m_updateWhenCreated = false;
			m_updateWhenCreatedSuccessCb = null;
			m_updateWhenCreatedFailureCb = null;
		}
	}

	public string ToJsonString()
	{
		return JsonWriter.Serialize(m_data);
	}

	public void ReadFromJson(string json)
	{
		object jsonObj = JsonReader.Deserialize(json);
		ReadFromJson(jsonObj);
	}

	public void ReadFromJson(object jsonObj)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonObj;
		m_state = EntityState.Ready;
		m_entityType = (string)dictionary["entityType"];
		m_entityId = (string)dictionary["entityId"];
		m_acl = ACL.CreateFromJson((Dictionary<string, object>)dictionary["acl"]);
		UpdateTimeStamps(dictionary);
		m_data = JsonToDictionary(dictionary["data"]);
	}

	public override string ToString()
	{
		return ToJsonString();
	}

	protected static IDictionary<string, object> JsonToDictionary(object jsonObj)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object>.Enumerator enumerator = (jsonObj as Dictionary<string, object>).GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, object> current = enumerator.Current;
			object value = current.Value;
			string key = current.Key;
			if (value is Dictionary<string, object>)
			{
				dictionary[key] = JsonToDictionary(value);
			}
			else if (value is Array)
			{
				dictionary[key] = JsonToList(value);
			}
			else
			{
				dictionary[key] = JsonToBasicType(value);
			}
		}
		return dictionary;
	}

	protected static IList<object> JsonToList(object jsonObj)
	{
		List<object> list = new List<object>();
		Array array = (Array)jsonObj;
		if (array != null)
		{
			object obj = null;
			for (int i = 0; i < array.Length; i++)
			{
				obj = array.GetValue(i);
				if (obj is Dictionary<string, object>)
				{
					list.Add(JsonToDictionary(obj));
				}
				else if (obj is Array)
				{
					list.Add(JsonToList(obj));
				}
				else
				{
					list.Add(JsonToBasicType(obj));
				}
			}
		}
		return list;
	}

	protected static object JsonToBasicType(object jsonObj)
	{
		if (jsonObj is bool)
		{
			return (bool)jsonObj;
		}
		if (jsonObj is string)
		{
			return (string)jsonObj;
		}
		if (jsonObj is int)
		{
			return (int)jsonObj;
		}
		if (jsonObj is uint)
		{
			return (uint)jsonObj;
		}
		if (jsonObj is float)
		{
			return (float)jsonObj;
		}
		if (jsonObj is double)
		{
			return (double)jsonObj;
		}
		if (jsonObj is decimal)
		{
			return (decimal)jsonObj;
		}
		if (jsonObj is long)
		{
			return (long)jsonObj;
		}
		if (jsonObj is ulong)
		{
			return (ulong)jsonObj;
		}
		if (jsonObj is short)
		{
			return (short)jsonObj;
		}
		if (jsonObj is ushort)
		{
			return (ushort)jsonObj;
		}
		if (jsonObj is sbyte)
		{
			return (sbyte)jsonObj;
		}
		if (jsonObj is byte)
		{
			return (byte)jsonObj;
		}
		throw new ArgumentException("Unexpected type");
	}
}
