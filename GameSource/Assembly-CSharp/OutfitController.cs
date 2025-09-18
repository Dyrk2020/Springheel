using System;
using System.Collections.Generic;
using UnityEngine;

public class OutfitController : MonoBehaviour, InputReceiver
{
	public OutfitRowController[] outfitRowControllers;

	public OutfitChangeProp outfitChangeProp;

	private Controller useController;

	public LobbyPlayer player;

	public Character characterUsing;

	public OutfitManager OutfitManager;

	private int row = 1;

	private int[] column = new int[4];

	public Sprite nullSprite;

	public Color nullSpriteColor;

	public float nullSpriteScale;

	public SpriteRenderer backgroundSprite;

	private int numRows = 4;

	private int[] numOutfits = new int[4];

	public Controller UseController
	{
		get
		{
			return useController;
		}
		set
		{
			if (useController != null || value == null)
			{
				useController.RemoveReceiver(this);
			}
			useController = value;
			if (useController != null)
			{
				useController.AddReceiver(this);
			}
		}
	}

	private void UpdateNumOutfits()
	{
		if (OutfitManager.characterOutfitsUnlocked.ContainsKey(player.CharacterInstance))
		{
			for (int i = 0; i < Outfit.NumOutfitTypes; i++)
			{
				numOutfits[i] = GetUnlockedOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)i).Count;
			}
		}
		ShouldRowExist();
	}

	private void ShouldRowExist()
	{
		for (int i = 0; i < Outfit.NumOutfitTypes && GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)i) != null; i++)
		{
			if (GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)i).Count == 0)
			{
				outfitRowControllers[i].Hide();
			}
			else
			{
				outfitRowControllers[i].Show();
			}
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.Key == outfitChangeProp.usedInputKey && e.Changed && !e.Valueb)
		{
			outfitChangeProp.usedInputKey = InputEvent.InputKey.NoKey;
		}
		else if ((e.Key == InputEvent.InputKey.Back || e.Key == InputEvent.InputKey.Accept || e.Key == InputEvent.InputKey.Inventory) && e.Changed && !e.Valueb)
		{
			characterUsing.LockInputTemporarily(0.2f);
			Release();
			Hide();
			e.Consume();
		}
		else if ((e.Key == InputEvent.InputKey.Start || e.Key == InputEvent.InputKey.Esc || e.Key == InputEvent.InputKey.Scoreboard) && e.Changed && !e.Valueb)
		{
			characterUsing.LockInputTemporarily(0.2f);
			Release(unFreeze: false);
			Hide();
		}
		else
		{
			if (e.Key == InputEvent.InputKey.ChangeMode)
			{
				return;
			}
			UpdateNumOutfits();
			InputEvent.InputKey inputKey = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.OrthoRight : InputEvent.InputKey.OrthoLeft);
			InputEvent.InputKey inputKey2 = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.OrthoLeft : InputEvent.InputKey.OrthoRight);
			InputEvent.InputKey inputKey3 = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.RotateRight : InputEvent.InputKey.RotateLeft);
			InputEvent.InputKey inputKey4 = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.RotateLeft : InputEvent.InputKey.RotateRight);
			if ((e.Key == inputKey2 || e.Key == inputKey4) && e.Changed && e.Valueb && row >= 0 && row < numRows)
			{
				OutfitRowController obj = outfitRowControllers[row];
				int outfitType = (int)obj.OutfitType;
				column[outfitType]++;
				obj.Right();
				if (column[outfitType] > numOutfits[outfitType])
				{
					column[outfitType] = 0;
				}
				if (column[outfitType] == 0 && player.CharacterInstance.OutfitArt.GetDefaultForcedOutfit(outfitType) != null)
				{
					column[outfitType] = 1;
				}
			}
			if ((e.Key == inputKey || e.Key == inputKey3) && e.Changed && e.Valueb && row >= 0 && row < numRows)
			{
				OutfitRowController obj2 = outfitRowControllers[row];
				int outfitType2 = (int)obj2.OutfitType;
				column[outfitType2]--;
				obj2.Left();
				if (column[outfitType2] < 0)
				{
					column[outfitType2] = numOutfits[outfitType2];
				}
				if (column[outfitType2] == 0 && player.CharacterInstance.OutfitArt.GetDefaultForcedOutfit(outfitType2) != null)
				{
					column[outfitType2] = numOutfits[outfitType2];
				}
			}
			InputEvent.InputKey inputKey5 = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.OrthoDown : InputEvent.InputKey.OrthoUp);
			InputEvent.InputKey inputKey6 = (Modifiers.GetInstance().CameraFlippedOnY ? InputEvent.InputKey.OrthoUp : InputEvent.InputKey.OrthoDown);
			if (e.Key == inputKey5 && e.Changed && e.Valueb)
			{
				row++;
				if (row >= numRows)
				{
					row = 0;
				}
				if (GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)row) == null || GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)row).Count == 0)
				{
					row++;
					if (row >= numRows)
					{
						row = 0;
					}
				}
				AkSoundEngine.PostEvent("UI_Lobby_Arrow_Selection", base.gameObject);
			}
			if (e.Key == inputKey6 && e.Changed && e.Valueb)
			{
				row--;
				if (row < 0)
				{
					row = numRows - 1;
				}
				if (GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)row) == null || GetAllOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)row).Count == 0)
				{
					row--;
					if (row < 0)
					{
						row = numRows - 1;
					}
				}
				AkSoundEngine.PostEvent("UI_Lobby_Arrow_Selection", base.gameObject);
			}
			updateImages();
		}
	}

	public void Show()
	{
		if (characterUsing.AssociatedLobbyPlayer != null && characterUsing.AssociatedLobbyPlayer.characterOutfitsList.Count >= Outfit.NumOutfitTypes)
		{
			Outfit[] componentsInChildren = characterUsing.OutfitArt.GetComponentsInChildren<Outfit>();
			for (int i = 0; i < Outfit.NumOutfitTypes; i++)
			{
				int num = characterUsing.AssociatedLobbyPlayer.characterOutfitsList[i];
				if (num != -1)
				{
					Outfit outfit = componentsInChildren[num];
					int num2 = OutfitManager.characterOutfitsUnlocked[characterUsing][i].FindIndex((Outfit x) => outfit == x);
					column[i] = num2 + 1;
				}
				else
				{
					column[i] = 0;
				}
			}
		}
		else
		{
			for (int num3 = 0; num3 < Outfit.NumOutfitTypes; num3++)
			{
				column[num3] = 0;
			}
		}
		for (int num4 = 0; num4 < Outfit.NumOutfitTypes; num4++)
		{
			if (column[num4] != 0)
			{
				continue;
			}
			Outfit forcedDefault = characterUsing.OutfitArt.GetDefaultForcedOutfit(num4);
			if (forcedDefault != null)
			{
				int num5 = OutfitManager.characterOutfitsUnlocked[characterUsing][num4].FindIndex((Outfit x) => forcedDefault == x);
				column[num4] = num5 + 1;
			}
		}
		SelectFirstPopulatedRow();
		updateImages();
		backgroundSprite.enabled = true;
	}

	private void SelectFirstPopulatedRow()
	{
		int[] array = new int[outfitRowControllers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}
		Array.Sort(array, delegate(int a, int b)
		{
			int siblingIndex = outfitRowControllers[a].transform.GetSiblingIndex();
			int siblingIndex2 = outfitRowControllers[b].transform.GetSiblingIndex();
			return siblingIndex.CompareTo(siblingIndex2);
		});
		for (int num = 1; num == 1; num++)
		{
			int num2 = array[num];
			List<Outfit> unlockedOutfitListForCharacter = GetUnlockedOutfitListForCharacter(characterUsing, outfitRowControllers[num2].OutfitType);
			if (unlockedOutfitListForCharacter != null && unlockedOutfitListForCharacter.Count > 0)
			{
				row = num2;
				return;
			}
		}
		foreach (int num4 in array)
		{
			List<Outfit> unlockedOutfitListForCharacter2 = GetUnlockedOutfitListForCharacter(characterUsing, outfitRowControllers[num4].OutfitType);
			if (unlockedOutfitListForCharacter2 != null && unlockedOutfitListForCharacter2.Count > 0)
			{
				row = num4;
				break;
			}
		}
	}

	public void Hide()
	{
		OutfitRowController[] array = outfitRowControllers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Hide();
		}
		backgroundSprite.enabled = false;
	}

	private List<Outfit> GetUnlockedOutfitListForCharacter(Character character, Outfit.OutfitType outfitType)
	{
		if (OutfitManager.characterOutfitsUnlocked.ContainsKey(character))
		{
			return OutfitManager.characterOutfitsUnlocked[character][(int)outfitType];
		}
		return null;
	}

	private List<Outfit> GetAllOutfitListForCharacter(Character character, Outfit.OutfitType outfitType)
	{
		if (OutfitManager.characterOutfitsAll.ContainsKey(character))
		{
			return OutfitManager.characterOutfitsAll[character][(int)outfitType];
		}
		return null;
	}

	private Outfit GetOutfitForCharacter(Character character, Outfit.OutfitType outfitType, int outfitIndex)
	{
		return GetUnlockedOutfitListForCharacter(character, outfitType)?[outfitIndex];
	}

	private void updateImages()
	{
		for (int i = 0; i < numRows; i++)
		{
			outfitRowControllers[i].Select(i == row);
		}
		OutfitRowController[] array = outfitRowControllers;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Show();
		}
		if (OutfitManager.characterOutfitsUnlocked.ContainsKey(player.CharacterInstance))
		{
			UpdateNumOutfits();
			for (int k = 0; k < numRows; k++)
			{
				OutfitRowController outfitRowController = outfitRowControllers[k];
				int outfitType = (int)outfitRowController.OutfitType;
				outfitRowController.setText(column[outfitType] + "/" + numOutfits[outfitType]);
				if (numOutfits[outfitType] > 0 && column[outfitType] != 0)
				{
					outfitRowController.setOutfitImage(GetOutfitForCharacter(player.CharacterInstance, outfitRowController.OutfitType, column[outfitType] - 1));
				}
				else
				{
					outfitRowController.setOutfitImage(nullSprite, nullSpriteColor, nullSpriteScale, Vector3.zero);
				}
			}
			if (OutfitManager.characterOutfitsUnlocked.ContainsKey(player.CharacterInstance))
			{
				for (int l = 0; l < Outfit.NumOutfitTypes; l++)
				{
					SetOutfitsOnOff(column[l] - 1, GetUnlockedOutfitListForCharacter(player.CharacterInstance, (Outfit.OutfitType)l));
				}
			}
			player.CharacterInstance.TellEverybodyAboutOutfits();
		}
		else
		{
			array = outfitRowControllers;
			foreach (OutfitRowController obj in array)
			{
				obj.setText("0\\0");
				obj.setOutfitImage(nullSprite, nullSpriteColor, nullSpriteScale, Vector3.zero);
			}
		}
		player.CharacterInstance.OnOutfitsUpdated();
	}

	private void SetOutfitsOnOff(int whichOutfit, List<Outfit> listOufits)
	{
		if (listOufits == null)
		{
			return;
		}
		for (int i = 0; i < listOufits.Count; i++)
		{
			if (i == whichOutfit)
			{
				listOufits[i].on = true;
			}
			else
			{
				listOufits[i].on = false;
			}
		}
	}

	public virtual void Release(bool unFreeze = true)
	{
		outfitChangeProp.Release(unFreeze);
		if (player.LocalPlayer.AssociatedLobbyPlayer != null)
		{
			player.LocalPlayer.AssociatedLobbyPlayer.CallCmdSetOutfitsFromArray(characterUsing.GetOutfitsAsArray());
		}
		UseController = null;
		if (characterUsing != null && unFreeze)
		{
			characterUsing.InMenu = false;
			characterUsing.Unfreeze();
		}
	}

	private void Update()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad && UseController != null)
		{
			Release();
		}
	}
}
