using I2.Loc;
using UnityEngine;

public class UnLockInfo : MonoBehaviour
{
	public enum UnlockType
	{
		Character,
		Outfit,
		Level
	}

	public Sprite itemSprite;

	public UnlockType unlockType;

	public string DisplayName = "";

	[HideInInspector]
	public Character.Animals AssociatedCharacter;

	[HideInInspector]
	public GameState.LevelName AssociatedLevel;

	public bool IsLocal;

	public int forPlayerLocalNumber;

	[HideInInspector]
	public Outfit.Chicken ChickenPart;

	[HideInInspector]
	public Outfit.Horse HorsePart;

	[HideInInspector]
	public Outfit.Sheep SheepPart;

	[HideInInspector]
	public Outfit.Raccoon RaccoonPart;

	[HideInInspector]
	public Outfit.Chameleon ChameleonPart;

	[HideInInspector]
	public Outfit.Squirrel SquirrelPart;

	[HideInInspector]
	public Outfit.Bunny BunnyPart;

	[HideInInspector]
	public Outfit.Elephant ElephantPart;

	[HideInInspector]
	public Outfit.Monkey MonkeyPart;

	[HideInInspector]
	public Outfit.Snake SnakePart;

	[HideInInspector]
	public Outfit.Hippo HippoPart;

	[HideInInspector]
	public Outfit.Turtle TurtlePart;

	[HideInInspector]
	public Outfit.Panda PandaPart;

	[HideInInspector]
	public Outfit.Fox FoxPart;

	[HideInInspector]
	public Outfit.Platypus PlatypusPart;

	public string UpperString => GetTextStrings(UpperOrLower: true);

	public string LowerString => GetTextStrings(UpperOrLower: false);

	public string SteamNameString => GetSteamNameString();

	[HideInInspector]
	public int OutfitMaskNumber => AssociatedCharacter switch
	{
		Character.Animals.CHICKEN => (int)ChickenPart, 
		Character.Animals.HORSE => (int)HorsePart, 
		Character.Animals.SHEEP => (int)SheepPart, 
		Character.Animals.RACCOON => (int)RaccoonPart, 
		Character.Animals.CHAMELEON => (int)ChameleonPart, 
		Character.Animals.SQUIRREL => (int)SquirrelPart, 
		Character.Animals.ROBOT => (int)BunnyPart, 
		Character.Animals.ELEPHANT => (int)ElephantPart, 
		Character.Animals.MONKEY => (int)MonkeyPart, 
		Character.Animals.SNAKE => (int)SnakePart, 
		Character.Animals.HIPPO => (int)HippoPart, 
		Character.Animals.TURTLE => (int)TurtlePart, 
		Character.Animals.PANDA => (int)PandaPart, 
		Character.Animals.FOX => (int)FoxPart, 
		Character.Animals.PLATYPUS => (int)PlatypusPart, 
		_ => 0, 
	};

	public string GetTextStrings(bool UpperOrLower)
	{
		string result = "";
		string result2 = "";
		switch (unlockType)
		{
		case UnlockType.Character:
			result2 = ScriptLocalization.UnlockableText.Character_Unlock_Lower_Text;
			result = AssociatedCharacter switch
			{
				Character.Animals.CHICKEN => ScriptLocalization.CharacterNames.Chicken, 
				Character.Animals.HORSE => ScriptLocalization.CharacterNames.Horse, 
				Character.Animals.SHEEP => ScriptLocalization.CharacterNames.Sheep, 
				Character.Animals.RACCOON => ScriptLocalization.CharacterNames.Raccoon, 
				Character.Animals.CHAMELEON => ScriptLocalization.CharacterNames.Chameleon, 
				Character.Animals.SQUIRREL => ScriptLocalization.CharacterNames.Squirrel, 
				Character.Animals.ROBOT => ScriptLocalization.CharacterNames.Bunny, 
				Character.Animals.ELEPHANT => ScriptLocalization.CharacterNames.Elephant, 
				Character.Animals.MONKEY => ScriptLocalization.CharacterNames.Monkey, 
				Character.Animals.SNAKE => ScriptLocalization.CharacterNames.Snake, 
				Character.Animals.HIPPO => ScriptLocalization.CharacterNames.Hippo, 
				Character.Animals.TURTLE => ScriptLocalization.CharacterNames.Turtle, 
				Character.Animals.PANDA => ScriptLocalization.CharacterNames.Panda, 
				Character.Animals.FOX => ScriptLocalization.CharacterNames.Fox, 
				Character.Animals.PLATYPUS => ScriptLocalization.CharacterNames.Platypus, 
				_ => "No Character. You found a bug", 
			};
			break;
		case UnlockType.Outfit:
			result2 = ScriptLocalization.UnlockableText.Outfit_Unlock_Lower_Text;
			switch (AssociatedCharacter)
			{
			case Character.Animals.CHICKEN:
				switch (ChickenPart)
				{
				case Outfit.Chicken.Mohawk:
					result = ScriptLocalization.UnlockableText.Chicken_Mohawk;
					break;
				case Outfit.Chicken.Suspenders:
					result = ScriptLocalization.UnlockableText.Chicken_Suspenders;
					break;
				case Outfit.Chicken.FootballHelmet:
					result = LocalizationManager.GetTranslation("UnlockableText/ChickenFootballHelmet");
					break;
				case Outfit.Chicken.Arara:
					result = ScriptLocalization.UnlockableText.ChickenArara;
					break;
				}
				break;
			case Character.Animals.HORSE:
				switch (HorsePart)
				{
				case Outfit.Horse.TopHat:
					result = ScriptLocalization.UnlockableText.Horse_Top_Hat;
					break;
				case Outfit.Horse.Bowtie:
					result = ScriptLocalization.UnlockableText.Horse_Bowtie;
					break;
				case Outfit.Horse.Zebra:
					result = ScriptLocalization.UnlockableText.HorseZebra;
					break;
				case Outfit.Horse.RaceDay:
					result = ScriptLocalization.UnlockableText.HorseRaceDay;
					break;
				}
				break;
			case Character.Animals.SHEEP:
				switch (SheepPart)
				{
				case Outfit.Sheep.PartyHat:
					result = ScriptLocalization.UnlockableText.Sheep_Party_hat;
					break;
				case Outfit.Sheep.Sash:
					result = ScriptLocalization.UnlockableText.Sheep_Sash;
					break;
				case Outfit.Sheep.Sunglasses:
					result = LocalizationManager.GetTranslation("UnlockableText/SheepGlasses");
					break;
				case Outfit.Sheep.BlackSheep:
					result = LocalizationManager.GetTranslation("UnlockableText/BlackSheep");
					break;
				}
				break;
			case Character.Animals.RACCOON:
				switch (RaccoonPart)
				{
				case Outfit.Raccoon.Crown:
					result = ScriptLocalization.UnlockableText.Raccoon_Crown;
					break;
				case Outfit.Raccoon.Necktie:
					result = ScriptLocalization.UnlockableText.Raccon_NeckTie;
					break;
				case Outfit.Raccoon.Hoodie:
					result = LocalizationManager.GetTranslation("UnlockableText/RaccoonHoodie");
					break;
				case Outfit.Raccoon.RedPanda:
					result = LocalizationManager.GetTranslation("UnlockableText/RedPanda");
					break;
				}
				break;
			case Character.Animals.CHAMELEON:
				switch (ChameleonPart)
				{
				case Outfit.Chameleon.Goggles:
					result = ScriptLocalization.UnlockableText.Chameleon_Goggles;
					break;
				case Outfit.Chameleon.Cape:
					result = ScriptLocalization.UnlockableText.Chameleon_Cape;
					break;
				case Outfit.Chameleon.Shirt:
					result = LocalizationManager.GetTranslation("UnlockableText/ChameleonShirt");
					break;
				case Outfit.Chameleon.Axolotl:
					result = LocalizationManager.GetTranslation("UnlockableText/Axolotl");
					break;
				}
				break;
			case Character.Animals.SQUIRREL:
				switch (SquirrelPart)
				{
				case Outfit.Squirrel.Headphones:
					result = ScriptLocalization.UnlockableText.Squirrel_Headphones;
					break;
				case Outfit.Squirrel.RainBoots:
					result = ScriptLocalization.UnlockableText.Squirrel_Rainboots;
					break;
				case Outfit.Squirrel.WingSuit:
					result = LocalizationManager.GetTranslation("UnlockableText/SquirrelWingsuit");
					break;
				case Outfit.Squirrel.Skunk:
					result = LocalizationManager.GetTranslation("UnlockableText/Skunk");
					break;
				}
				break;
			case Character.Animals.ROBOT:
				switch (BunnyPart)
				{
				case Outfit.Bunny.Propeller:
					result = ScriptLocalization.UnlockableText.Bunny_Propeller;
					break;
				case Outfit.Bunny.Tutu:
					result = ScriptLocalization.UnlockableText.Bunny_Tutu;
					break;
				case Outfit.Bunny.Umbrella:
					result = LocalizationManager.GetTranslation("UnlockableText/BunnyUmbrella");
					break;
				case Outfit.Bunny.RegularBunny:
					result = LocalizationManager.GetTranslation("UnlockableText/RegularBunny");
					break;
				}
				break;
			case Character.Animals.ELEPHANT:
				switch (ElephantPart)
				{
				case Outfit.Elephant.JaneFonda_Body:
					result = LocalizationManager.GetTranslation("UnlockableText/ElephantAerobics");
					break;
				case Outfit.Elephant.JaneFonda_Head:
					result = LocalizationManager.GetTranslation("UnlockableText/ElephantHeadband");
					break;
				case Outfit.Elephant.Oxpecker:
					result = LocalizationManager.GetTranslation("UnlockableText/ElephantOxpecker");
					break;
				case Outfit.Elephant.Mammoth:
					result = LocalizationManager.GetTranslation("UnlockableText/Mammoth");
					break;
				}
				break;
			case Character.Animals.MONKEY:
				switch (MonkeyPart)
				{
				case Outfit.Monkey.Letterman:
					result = LocalizationManager.GetTranslation("UnlockableText/MonkeyLetterman");
					break;
				case Outfit.Monkey.ChefHat:
					result = LocalizationManager.GetTranslation("UnlockableText/MonkeyChefHat");
					break;
				case Outfit.Monkey.Backpack:
					result = LocalizationManager.GetTranslation("UnlockableText/MonkeyBackpack");
					break;
				case Outfit.Monkey.RobotMonkey:
					result = LocalizationManager.GetTranslation("UnlockableText/RobotMonkey");
					break;
				}
				break;
			case Character.Animals.SNAKE:
				switch (SnakePart)
				{
				case Outfit.Snake.Scarf:
					result = ScriptLocalization.UnlockableText.SnakeScarf;
					break;
				case Outfit.Snake.Cap:
					result = ScriptLocalization.UnlockableText.SnakeCap;
					break;
				case Outfit.Snake.Hoverboard:
					result = ScriptLocalization.UnlockableText.SnakeHoverboard;
					break;
				case Outfit.Snake.Cobra:
					result = ScriptLocalization.UnlockableText.Cobra;
					break;
				}
				break;
			case Character.Animals.HIPPO:
				switch (HippoPart)
				{
				case Outfit.Hippo.Boots:
					result = ScriptLocalization.UnlockableText.HippoBoots;
					break;
				case Outfit.Hippo.Floater:
					result = ScriptLocalization.UnlockableText.HippoFloaty;
					break;
				case Outfit.Hippo.Balloon:
					result = ScriptLocalization.UnlockableText.HippoBalloon;
					break;
				case Outfit.Hippo.Triceratops:
					result = ScriptLocalization.UnlockableText.Triceratops;
					break;
				}
				break;
			case Character.Animals.TURTLE:
				switch (TurtlePart)
				{
				case Outfit.Turtle.NinjaMask:
					result = ScriptLocalization.UnlockableText.TurtleNinjaStripes;
					break;
				case Outfit.Turtle.TrojanHelmet:
					result = ScriptLocalization.UnlockableText.TurtleHelmet;
					break;
				case Outfit.Turtle.PirateSuit:
					result = ScriptLocalization.UnlockableText.TurtlePirateSuit;
					break;
				case Outfit.Turtle.Armadillo:
					result = ScriptLocalization.UnlockableText.Armadillo;
					break;
				}
				break;
			case Character.Animals.PANDA:
				switch (PandaPart)
				{
				case Outfit.Panda.MailHat:
					result = ScriptLocalization.UnlockableText.PandaMailHat;
					break;
				case Outfit.Panda.MailBag:
					result = ScriptLocalization.UnlockableText.PandaMailBag;
					break;
				case Outfit.Panda.VikingHat:
					result = ScriptLocalization.UnlockableText.PandaVikingHat;
					break;
				case Outfit.Panda.VikingBody:
					result = ScriptLocalization.UnlockableText.PandaVikingBody;
					break;
				case Outfit.Panda.Bear:
					result = ScriptLocalization.UnlockableText.Bear;
					break;
				}
				break;
			case Character.Animals.FOX:
				switch (FoxPart)
				{
				case Outfit.Fox.Headband:
					result = ScriptLocalization.UnlockableText.FoxHeadband;
					break;
				case Outfit.Fox.Scarf:
					result = ScriptLocalization.UnlockableText.FoxScarf;
					break;
				case Outfit.Fox.FireTail:
					result = ScriptLocalization.UnlockableText.FoxFireTail;
					break;
				case Outfit.Fox.DireWolf:
					result = ScriptLocalization.UnlockableText.DireWolf;
					break;
				}
				break;
			case Character.Animals.PLATYPUS:
				switch (PlatypusPart)
				{
				case Outfit.Platypus.Pan:
					result = ScriptLocalization.UnlockableText.PlatypusPan;
					break;
				case Outfit.Platypus.Hood:
					result = ScriptLocalization.UnlockableText.PlatypusHood;
					break;
				case Outfit.Platypus.JesterBoots:
					result = ScriptLocalization.UnlockableText.PlatypusJesterBoots;
					break;
				case Outfit.Platypus.Toucan:
					result = ScriptLocalization.UnlockableText.Toucan;
					break;
				}
				break;
			}
			break;
		case UnlockType.Level:
			result = AssociatedLevel switch
			{
				GameState.LevelName.FARM => ScriptLocalization.LevelNames.The_Farm, 
				GameState.LevelName.ROOFTOPS => ScriptLocalization.LevelNames.Rooftops, 
				GameState.LevelName.OLDMANSION => ScriptLocalization.LevelNames.Old_House, 
				GameState.LevelName.WATERFALL => ScriptLocalization.LevelNames.Waterfall, 
				GameState.LevelName.PYRAMID => ScriptLocalization.LevelNames.Desert, 
				GameState.LevelName.WINDMILL => ScriptLocalization.LevelNames.Windmill, 
				GameState.LevelName.METALPLANT => ScriptLocalization.LevelNames.Metal_Plant, 
				GameState.LevelName.ICEBERG => ScriptLocalization.LevelNames.Iceberg, 
				GameState.LevelName.DANCEPARTY => ScriptLocalization.LevelNames.Dance_Party, 
				GameState.LevelName.PIER => ScriptLocalization.LevelNames.The_Pier, 
				GameState.LevelName.BLANKLEVEL => ScriptLocalization.LevelNames.Blank_Level, 
				GameState.LevelName.JUNGLETEMPLE => LocalizationManager.GetTranslation("LevelNames/Jungle"), 
				GameState.LevelName.VOLCANO => LocalizationManager.GetTranslation("LevelNames/Volcano"), 
				GameState.LevelName.CRUMBLINGBRIDGE => LocalizationManager.GetTranslation("LevelNames/CrumblingBridge"), 
				GameState.LevelName.NUCLEARPLANT => LocalizationManager.GetTranslation("LevelNames/NuclearPlant"), 
				GameState.LevelName.TRONLEVEL => LocalizationManager.GetTranslation("LevelNames/TronLevel"), 
				GameState.LevelName.SPACELEVEL => ScriptLocalization.LevelNames.SpaceLevel, 
				GameState.LevelName.BALLROOM => ScriptLocalization.LevelNames.TheBallroom, 
				GameState.LevelName.ROLLERCOASTER => ScriptLocalization.LevelNames.Rollercoaster, 
				GameState.LevelName.METRO => ScriptLocalization.LevelNames.Metro, 
				GameState.LevelName.WATERTOWER => ScriptLocalization.LevelNames.WaterTower, 
				GameState.LevelName.RAFT => ScriptLocalization.LevelNames.Raft, 
				GameState.LevelName.PICTUREFRAME => ScriptLocalization.LevelNames.PictureFrame, 
				GameState.LevelName.SPACESTATION => ScriptLocalization.LevelNames.SpaceStation, 
				_ => "no Level?", 
			};
			result2 = ScriptLocalization.UnlockableText.Level_Unlock_Lower_Text;
			break;
		}
		if (UpperOrLower)
		{
			return result;
		}
		return result2;
	}

	public void SendUnlockedAnalytic()
	{
		float totalPlaytime = 0f;
		SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(forPlayerLocalNumber);
		if (saveFileDataForLocalPlayer != null)
		{
			totalPlaytime = saveFileDataForLocalPlayer.GetStat<StatFloat>("TotalMatchTime").value;
		}
		switch (unlockType)
		{
		case UnlockType.Character:
			AnalyticEvent.CharacterUnlockedEvent(AssociatedCharacter, totalPlaytime);
			break;
		case UnlockType.Outfit:
		{
			int num = 0;
			if (saveFileDataForLocalPlayer != null)
			{
				StatCountArray stat = saveFileDataForLocalPlayer.GetStat<StatCountArray>("OutfitsUnlocked");
				for (int i = 0; i != stat.values.Length; i++)
				{
					int num2 = stat.values[i];
					for (int j = 0; j < 8; j++)
					{
						if ((num2 & (1 << j)) != 0)
						{
							num++;
						}
					}
				}
			}
			AnalyticEvent.OutfitUnlocked(AssociatedCharacter, OutfitMaskNumber, num, totalPlaytime);
			break;
		}
		case UnlockType.Level:
			AnalyticEvent.LevelUnlockedEvent(AssociatedLevel, totalPlaytime);
			break;
		}
	}

	public string GetSteamNameString()
	{
		if (DisplayName == "")
		{
			return "";
		}
		return ScriptLocalization.UnlockableText.ForPlayer + " " + DisplayName;
	}
}
