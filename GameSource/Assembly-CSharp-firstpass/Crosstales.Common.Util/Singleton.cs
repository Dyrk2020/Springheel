using System;
using UnityEngine;

namespace Crosstales.Common.Util;

[DisallowMultipleComponent]
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	[Tooltip("Don't destroy gameobject during scene switches (default: false).")]
	[SerializeField]
	private bool dontDestroy = true;

	public static string PrefabPath;

	public static string GameObjectName = typeof(T).Name;

	protected static T instance;

	private static readonly object LOCK_OBJ = new object();

	public static T Instance
	{
		get
		{
			if (SingletonHelper.isQuitting && BaseHelper.isEditorMode)
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.Log($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
				}
				return instance;
			}
			if (instance == null)
			{
				CreateInstance();
			}
			return instance;
		}
		protected set
		{
			lock (LOCK_OBJ)
			{
				instance = value;
			}
		}
	}

	public bool DontDestroy
	{
		get
		{
			return dontDestroy;
		}
		set
		{
			dontDestroy = value;
		}
	}

	protected virtual void Awake()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"{Time.realtimeSinceStartup}-[Singleton] Instance '{typeof(T)}' AWAKE");
		}
		BaseHelper.ApplicationIsPlaying = Application.isPlaying;
		if (instance == null)
		{
			Instance = GetComponent<T>();
			if (!BaseHelper.isEditorMode && dontDestroy)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.transform.root.gameObject);
			}
		}
		else if (!BaseHelper.isEditorMode && dontDestroy && instance != this)
		{
			Debug.LogWarning("Only one active instance of '" + typeof(T).Name + "' allowed in all scenes!" + Environment.NewLine + "This object will now be destroyed.", this);
			UnityEngine.Object.Destroy(base.gameObject, 0.1f);
		}
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			if (BaseConstants.DEV_DEBUG)
			{
				Debug.Log($"{Time.realtimeSinceStartup}-[Singleton] Instance '{typeof(T)}' ONDESTROY: {instance.GetInstanceID()}");
			}
			if (!dontDestroy)
			{
				Instance = null;
			}
		}
	}

	protected virtual void OnApplicationQuit()
	{
		SingletonHelper.isQuitting = true;
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log("OnApplicationQuit", this);
		}
		BaseHelper.ApplicationIsPlaying = false;
	}

	public static void CreateInstance(bool searchExistingGameObject = true, bool deleteExistingInstance = false)
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"[Singleton] Instance '{typeof(T)}' will be newly created.");
		}
		if (deleteExistingInstance)
		{
			DeleteInstance();
		}
		if (!(instance == null))
		{
			return;
		}
		if (searchExistingGameObject)
		{
			Instance = (T)UnityEngine.Object.FindObjectOfType(typeof(T));
		}
		if (!(instance == null))
		{
			return;
		}
		if (!string.IsNullOrEmpty(PrefabPath))
		{
			T val = Resources.Load<T>(PrefabPath);
			if (val == null)
			{
				Debug.LogWarning("Singleton prefab missing: " + PrefabPath);
			}
			else
			{
				if (BaseConstants.DEV_DEBUG)
				{
					Debug.Log($"{Time.realtimeSinceStartup}-[Singleton] Instance '{typeof(T)}' CREATE Prefab");
				}
				Instance = UnityEngine.Object.Instantiate(val);
				Instance.name = GameObjectName;
			}
		}
		if (instance == null && !BaseHelper.isEditorMode)
		{
			if (BaseConstants.DEV_DEBUG)
			{
				Debug.Log($"{Time.realtimeSinceStartup}-[Singleton] Instance '{typeof(T)}' CREATE Play: {BaseHelper.isEditorMode}");
			}
			Instance = new GameObject(GameObjectName).AddComponent<T>();
		}
	}

	public static void DeleteInstance()
	{
		if (instance != null)
		{
			T val = instance;
			Instance = null;
			UnityEngine.Object.Destroy(val.gameObject);
		}
	}
}
