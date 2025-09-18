using UnityEngine;

public class CharacterSpriteLoader : MonoBehaviour
{
	public CharacterSpriteManager ManagerObj;

	private void Start()
	{
		if (ManagerObj != null)
		{
			CharacterSpriteManager.SetInstance(ManagerObj);
		}
	}
}
