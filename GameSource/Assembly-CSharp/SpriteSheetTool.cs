using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetTool : ScriptableObject
{
	public List<Texture2D> OldTextures;

	public List<Texture2D> NewTextures;

	public List<Texture2D> UnusedTextures;

	public bool logging = true;

	public bool openLogAfterOperation = true;

	public bool searchInPrefabs = true;

	public bool searchInScenes = true;

	public bool searchInAnimClips = true;
}
