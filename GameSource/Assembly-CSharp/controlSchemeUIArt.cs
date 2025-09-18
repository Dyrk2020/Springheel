using System;
using UnityEngine;

[Serializable]
public class controlSchemeUIArt
{
	public ControlSchemeTypes controlSchemeType;

	public Sprite[] buttonArray = new Sprite[Enum.GetValues(typeof(buttonTypes)).Length];
}
