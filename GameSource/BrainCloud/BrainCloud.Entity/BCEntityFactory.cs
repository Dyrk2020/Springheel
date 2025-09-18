using System;
using System.Collections.Generic;
using System.Reflection;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Entity;

public class BCEntityFactory
{
	public delegate BCUserEntity CreateUserEntityFromType(string type);

	private BrainCloudEntity m_bcEntityService;

	private IDictionary<string, ConstructorInfo> m_registeredClasses;

	public BCEntityFactory(BrainCloudEntity in_bcEntityService)
	{
		m_bcEntityService = in_bcEntityService;
		m_registeredClasses = new Dictionary<string, ConstructorInfo>();
	}

	public T NewEntity<T>(string entityType) where T : BCEntity
	{
		T val = (T)CreateRegisteredEntityClass(entityType);
		if (val == null)
		{
			val = (T)Activator.CreateInstance(typeof(T), m_bcEntityService);
		}
		val.BrainCloudEntityService = m_bcEntityService;
		val.EntityType = entityType;
		return val;
	}

	public BCUserEntity NewUserEntity(string entityType)
	{
		BCUserEntity bCUserEntity = (BCUserEntity)CreateRegisteredEntityClass(entityType);
		if (bCUserEntity == null)
		{
			bCUserEntity = new BCUserEntity(m_bcEntityService);
		}
		bCUserEntity.EntityType = entityType;
		return bCUserEntity;
	}

	public IList<BCUserEntity> NewUserEntitiesFromGetList(string json)
	{
		Dictionary<string, object> dictionary = JsonReader.Deserialize<Dictionary<string, object>>(json);
		try
		{
			return NewUserEntitiesFromJsonString(json, (Array)((Dictionary<string, object>)dictionary["data"])["entityList"]);
		}
		catch (KeyNotFoundException)
		{
			return new List<BCUserEntity>();
		}
	}

	public IList<BCUserEntity> NewUserEntitiesFromReadPlayerState(string json)
	{
		Dictionary<string, object> dictionary = JsonReader.Deserialize<Dictionary<string, object>>(json);
		try
		{
			return NewUserEntitiesFromJsonString(json, (Array)((Dictionary<string, object>)dictionary["data"])["entities"]);
		}
		catch (KeyNotFoundException)
		{
			return new List<BCUserEntity>();
		}
	}

	public IList<BCUserEntity> NewUserEntitiesFromStartMatch(string json)
	{
		Dictionary<string, object> dictionary = JsonReader.Deserialize<Dictionary<string, object>>(json);
		try
		{
			return NewUserEntitiesFromJsonString(json, (Array)((Dictionary<string, object>)((Dictionary<string, object>)dictionary["data"])["initialSharedData"])["entities"]);
		}
		catch (KeyNotFoundException)
		{
			return new List<BCUserEntity>();
		}
	}

	public IList<BCUserEntity> NewUserEntitiesFromDataResponse(string json)
	{
		Dictionary<string, object> dictionary = JsonReader.Deserialize<Dictionary<string, object>>(json);
		try
		{
			return NewUserEntitiesFromJsonString(json, (Array)((Dictionary<string, object>)((Dictionary<string, object>)dictionary["data"])["response"])["entities"]);
		}
		catch (KeyNotFoundException)
		{
			return new List<BCUserEntity>();
		}
	}

	public void RegisterEntityClass<T>(string entityType) where T : BCEntity
	{
		Type typeFromHandle = typeof(T);
		Type[] types = new Type[0];
		ConstructorInfo constructor = typeFromHandle.GetConstructor(types);
		if (constructor != null)
		{
			m_registeredClasses[entityType] = constructor;
		}
	}

	private BCEntity CreateRegisteredEntityClass(string entityType)
	{
		ConstructorInfo value = null;
		if (m_registeredClasses.TryGetValue(entityType, out value))
		{
			return (BCEntity)value.Invoke(null);
		}
		return null;
	}

	public BCUserEntity NewUserFromDictionary(Dictionary<string, object> in_dict)
	{
		BCUserEntity bCUserEntity = null;
		if (in_dict != null)
		{
			try
			{
				bCUserEntity = NewUserEntity((string)in_dict["entityType"]);
				bCUserEntity.ReadFromJson(in_dict);
			}
			catch (Exception)
			{
			}
		}
		return bCUserEntity;
	}

	public IList<BCUserEntity> NewUserEntitiesFromJsonString(string json, Array entitiesJson)
	{
		List<BCUserEntity> list = new List<BCUserEntity>();
		Dictionary<string, object> dictionary = null;
		for (int i = 0; i < entitiesJson.Length; i++)
		{
			try
			{
				dictionary = entitiesJson.GetValue(i) as Dictionary<string, object>;
				BCUserEntity item = NewUserFromDictionary(dictionary);
				list.Add(item);
			}
			catch (Exception)
			{
			}
		}
		return list;
	}
}
