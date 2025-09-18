using System;
using UnityEngine;

[Serializable]
public class CrumblingBlockScriptableObject : ScriptableObject
{
	public string rootName;

	public Sprite hold;

	public Sprite broken1;

	public Sprite broken2;

	public Sprite[] Crumbling;
}
