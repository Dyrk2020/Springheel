using System;
using UnityEngine;

[Serializable]
public class UIControlLibrary : ScriptableObject
{
	public controlSchemeUIArt[] controlSchemes;

	public ControlSchemeTypes currentControlType;

	protected static UIControlLibrary instance;

	public static UIControlLibrary GetInstance()
	{
		if (instance == null)
		{
			instance = (UIControlLibrary)Resources.Load("MainUIControlLibrary");
			UnityEngine.Object.DontDestroyOnLoad(instance);
		}
		return instance;
	}

	public Sprite GetButtonArt(buttonTypes buttonType)
	{
		return controlSchemes[(int)currentControlType].buttonArray[(int)buttonType];
	}
}
