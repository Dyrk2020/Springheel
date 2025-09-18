using System.Collections.Generic;
using UnityEngine;

public class OutfitChangeProp : UsableProp
{
	public OutfitManager OutfitManager;

	public static Dictionary<Character, OutfitController> OutfitControllers = new Dictionary<Character, OutfitController>();

	public OutfitController outfitControllerPrefab;

	public string audioString;

	public string exitAudioString;

	public Sprite SpriteAvailable;

	public Sprite SpriteUnAvailable;

	protected bool OutfitsAvailable
	{
		get
		{
			int[] values = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatCountArray>("OutfitsUnlocked").values;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override bool Use(LobbyPlayer lobbyPlayer, InputEvent.InputKey usedInputKey)
	{
		if (!OutfitsAvailable || !base.Use(lobbyPlayer, usedInputKey))
		{
			return false;
		}
		OutfitController outfitController;
		if (!OutfitControllers.ContainsKey(lobbyPlayer.CharacterInstance))
		{
			outfitController = Object.Instantiate(outfitControllerPrefab);
			OutfitControllers.Add(lobbyPlayer.CharacterInstance, outfitController);
			outfitController.OutfitManager = OutfitManager;
		}
		else
		{
			outfitController = OutfitControllers[lobbyPlayer.CharacterInstance];
		}
		outfitController.outfitChangeProp = this;
		outfitController.player = lobbyPlayer;
		outfitController.characterUsing = characterUsing;
		characterUsing.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		outfitController.transform.position = characterUsing.transform.position;
		outfitController.UseController = lobbyPlayer.LocalPlayer.UseController;
		outfitController.Show();
		if (audioString != "")
		{
			AkSoundEngine.PostEvent(audioString, base.gameObject);
		}
		return true;
	}

	public override void Release(bool unFreeze = true)
	{
		base.Release(unFreeze);
		if (exitAudioString != "")
		{
			AkSoundEngine.PostEvent(exitAudioString, base.gameObject);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (OutfitsAvailable)
		{
			spriteRenderer.sprite = SpriteAvailable;
		}
		else
		{
			spriteRenderer.sprite = SpriteUnAvailable;
		}
	}

	public override void Update()
	{
		if (OutfitsAvailable)
		{
			spriteRenderer.sprite = SpriteAvailable;
			Tint();
		}
		else
		{
			spriteRenderer.sprite = SpriteUnAvailable;
		}
	}
}
