using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvent;

public class GameEventManager
{
	public static Dictionary<Type, List<IGameEventListener>> listeners = new Dictionary<Type, List<IGameEventListener>>();

	public static void ChangeListener<T>(IGameEventListener listener, bool adding) where T : GameEvent
	{
		Type typeFromHandle = typeof(T);
		if (listeners == null)
		{
			Debug.Log("NO LISTENERS");
		}
		if (adding)
		{
			if (!listeners.ContainsKey(typeFromHandle))
			{
				listeners.Add(typeFromHandle, new List<IGameEventListener>());
			}
			List<IGameEventListener> list = listeners[typeFromHandle];
			if (!list.Contains(listener))
			{
				list.Add(listener);
			}
		}
		else if (listeners.ContainsKey(typeFromHandle))
		{
			List<IGameEventListener> list2 = listeners[typeFromHandle];
			if (list2.Contains(listener))
			{
				list2.Remove(listener);
			}
		}
	}

	public static void RemoveListenerFromAll(IGameEventListener listener)
	{
		foreach (List<IGameEventListener> value in listeners.Values)
		{
			if (value.Contains(listener))
			{
				value.Remove(listener);
			}
		}
	}

	public static bool SendEvent(GameEvent e)
	{
		Type type = e.GetType();
		e.Sent = true;
		if (!listeners.ContainsKey(type))
		{
			return false;
		}
		List<int> list = new List<int>();
		List<IGameEventListener> list2 = listeners[type];
		for (int i = 0; i != list2.Count; i++)
		{
			if (e.Resolved)
			{
				break;
			}
			bool flag = false;
			if (!((!(list2[i] is UnityEngine.Object)) ? (list2[i] == null) : ((UnityEngine.Object)list2[i] == null)))
			{
				try
				{
					list2[i].handleEvent(e);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while handling " + type.ToString() + " event on " + list2[i].GetType().ToString() + ": " + ex.Message + "\n" + ex.StackTrace);
				}
			}
			else
			{
				list.Add(i);
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list2.RemoveAt(list[num]);
		}
		e.Resolved = true;
		return true;
	}
}
