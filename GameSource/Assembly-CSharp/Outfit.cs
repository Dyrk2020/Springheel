using GameEvent;
using UnityEngine;

public class Outfit : MonoBehaviour, IGameEventListener
{
	public enum OutfitType
	{
		Accessory,
		Body,
		Head,
		Skin,
		FollowOutfit,
		Zombie
	}

	public enum Chicken
	{
		Mohawk = 1,
		Suspenders = 2,
		FootballHelmet = 4,
		Arara = 8,
		Zombie = 0x10
	}

	public enum Horse
	{
		TopHat = 1,
		Bowtie = 2,
		Zebra = 4,
		RaceDay = 8,
		Zombie = 0x10
	}

	public enum Sheep
	{
		PartyHat = 1,
		Sash = 2,
		Sunglasses = 4,
		BlackSheep = 8,
		Zombie = 0x10
	}

	public enum Raccoon
	{
		Crown = 1,
		Necktie = 2,
		Hoodie = 4,
		RedPanda = 8,
		Zombie = 0x10
	}

	public enum Chameleon
	{
		Goggles = 1,
		Cape = 2,
		Shirt = 4,
		Axolotl = 8,
		Zombie = 0x10
	}

	public enum Squirrel
	{
		Headphones = 1,
		RainBoots = 2,
		WingSuit = 4,
		Skunk = 8,
		Zombie = 0x10
	}

	public enum Bunny
	{
		Propeller = 1,
		Tutu = 2,
		Umbrella = 4,
		RegularBunny = 8,
		Zombie = 0x10
	}

	public enum Elephant
	{
		JaneFonda_Body = 1,
		JaneFonda_Head = 2,
		Oxpecker = 4,
		Mammoth = 8,
		Zombie = 0x10
	}

	public enum Monkey
	{
		Letterman = 1,
		ChefHat = 2,
		Backpack = 4,
		RobotMonkey = 8,
		Zombie = 0x10
	}

	public enum Snake
	{
		Scarf = 1,
		Cap = 2,
		Hoverboard = 4,
		Cobra = 8,
		Zombie = 0x10
	}

	public enum Hippo
	{
		Boots = 1,
		Floater = 2,
		Balloon = 4,
		Triceratops = 8,
		Zombie = 0x10
	}

	public enum Turtle
	{
		NinjaMask = 1,
		TrojanHelmet = 2,
		PirateSuit = 4,
		Armadillo = 8,
		Zombie = 0x10
	}

	public enum Panda
	{
		MailHat = 1,
		MailBag = 2,
		VikingHat = 4,
		Bear = 8,
		Zombie = 0x10,
		VikingBody = 0x20
	}

	public enum Fox
	{
		Headband = 1,
		Scarf = 2,
		FireTail = 4,
		DireWolf = 8,
		Zombie = 0x10
	}

	public enum Platypus
	{
		Pan = 1,
		Hood = 2,
		JesterBoots = 4,
		Toucan = 8,
		Zombie = 0x10
	}

	public static readonly int NumOutfitTypes = 4;

	public string outfitString;

	public OutfitType outfitType;

	public bool on;

	public bool Unlocked;

	public bool TempUnlocked;

	public int UnlockNumber;

	public Sprite UISprite;

	public Vector2 UISpriteOffset;

	public float UISpriteScale = 1f;

	public Character.Animals AssociatedCharacter;

	public bool BehindLayer;

	public int LayerNumber = 1;

	public bool SpecialLayering;

	public bool hidesAnimalBody;

	public bool ZombieEyesForSkinX;

	public bool specialCharacterSoundFx;

	public Outfit followThisOutfit;

	public Sprite[] outputSprites;

	public bool[] InvertFrontBack;

	public GameObject outputSpriteGameObject;

	public SpriteRenderer outputSpriteRenderer;

	[Range(-0.5f, 0.5f)]
	public float hueShift;

	[Range(-1f, 1f)]
	public float saturationShift;

	[Range(-1f, 1f)]
	public float valueShift;

	[Range(-3f, 3f)]
	public float contrastShift = 1f;

	public bool colorize;

	[HideInInspector]
	public Chicken ChickenPart;

	[HideInInspector]
	public Horse HorsePart;

	[HideInInspector]
	public Sheep SheepPart;

	[HideInInspector]
	public Raccoon RaccoonPart;

	[HideInInspector]
	public Chameleon ChameleonPart;

	[HideInInspector]
	public Squirrel SquirrelPart;

	[HideInInspector]
	public Bunny BunnyPart;

	[HideInInspector]
	public Elephant ElephantPart;

	[HideInInspector]
	public Monkey MonkeyPart;

	[HideInInspector]
	public Snake SnakePart;

	[HideInInspector]
	public Hippo HippoPart;

	[HideInInspector]
	public Turtle TurtlePart;

	[HideInInspector]
	public Panda PandaPart;

	[HideInInspector]
	public Fox FoxPart;

	[HideInInspector]
	public Platypus PlatypusPart;

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

	private void Awake()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		int num = 0;
		if (AssociatedCharacter == Character.Animals.CHICKEN)
		{
			num = (int)ChickenPart;
		}
		if (AssociatedCharacter == Character.Animals.HORSE)
		{
			num = (int)HorsePart;
		}
		if (AssociatedCharacter == Character.Animals.SHEEP)
		{
			num = (int)SheepPart;
		}
		if (AssociatedCharacter == Character.Animals.RACCOON)
		{
			num = (int)RaccoonPart;
		}
		if (AssociatedCharacter == Character.Animals.CHAMELEON)
		{
			num = (int)ChameleonPart;
		}
		if (AssociatedCharacter == Character.Animals.SQUIRREL)
		{
			num = (int)SquirrelPart;
		}
		if (AssociatedCharacter == Character.Animals.ROBOT)
		{
			num = (int)BunnyPart;
		}
		if (AssociatedCharacter == Character.Animals.ELEPHANT)
		{
			num = (int)ElephantPart;
		}
		if (AssociatedCharacter == Character.Animals.MONKEY)
		{
			num = (int)MonkeyPart;
		}
		if (AssociatedCharacter == Character.Animals.SNAKE)
		{
			num = (int)SnakePart;
		}
		if (AssociatedCharacter == Character.Animals.HIPPO)
		{
			num = (int)HippoPart;
		}
		if (AssociatedCharacter == Character.Animals.TURTLE)
		{
			num = (int)TurtlePart;
		}
		if (AssociatedCharacter == Character.Animals.PANDA)
		{
			num = (int)PandaPart;
		}
		if (AssociatedCharacter == Character.Animals.FOX)
		{
			num = (int)FoxPart;
		}
		if (AssociatedCharacter == Character.Animals.PLATYPUS)
		{
			num = (int)PlatypusPart;
		}
		StatCountArray stat = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatCountArray>("OutfitsUnlocked");
		Unlocked = (stat.values[(int)AssociatedCharacter] & num) > 0;
		ChangeListeners(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListeners(adding: false);
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(CheatUnlockEvent))
		{
			Unlocked = true;
		}
	}

	public void ApplyHSVMaterialProperties(SpriteRenderer targetSpriteRenderer)
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		targetSpriteRenderer.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetFloat("_HueShiftAmount", hueShift);
		materialPropertyBlock.SetFloat("_SatShiftAmount", saturationShift);
		materialPropertyBlock.SetFloat("_ValShiftAmount", valueShift);
		materialPropertyBlock.SetFloat("_ContrastShiftAmount", contrastShift);
		materialPropertyBlock.SetFloat("_Colorize", (!colorize) ? 1 : 0);
		targetSpriteRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
