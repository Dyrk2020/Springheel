using System.Collections.Generic;
using UnityEngine;

public class TagComparer
{
	public enum Tag
	{
		None = 0,
		Untagged = 1,
		Respawn = 2,
		Finish = 4,
		EditorOnly = 8,
		MainCamera = 0x10,
		Player = 0x20,
		GameController = 0x40,
		Solid = 0x80,
		Hazard = 0x100,
		Goal = 0x200,
		NoPlace = 0x400,
		NotWall = 0x800,
		CameraBoundary = 0x1000,
		SolidNotWall = 0x2000,
		PlacementIndicator = 0x4000,
		Attachment = 0x8000,
		Body = 0x10000,
		AttachedPiece = 0x20000,
		NoAttach = 0x40000,
		Deathpit = 0x80000,
		OneWay = 0x100000,
		Attachable = 0x200000,
		Start = 0x400000,
		NoPhysicModifier = 0x800000,
		StartProtection = 0x1000000,
		BlockProjectiles = 0x2000000,
		SpecialButton = 0x4000000,
		Impulse = 0x8000000
	}

	public static string[] PossibleTags = new string[32]
	{
		"Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController", "Solid", "Hazard", "Goal",
		"Solid_NoPlace", "Solid_NotWall", "Solid_Player", "CameraBoundary", "SolidNotWall", "PlacementIndicator", "Attachment", "Player_Body", "AttachedPiece", "NoAttach",
		"Deathpit", "Solid_NoAttach", "OneWay", "Solid_NoPlace_NoAttach", "Solid_NotWall_NoPlace", "Attachable", "Start", "Hazard_NoAttach", "Solid_Hazard_NoAttach_NoPlace", "Solid_Hazard_NoAttach",
		"NoPhysicModifier", "StartProtection"
	};

	public static Dictionary<string, Tag> stringToTagDict = new Dictionary<string, Tag>
	{
		{
			"Untagged",
			Tag.Untagged
		},
		{
			"Respawn",
			Tag.Respawn
		},
		{
			"Finish",
			Tag.Finish
		},
		{
			"EditorOnly",
			Tag.EditorOnly
		},
		{
			"MainCamera",
			Tag.MainCamera
		},
		{
			"Player",
			Tag.Player
		},
		{
			"GameController",
			Tag.GameController
		},
		{
			"Solid",
			Tag.Solid
		},
		{
			"Hazard",
			Tag.Hazard
		},
		{
			"Goal",
			Tag.Goal
		},
		{
			"NoPlace",
			Tag.NoPlace
		},
		{
			"NotWall",
			Tag.NotWall
		},
		{
			"CameraBoundary",
			Tag.CameraBoundary
		},
		{
			"SolidNotWall",
			Tag.SolidNotWall
		},
		{
			"PlacementIndicator",
			Tag.PlacementIndicator
		},
		{
			"Attachment",
			Tag.Attachment
		},
		{
			"Body",
			Tag.Body
		},
		{
			"AttachedPiece",
			Tag.AttachedPiece
		},
		{
			"NoAttach",
			Tag.NoAttach
		},
		{
			"Deathpit",
			Tag.Deathpit
		},
		{
			"OneWay",
			Tag.OneWay
		},
		{
			"Attachable",
			Tag.Attachable
		},
		{
			"Start",
			Tag.Start
		},
		{
			"NoPhysicModifier",
			Tag.NoPhysicModifier
		},
		{
			"StartProtection",
			Tag.StartProtection
		},
		{
			" BlockProjectiles",
			Tag.BlockProjectiles
		},
		{
			" SpecialButton",
			Tag.SpecialButton
		},
		{
			" Has Impulse",
			Tag.Impulse
		}
	};

	public int myTagsMask;

	public int checkTagsMask;

	public int ignoreTagsMask;

	private static Dictionary<string, HashSet<Tag>> compoundTags;

	private static Dictionary<string, HashSet<Tag>> CompoundTags
	{
		get
		{
			if (compoundTags == null)
			{
				compoundTags = new Dictionary<string, HashSet<Tag>>();
				string[] possibleTags = PossibleTags;
				foreach (string text in possibleTags)
				{
					string[] array = text.Split('_');
					HashSet<Tag> hashSet = new HashSet<Tag>();
					string[] array2 = array;
					foreach (string key in array2)
					{
						hashSet.Add(stringToTagDict[key]);
					}
					compoundTags.Add(text, hashSet);
				}
			}
			return compoundTags;
		}
	}

	public void Initialize(int myTagsMask, int checkTagsMask, int ignoreTagsMask)
	{
		this.myTagsMask = myTagsMask;
		this.checkTagsMask = checkTagsMask;
		this.ignoreTagsMask = ignoreTagsMask;
	}

	public static bool DoTagMatch(TagComparer a, TagComparer b, out bool tagMatch)
	{
		tagMatch = false;
		if ((a.ignoreTagsMask & b.myTagsMask) != 0)
		{
			return true;
		}
		if ((a.checkTagsMask & b.myTagsMask) != 0)
		{
			tagMatch = true;
		}
		return false;
	}

	public static bool DoTagMatch(TagComparer a, int bMask, out bool tagMatch)
	{
		tagMatch = false;
		if ((a.ignoreTagsMask & bMask) != 0)
		{
			return true;
		}
		if ((a.checkTagsMask & bMask) != 0)
		{
			tagMatch = true;
		}
		return false;
	}

	public static bool DoTagMatch(TagComparer a, string bTag, out bool tagMatch)
	{
		int maskFromTagString = GetMaskFromTagString(bTag);
		return DoTagMatch(a, maskFromTagString, out tagMatch);
	}

	public static HashSet<Tag> GetSplitTag(string compoundTag)
	{
		if (compoundTag.NullOrEmpty())
		{
			return null;
		}
		if (CompoundTags.TryGetValue(compoundTag, out var value))
		{
			return value;
		}
		Debug.LogError("Warning! Compound tag " + compoundTag + " was not in PossibleTags -- rectifying");
		string[] array = compoundTag.Split('_');
		HashSet<Tag> hashSet = new HashSet<Tag>();
		string[] array2 = array;
		foreach (string key in array2)
		{
			hashSet.Add(stringToTagDict[key]);
		}
		compoundTags.Add(compoundTag, hashSet);
		return hashSet;
	}

	public static bool ShouldIgnoreCollision(TagComparer a, TagComparer b)
	{
		return ShouldIgnoreCollision(a, b.myTagsMask);
	}

	public static bool ShouldIgnoreCollision(TagComparer a, int bMask)
	{
		if ((a.checkTagsMask & bMask) != 0)
		{
			return false;
		}
		if ((a.ignoreTagsMask & bMask) != 0)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldIgnoreCollision(TagComparer a, string bTag)
	{
		int maskFromTagString = GetMaskFromTagString(bTag);
		return ShouldIgnoreCollision(a, maskFromTagString);
	}

	public static int GetMaskFromTagString(string tagString)
	{
		HashSet<Tag> splitTag = GetSplitTag(tagString);
		int num = 0;
		foreach (Tag item in splitTag)
		{
			num |= (int)item;
		}
		return num;
	}

	public static int GetMaskFromTagList(string tagListString)
	{
		string[] array = tagListString.Split(',');
		int num = 0;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			int maskFromTagString = GetMaskFromTagString(array2[i]);
			num |= maskFromTagString;
		}
		return num;
	}
}
