using System.Collections.Generic;
using System.Linq;
using Crosstales.Common.Model.Enum;
using UnityEngine;

namespace Crosstales.Common.Util;

public class PlatformController : MonoBehaviour
{
	[Tooltip("Selected platforms for the controller.")]
	[Header("Configuration")]
	public List<Platform> Platforms;

	[Tooltip("Enable or disable the 'Objects' for the selected 'Platforms' (default: true).")]
	public bool Active = true;

	[Header("GameObjects")]
	[Tooltip("Selected objects for the controller.")]
	public GameObject[] Objects;

	[Tooltip("Selected scripts for the controller.")]
	[Header("MonoBehaviour Scripts")]
	public MonoBehaviour[] Scripts;

	protected Platform _currentPlatform;

	protected virtual void Awake()
	{
		if (base.enabled)
		{
			selectPlatform();
		}
	}

	private void Start()
	{
	}

	protected void selectPlatform()
	{
		_currentPlatform = BaseHelper.CurrentPlatform;
		activateGameObjects();
		activateScripts();
	}

	protected void activateGameObjects()
	{
		GameObject[] objects = Objects;
		if (objects == null || objects.Length == 0)
		{
			return;
		}
		bool active = (Platforms.Contains(_currentPlatform) ? Active : (!Active));
		foreach (GameObject item in Objects.Where((GameObject go) => go != null))
		{
			item.SetActive(active);
		}
	}

	protected void activateScripts()
	{
		MonoBehaviour[] scripts = Scripts;
		if (scripts == null || scripts.Length == 0)
		{
			return;
		}
		bool flag = (Platforms.Contains(_currentPlatform) ? Active : (!Active));
		foreach (MonoBehaviour item in Scripts.Where((MonoBehaviour script) => script != null))
		{
			item.enabled = flag;
		}
	}
}
