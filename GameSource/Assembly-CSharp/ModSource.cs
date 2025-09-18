using System;
using System.Xml;
using UnityEngine;

[Serializable]
public class ModSource
{
	public int GravityMode;

	public int JumpSpeedMode;

	public bool WallJumpsDisabled;

	public bool WallSlidesDisabled;

	public int SprintSpeedMode;

	public int GameSpeedMode;

	public bool DanceInvincibility;

	public int InvisibilityMode;

	public bool MirrorControls;

	public int PlatformSpeedMode;

	public int RateOfFireMode;

	public int MultiJumpMode;

	public int ProjectileExplosionMode;

	public int CharacterSizeMode;

	public bool JetpackMode;

	public int PostDeathBehaviorMode;

	public int CameraFlipMode;

	public int DoomsdayMeteorsMode;

	public int DoomsdayLavaMode;

	public bool PlayerPlayerCollisions;

	public int ProjectileSpeedMode;

	public bool PreviewModsInTreehouse;

	public bool ForceLobbyModifiers;

	public bool Frictionless;

	public void ReadFromModSettings()
	{
		Modifiers instance = Modifiers.GetInstance();
		GravityMode = instance.GravityMode;
		JumpSpeedMode = instance.JumpSpeedMode;
		SprintSpeedMode = instance.SprintSpeedMode;
		WallJumpsDisabled = instance.wallJumpsDisabled;
		WallSlidesDisabled = instance.wallSlidesDisabled;
		GameSpeedMode = instance.GameSpeedMode;
		DanceInvincibility = instance.danceInvincibility;
		InvisibilityMode = instance.invisibilityMode;
		MirrorControls = instance.mirrorControls;
		PlatformSpeedMode = instance.PlatformSpeedMode;
		RateOfFireMode = instance.RateOfFireMode;
		MultiJumpMode = instance.MultiJumpMode;
		ProjectileExplosionMode = instance.ProjectileExplosionMode;
		CharacterSizeMode = instance.CharacterSizeMode;
		JetpackMode = instance.jetpackMode;
		PostDeathBehaviorMode = instance.PostDeathBehaviorMode;
		CameraFlipMode = instance.CameraFlipMode;
		DoomsdayMeteorsMode = instance.DoomsdayMeteorsMode;
		DoomsdayLavaMode = instance.DoomsdayLavaMode;
		PlayerPlayerCollisions = instance.playerPlayerCollisions;
		ProjectileSpeedMode = instance.ProjectileSpeedMode;
		PreviewModsInTreehouse = instance.modsPreview;
		ForceLobbyModifiers = instance.forceLobbyModifiers;
		Frictionless = instance.frictionless;
	}

	public void WriteToModSettings(bool includeTreehouseSettings = true)
	{
		Modifiers instance = Modifiers.GetInstance();
		instance.GravityMode = GravityMode;
		instance.JumpSpeedMode = JumpSpeedMode;
		instance.SprintSpeedMode = SprintSpeedMode;
		instance.wallJumpsDisabled = WallJumpsDisabled;
		instance.wallSlidesDisabled = WallSlidesDisabled;
		instance.GameSpeedMode = GameSpeedMode;
		instance.danceInvincibility = DanceInvincibility;
		instance.invisibilityMode = InvisibilityMode;
		instance.mirrorControls = MirrorControls;
		instance.PlatformSpeedMode = PlatformSpeedMode;
		instance.RateOfFireMode = RateOfFireMode;
		instance.MultiJumpMode = MultiJumpMode;
		instance.ProjectileExplosionMode = ProjectileExplosionMode;
		instance.CharacterSizeMode = CharacterSizeMode;
		instance.jetpackMode = JetpackMode;
		instance.PostDeathBehaviorMode = PostDeathBehaviorMode;
		instance.CameraFlipMode = CameraFlipMode;
		instance.DoomsdayMeteorsMode = DoomsdayMeteorsMode;
		instance.DoomsdayLavaMode = DoomsdayLavaMode;
		instance.playerPlayerCollisions = PlayerPlayerCollisions;
		instance.ProjectileSpeedMode = ProjectileSpeedMode;
		if (includeTreehouseSettings)
		{
			instance.modsPreview = PreviewModsInTreehouse;
			instance.forceLobbyModifiers = ForceLobbyModifiers;
		}
		instance.frictionless = Frictionless;
	}

	public bool IsCurrentlyApplied()
	{
		Modifiers instance = Modifiers.GetInstance();
		if (instance.GravityMode != GravityMode)
		{
			return false;
		}
		if (instance.JumpSpeedMode != JumpSpeedMode)
		{
			return false;
		}
		if (instance.SprintSpeedMode != SprintSpeedMode)
		{
			return false;
		}
		if (instance.wallJumpsDisabled != WallJumpsDisabled)
		{
			return false;
		}
		if (instance.wallSlidesDisabled != WallSlidesDisabled)
		{
			return false;
		}
		if (instance.GameSpeedMode != GameSpeedMode)
		{
			return false;
		}
		if (instance.danceInvincibility != DanceInvincibility)
		{
			return false;
		}
		if (instance.invisibilityMode != InvisibilityMode)
		{
			return false;
		}
		if (instance.mirrorControls != MirrorControls)
		{
			return false;
		}
		if (instance.PlatformSpeedMode != PlatformSpeedMode)
		{
			return false;
		}
		if (instance.RateOfFireMode != RateOfFireMode)
		{
			return false;
		}
		if (instance.MultiJumpMode != MultiJumpMode)
		{
			return false;
		}
		if (instance.ProjectileExplosionMode != ProjectileExplosionMode)
		{
			return false;
		}
		if (instance.CharacterSizeMode != CharacterSizeMode)
		{
			return false;
		}
		if (instance.jetpackMode != JetpackMode)
		{
			return false;
		}
		if (instance.PostDeathBehaviorMode != PostDeathBehaviorMode)
		{
			return false;
		}
		if (instance.CameraFlipMode != CameraFlipMode)
		{
			return false;
		}
		if (instance.DoomsdayMeteorsMode != DoomsdayMeteorsMode)
		{
			return false;
		}
		if (instance.DoomsdayLavaMode != DoomsdayMeteorsMode)
		{
			return false;
		}
		if (instance.playerPlayerCollisions != PlayerPlayerCollisions)
		{
			return false;
		}
		if (instance.ProjectileSpeedMode != ProjectileSpeedMode)
		{
			return false;
		}
		if (instance.modsPreview != PreviewModsInTreehouse)
		{
			return false;
		}
		if (instance.forceLobbyModifiers != ForceLobbyModifiers)
		{
			return false;
		}
		if (instance.frictionless != Frictionless)
		{
			return false;
		}
		return true;
	}

	public bool HasDefaultValues()
	{
		return CompareTo(new ModSource());
	}

	public bool CompareTo(ModSource other)
	{
		if (other.GravityMode != GravityMode)
		{
			return false;
		}
		if (other.JumpSpeedMode != JumpSpeedMode)
		{
			return false;
		}
		if (other.SprintSpeedMode != SprintSpeedMode)
		{
			return false;
		}
		if (other.WallJumpsDisabled != WallJumpsDisabled)
		{
			return false;
		}
		if (other.WallSlidesDisabled != WallSlidesDisabled)
		{
			return false;
		}
		if (other.GameSpeedMode != GameSpeedMode)
		{
			return false;
		}
		if (other.DanceInvincibility != DanceInvincibility)
		{
			return false;
		}
		if (other.InvisibilityMode != InvisibilityMode)
		{
			return false;
		}
		if (other.MirrorControls != MirrorControls)
		{
			return false;
		}
		if (other.PlatformSpeedMode != PlatformSpeedMode)
		{
			return false;
		}
		if (other.RateOfFireMode != RateOfFireMode)
		{
			return false;
		}
		if (other.MultiJumpMode != MultiJumpMode)
		{
			return false;
		}
		if (other.ProjectileExplosionMode != ProjectileExplosionMode)
		{
			return false;
		}
		if (other.CharacterSizeMode != CharacterSizeMode)
		{
			return false;
		}
		if (other.JetpackMode != JetpackMode)
		{
			return false;
		}
		if (other.PostDeathBehaviorMode != PostDeathBehaviorMode)
		{
			return false;
		}
		if (other.CameraFlipMode != CameraFlipMode)
		{
			return false;
		}
		if (other.DoomsdayMeteorsMode != DoomsdayMeteorsMode)
		{
			return false;
		}
		if (other.DoomsdayLavaMode != DoomsdayMeteorsMode)
		{
			return false;
		}
		if (other.PlayerPlayerCollisions != PlayerPlayerCollisions)
		{
			return false;
		}
		if (other.ProjectileSpeedMode != ProjectileSpeedMode)
		{
			return false;
		}
		if (other.PreviewModsInTreehouse != PreviewModsInTreehouse)
		{
			return false;
		}
		if (other.ForceLobbyModifiers != ForceLobbyModifiers)
		{
			return false;
		}
		if (other.Frictionless != Frictionless)
		{
			return false;
		}
		return true;
	}

	public void ReadFromXmlNode(XmlNode child)
	{
		GravityMode = QuickSaver.ParseAttrInt(child, "GravityMode");
		JumpSpeedMode = QuickSaver.ParseAttrInt(child, "JumpSpeedMode");
		SprintSpeedMode = QuickSaver.ParseAttrInt(child, "SprintSpeedMode");
		WallJumpsDisabled = QuickSaver.ParseAttrBool(child, "WallJumpsDisabled");
		WallSlidesDisabled = QuickSaver.ParseAttrBool(child, "WallSlidesDisabled");
		GameSpeedMode = QuickSaver.ParseAttrInt(child, "GameSpeedMode");
		DanceInvincibility = QuickSaver.ParseAttrBool(child, "DanceInvincibility");
		InvisibilityMode = QuickSaver.ParseAttrInt(child, "InvisibilityMode");
		MirrorControls = QuickSaver.ParseAttrBool(child, "MirrorControls");
		PlatformSpeedMode = QuickSaver.ParseAttrInt(child, "PlatformSpeedMode");
		RateOfFireMode = QuickSaver.ParseAttrInt(child, "RateOfFireMode");
		MultiJumpMode = QuickSaver.ParseAttrInt(child, "MultiJumpMode");
		ProjectileExplosionMode = QuickSaver.ParseAttrInt(child, "ProjectileExplosionMode");
		CharacterSizeMode = QuickSaver.ParseAttrInt(child, "CharacterSizeMode");
		JetpackMode = QuickSaver.ParseAttrBool(child, "JetpackMode");
		PostDeathBehaviorMode = QuickSaver.ParseAttrInt(child, "PostDeathBehaviorMode");
		CameraFlipMode = QuickSaver.ParseAttrInt(child, "CameraFlipMode");
		DoomsdayMeteorsMode = QuickSaver.ParseAttrInt(child, "DoomsdayMeteorsMode");
		DoomsdayLavaMode = QuickSaver.ParseAttrInt(child, "DoomsdayLavaMode");
		PlayerPlayerCollisions = QuickSaver.ParseAttrBool(child, "PlayerPlayerCollisions");
		ProjectileSpeedMode = QuickSaver.ParseAttrInt(child, "ProjectileSpeedMode");
		PreviewModsInTreehouse = QuickSaver.ParseAttrBool(child, "PreviewModsInTreehouse");
		ForceLobbyModifiers = QuickSaver.ParseAttrBool(child, "ForceLobbyModifiers");
		Frictionless = QuickSaver.ParseAttrBool(child, "Frictionless");
		ClampNumericValues();
	}

	public void WriteToXmlNode(XmlDocument doc, XmlElement modsNode)
	{
		QuickSaver.AddAttribute(doc, modsNode, "GravityMode", GravityMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "JumpSpeedMode", JumpSpeedMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "SprintSpeedMode", SprintSpeedMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "WallJumpsDisabled", WallJumpsDisabled.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "WallSlidesDisabled", WallSlidesDisabled.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "GameSpeedMode", GameSpeedMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "DanceInvincibility", DanceInvincibility.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "InvisibilityMode", InvisibilityMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "MirrorControls", MirrorControls.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "PlatformSpeedMode", PlatformSpeedMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "RateOfFireMode", RateOfFireMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "MultiJumpMode", MultiJumpMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "ProjectileExplosionMode", ProjectileExplosionMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "CharacterSizeMode", CharacterSizeMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "JetpackMode", JetpackMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "PostDeathBehaviorMode", PostDeathBehaviorMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "CameraFlipMode", CameraFlipMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "DoomsdayMeteorsMode", DoomsdayMeteorsMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "DoomsdayLavaMode", DoomsdayLavaMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "PlayerPlayerCollisions", PlayerPlayerCollisions.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "ProjectileSpeedMode", ProjectileSpeedMode.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "PreviewModsInTreehouse", PreviewModsInTreehouse.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "ForceLobbyModifiers", ForceLobbyModifiers.ToString());
		QuickSaver.AddAttribute(doc, modsNode, "Frictionless", Frictionless.ToString());
	}

	public void ClampNumericValues()
	{
		Modifiers instance = Modifiers.GetInstance();
		GravityMode = Mathf.Clamp(GravityMode, 0, instance.GravityValues.Length - 1);
		JumpSpeedMode = Mathf.Clamp(JumpSpeedMode, 0, instance.JumpSpeedValues.Length - 1);
		SprintSpeedMode = Mathf.Clamp(SprintSpeedMode, 0, instance.SprintModifiers.Length - 1);
		GameSpeedMode = Mathf.Clamp(GameSpeedMode, 0, instance.GameSpeedValues.Length - 1);
		InvisibilityMode = Mathf.Clamp(InvisibilityMode, 0, 3);
		PlatformSpeedMode = Mathf.Clamp(PlatformSpeedMode, 0, instance.PlatformMoveSpeedValues.Length - 1);
		RateOfFireMode = Mathf.Clamp(RateOfFireMode, 0, instance.RateOfFireValues.Length - 1);
		MultiJumpMode = Mathf.Clamp(MultiJumpMode, 0, instance.MultiJumpValues.Length - 1);
		ProjectileExplosionMode = Mathf.Clamp(ProjectileExplosionMode, 0, instance.ProjectileExplosionScales.Length - 1);
		CharacterSizeMode = Mathf.Clamp(CharacterSizeMode, 0, instance.CharacterScales.Length - 1);
		PostDeathBehaviorMode = Mathf.Clamp(PostDeathBehaviorMode, 0, 3);
		CameraFlipMode = Mathf.Clamp(CameraFlipMode, 0, 3);
		DoomsdayMeteorsMode = Mathf.Clamp(DoomsdayMeteorsMode, 0, instance.DoomsdayModifierTimes.Length - 1);
		DoomsdayLavaMode = Mathf.Clamp(DoomsdayLavaMode, 0, instance.DoomsdayModifierTimes.Length - 1);
		ProjectileSpeedMode = Mathf.Clamp(ProjectileSpeedMode, 0, instance.ProjectileSpeedValues.Length - 1);
	}
}
