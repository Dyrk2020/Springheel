using System.Collections.Generic;
using UnityEngine;

public class BeeSwarm : MonoBehaviour
{
	public Beehive HomeHive;

	private Character target;

	private Dictionary<Character, bool> caught;

	private Character[] characters;

	private bool disappearing;

	[HideInInspector]
	public int placedByPlayerNumber;

	[HideInInspector]
	public string KillType;

	private void Awake()
	{
		caught = new Dictionary<Character, bool>();
		characters = new Character[4];
	}

	public void Release(Character target)
	{
		disappearing = false;
		caught.Keys.CopyTo(characters, 0);
		for (int i = 0; i < characters.Length; i++)
		{
			if (characters[i] != null)
			{
				caught[characters[i]] = false;
			}
		}
		this.target = target;
		AkSoundEngine.PostEvent("SFX_Pieces_Beehive_FollowPlayer_Start", base.gameObject);
	}

	public void Disappear()
	{
		if (target != null && (!caught.ContainsKey(target) || !caught[target]) && !disappearing)
		{
			AkSoundEngine.PostEvent("SFX_Pieces_Beehive_FollowPlayer_Stop", base.gameObject);
		}
		disappearing = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Character componentInParent = collision.GetComponentInParent<Character>();
		if (componentInParent == null)
		{
			return;
		}
		if (!caught.ContainsKey(componentInParent))
		{
			caught.Add(componentInParent, value: false);
		}
		if (!componentInParent.Dead && !componentInParent.Dying && !caught[componentInParent])
		{
			caught[componentInParent] = true;
			if (componentInParent == target)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Beehive_FollowPlayer_Catch", base.gameObject);
			}
			else if (componentInParent != target)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Beehive_FollowPlayer_Collateral", base.gameObject);
			}
		}
	}
}
