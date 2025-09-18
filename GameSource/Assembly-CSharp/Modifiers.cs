using System;
using System.Collections.Generic;
using System.Text;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Modifiers : ScriptableObject
{
	public enum PostDeathBehaviors
	{
		None,
		Agony,
		Ghost,
		Zombie
	}

	public enum CameraFlipModes
	{
		None,
		FlipX,
		FlipY,
		FlipXY
	}

	public enum GravityType
	{
		NORMAL,
		LOW,
		HIGH
	}

	[Serializable]
	public class PerLevelLavaSettings
	{
		public GameState.LevelName level;

		public DoomsdayLava.Direction lavaDirection;

		public Vector2 extraPadding;

		public float speedMultiplier = 1f;

		public PerLevelLavaSettings(GameState.LevelName level, DoomsdayLava.Direction lavaDirection = DoomsdayLava.Direction.Up, Vector2 extraPadding = default(Vector2), float speedMultiplier = 1f)
		{
			this.level = level;
			this.lavaDirection = lavaDirection;
			this.extraPadding = extraPadding;
			this.speedMultiplier = speedMultiplier;
		}
	}

	public enum InvisibilityModes
	{
		Off,
		WhenMoving,
		WhenStationary,
		Always
	}

	protected static Modifiers instance;

	[Header("Modifiers")]
	public bool modsApplied;

	public bool modsPreview;

	public bool ModsAlwaysOnDebug;

	public bool forceLobbyModifiers;

	public bool competitiveRandomizer;

	public float[] GravityValues = new float[3] { 64.8f, 30f, 100f };

	public float[] JumpSpeedValues = new float[4] { 19f, 14f, 30f, 10f };

	public float DefaultSprintExtraSpeed = 0.6f;

	public float[] SprintModifiers = new float[5] { 1f, 0.5f, 0.7f, 1.5f, 2f };

	public float[] GameSpeedValues = new float[5] { 1f, 0.5f, 0.75f, 1.5f, 2f };

	public float[] PlatformMoveSpeedValues = new float[5] { 1f, 0.25f, 0.5f, 2f, 5f };

	public float[] RotatorSpeedValues = new float[5] { 1f, 0.25f, 0.5f, 2f, 5f };

	public float[] RateOfFireValues = new float[5] { 1f, 0.25f, 0.5f, 2f, 5f };

	public string[] RateOfFireAudioEventStrings = new string[5] { "RULES_Set_Projectile_Rate_Normal", "RULES_Set_Projectile_Rate_Slowest", "RULES_Set_Projectile_Rate_Slower", "RULES_Set_Projectile_Rate_Faster", "RULES_Set_Projectile_Rate_Fastest" };

	public float[] ProjectileSpeedValues = new float[5] { 1f, 0.25f, 0.5f, 2f, 3f };

	public int[] MultiJumpValues = new int[4] { 0, 1, 2, 2147483647 };

	public float[] ProjectileExplosionScales = new float[5] { 0f, 1f, 2f, 5f, 8f };

	public float[] CharacterScales = new float[6] { 1.3f, 0.2f, 0.75f, 2f, 3f, 5f };

	public float[] CharacterSizeSpeedScales = new float[6] { 1f, 1f, 1f, 1.2f, 1.5f, 1.7f };

	public float[] CharacterSizeZoomMultipliers = new float[6] { 1f, 1f, 1f, 1.2f, 1.5f, 2f };

	public float[] CharacterLightCellTriggerRadiusMultipliers = new float[6] { 1f, 1f, 1f, 1.2f, 1.5f, 2f };

	[Header("Basic Jetpack")]
	public float jetpackThrust = 110f;

	public AnimationCurve jetpackVelocityModifier = new AnimationCurve();

	public float JetpackJumpSpeedModifier = 0.2f;

	public float JetpackWallJumpSpeedModifier = 0.2f;

	public float JetpackHorizontalForcedSpeed = 0.9f;

	[Header("Jetpack Death Spiral")]
	public float JetpackDeathSpiralUpAccel = 80f;

	public float JetpackDeathSpiralCenterAccel = 300f;

	public float JetpackDeathSpiralRPS = -1.5f;

	public float JetpackDeathSpiralTime = 2f;

	public float JetpackDeathSpiralNoise = 0.1f;

	public AnimationCurve JetPackDeathSpiralDecay = new AnimationCurve();

	[Header("Jetpack win spiral")]
	public float JetpackWinSpiralUpAccel = 80f;

	public float JetpackWinSpiralCenterAccel = 300f;

	public float JetpackWinSpiralRPS = -1.5f;

	public float JetpackWinSpiralTime = 2f;

	public float JetpackWinSpiralNoise = 0.1f;

	public AnimationCurve JetPackWinSpiralDecay = new AnimationCurve();

	[Header("Agony")]
	public int agonyTimeLimitInvisible = 10;

	public int agonyTimeLimit = 30;

	[Header("Zombie")]
	public float zombificationSpeedMultiplier = 0.25f;

	public float zombieAnimatorSpeed = 0.8f;

	public float zombificationTime = 2.2f;

	public float zombificationJumpSpeedMultiplier = 0.8f;

	[Header("Doomsday")]
	public int[] DoomsdayModifierTimes = new int[14]
	{
		0, 0, 15, 30, 45, 60, 90, 120, 150, 180,
		210, 240, 270, 300
	};

	public List<PerLevelLavaSettings> perLevelLavaSettings;

	public int DoomsdayMeteorsEasySeconds = 10;

	public int DoomsdayMeteorsFullRampUpSeconds = 120;

	public float DoomsdayLavaRiseSpeed = 0.7f;

	[Header("Frictionless")]
	public float FrictionlessFrictionValue = -0.98f;

	[Header("PlayerCollisionsStartInvincibility")]
	public float PlayerCollisionsStartInvincibilityTime = 5f;

	public int GravityMode;

	public int JumpSpeedMode;

	public int SprintSpeedMode;

	public bool wallJumpsDisabled;

	public bool wallSlidesDisabled;

	public int GameSpeedMode;

	public bool danceInvincibility;

	public int invisibilityMode;

	public bool mirrorControls;

	public int PlatformSpeedMode;

	public int RateOfFireMode;

	public int MultiJumpMode;

	public int ProjectileExplosionMode;

	public int CharacterSizeMode;

	public bool jetpackMode;

	public int PostDeathBehaviorMode;

	public int CameraFlipMode;

	public int DoomsdayMeteorsMode;

	public int DoomsdayLavaMode;

	public bool playerPlayerCollisions;

	public int ProjectileSpeedMode;

	public bool frictionless;

	private static ModSource defaultModSource;

	public static bool anyModsPrinted;

	public static StringBuilder stringBuilder;

	public bool ModsApplied
	{
		get
		{
			if (!modsApplied)
			{
				return ModsAlwaysOnDebug;
			}
			return true;
		}
		set
		{
			modsApplied = value;
		}
	}

	public float Gravity => GravityValues[ModsApplied ? GravityMode : 0];

	public float GravityScale => Gravity / GravityValues[0];

	public float JumpSpeed => JumpSpeedValues[ModsApplied ? JumpSpeedMode : 0];

	public float WallJumpHorizontalPush
	{
		get
		{
			float num = SprintSpeed * CharacterSizeSpeedMultiplier;
			if (ModsApplied)
			{
				float num2 = JumpSpeed / JumpSpeedValues[0];
				float num3 = SprintSpeed / SprintModifiers[0];
				return num * (num2 / (GravityScale * num3));
			}
			return num;
		}
	}

	public float WallJumpVerticalPush => JumpSpeed * CharacterSizeSpeedMultiplier;

	public float SprintSpeed => SprintModifiers[ModsApplied ? SprintSpeedMode : 0];

	public bool WallJumpsDisabled
	{
		get
		{
			if (ModsApplied)
			{
				return wallJumpsDisabled;
			}
			return false;
		}
	}

	public bool WallSlidesDisabled => WallJumpsDisabled;

	public float GameSpeed
	{
		get
		{
			if (ModsApplied && LobbyManager.instance != null && (LobbyManager.instance.CurrentGameController == null || LobbyManager.instance.CurrentGameController.NextPhase == GameControl.GamePhase.PLAY))
			{
				return GameSpeedValues[GameSpeedMode];
			}
			return GameSpeedValues[0];
		}
	}

	public bool DanceInvincibility
	{
		get
		{
			if (ModsApplied)
			{
				return danceInvincibility;
			}
			return false;
		}
	}

	public bool InvisibleWhenMoving
	{
		get
		{
			if (ModsApplied)
			{
				InvisibilityModes invisibilityModes = (InvisibilityModes)invisibilityMode;
				if (invisibilityModes == InvisibilityModes.WhenMoving || invisibilityModes == InvisibilityModes.Always)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool InvisibleWhenStationary
	{
		get
		{
			if (ModsApplied)
			{
				InvisibilityModes invisibilityModes = (InvisibilityModes)invisibilityMode;
				if ((uint)(invisibilityModes - 2) <= 1u)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool MirrorControls
	{
		get
		{
			if (ModsApplied)
			{
				return mirrorControls;
			}
			return false;
		}
	}

	public float PlatformMoveSpeed
	{
		get
		{
			if (ModsApplied)
			{
				return PlatformMoveSpeedValues[PlatformSpeedMode];
			}
			return 1f;
		}
	}

	public float RotatorSpeed
	{
		get
		{
			if (ModsApplied)
			{
				return RotatorSpeedValues[PlatformSpeedMode];
			}
			return 1f;
		}
	}

	public float RateOfFire
	{
		get
		{
			if (ModsApplied)
			{
				return RateOfFireValues[RateOfFireMode];
			}
			return 1f;
		}
	}

	public float ProjectileSpeed
	{
		get
		{
			if (ModsApplied)
			{
				return ProjectileSpeedValues[ProjectileSpeedMode];
			}
			return 1f;
		}
	}

	public int MaxAirJumps
	{
		get
		{
			if (ModsApplied)
			{
				return MultiJumpValues[MultiJumpMode];
			}
			return 0;
		}
	}

	public bool ProjectilesExplode
	{
		get
		{
			if (ModsApplied)
			{
				return ProjectileExplosionMode != 0;
			}
			return false;
		}
	}

	public float ProjectileExplosionScale => ProjectileExplosionScales[ProjectileExplosionMode];

	public float CharacterScale
	{
		get
		{
			if (ModsApplied)
			{
				return CharacterScales[CharacterSizeMode];
			}
			return CharacterScales[0];
		}
	}

	public float CharacterRelativeScale
	{
		get
		{
			if (ModsApplied)
			{
				return CharacterScale / CharacterScales[0];
			}
			return 1f;
		}
	}

	public string CharacterScaleAudioStateString
	{
		get
		{
			if (ModsApplied)
			{
				switch (CharacterSizeMode)
				{
				case 1:
					return "Way_Tiny";
				case 2:
					return "Tiny";
				case 3:
					return "Big";
				case 4:
					return "Way_Big";
				case 5:
					return "Huge";
				}
			}
			return "Normal";
		}
	}

	public float CharacterSizeSpeedMultiplier
	{
		get
		{
			if (ModsApplied)
			{
				return CharacterSizeSpeedScales[CharacterSizeMode];
			}
			return 1f;
		}
	}

	public float CharacterSizeZoomMultiplier
	{
		get
		{
			if (ModsApplied)
			{
				return CharacterSizeZoomMultipliers[CharacterSizeMode];
			}
			return 1f;
		}
	}

	public float CharacterLightCellTriggerRadiusMultiplier
	{
		get
		{
			if (ModsApplied)
			{
				return CharacterLightCellTriggerRadiusMultipliers[CharacterSizeMode];
			}
			return 1f;
		}
	}

	public bool JetpackMode
	{
		get
		{
			if (ModsApplied)
			{
				return jetpackMode;
			}
			return false;
		}
	}

	public PostDeathBehaviors PostDeathBehavior
	{
		get
		{
			if (ModsApplied)
			{
				return (PostDeathBehaviors)PostDeathBehaviorMode;
			}
			return PostDeathBehaviors.None;
		}
	}

	public CameraFlipModes CameraFlipping
	{
		get
		{
			if (ModsApplied)
			{
				return (CameraFlipModes)CameraFlipMode;
			}
			return CameraFlipModes.None;
		}
	}

	public bool CameraFlippedOnX
	{
		get
		{
			if (!ModsApplied)
			{
				return false;
			}
			switch ((CameraFlipModes)CameraFlipMode)
			{
			case CameraFlipModes.None:
			case CameraFlipModes.FlipY:
				return false;
			case CameraFlipModes.FlipX:
			case CameraFlipModes.FlipXY:
				return true;
			default:
				return false;
			}
		}
	}

	public bool CameraFlippedOnY
	{
		get
		{
			if (!ModsApplied)
			{
				return false;
			}
			switch ((CameraFlipModes)CameraFlipMode)
			{
			case CameraFlipModes.None:
			case CameraFlipModes.FlipX:
				return false;
			case CameraFlipModes.FlipY:
			case CameraFlipModes.FlipXY:
				return true;
			default:
				return false;
			}
		}
	}

	public bool CameraFlippedOnSingleAxis
	{
		get
		{
			if (!ModsApplied)
			{
				return false;
			}
			switch ((CameraFlipModes)CameraFlipMode)
			{
			case CameraFlipModes.None:
			case CameraFlipModes.FlipXY:
				return false;
			case CameraFlipModes.FlipX:
			case CameraFlipModes.FlipY:
				return true;
			default:
				return false;
			}
		}
	}

	public int DoomsdayMeteorsDelay => DoomsdayModifierTimes[DoomsdayMeteorsMode];

	public int DoomsdayLavaDelay => DoomsdayModifierTimes[DoomsdayLavaMode];

	public bool DoomsdayMeteorsEnabled
	{
		get
		{
			if (ModsApplied)
			{
				return DoomsdayMeteorsMode > 0;
			}
			return false;
		}
	}

	public bool DoomsdayLavaEnabled
	{
		get
		{
			if (ModsApplied)
			{
				return DoomsdayLavaMode > 0;
			}
			return false;
		}
	}

	public bool PlayerPlayerCollisions
	{
		get
		{
			if (ModsApplied)
			{
				return playerPlayerCollisions;
			}
			return false;
		}
	}

	public bool Frictionless
	{
		get
		{
			if (ModsApplied)
			{
				return frictionless;
			}
			return false;
		}
	}

	public static ModSource DefaultModSource
	{
		get
		{
			if (defaultModSource == null)
			{
				defaultModSource = new ModSource();
			}
			return defaultModSource;
		}
	}

	public bool IsNonDefault => !DefaultModSource.IsCurrentlyApplied();

	public bool AppliedAndNonDefault
	{
		get
		{
			if (ModsApplied)
			{
				return IsNonDefault;
			}
			return false;
		}
	}

	public static Modifiers GetInstance()
	{
		if (instance == null)
		{
			instance = (Modifiers)Resources.Load("ModifierSettings");
		}
		return instance;
	}

	public void OnModifiersDynamicChange()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null)
		{
			modsApplied = modsPreview;
			foreach (Character item in LobbyManager.instance.CurrentLevelSelectController.EnumerateCharacters())
			{
				item.RefreshScale();
			}
			LobbyManager.instance.CurrentLevelSelectController.RefreshCharacterPosition();
			Time.timeScale = GameSpeed;
		}
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null)
		{
			Character[] array = UnityEngine.Object.FindObjectsOfType<Character>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshScale();
			}
			Time.timeScale = GameSpeed;
		}
		AkSoundEngine.PostEvent(RateOfFireAudioEventStrings[RateOfFireMode], LobbyManager.instance.gameObject);
	}

	public static string GetGravityValueString(int GravityMode)
	{
		return GravityMode switch
		{
			0 => ScriptLocalization.Modifiers.Normal, 
			1 => ScriptLocalization.Modifiers.Low, 
			2 => ScriptLocalization.Modifiers.High, 
			_ => null, 
		};
	}

	public static string GetJumpSpeedValueString(int JumpSpeedMode)
	{
		return JumpSpeedMode switch
		{
			0 => ScriptLocalization.Modifiers.JumpNormal, 
			1 => ScriptLocalization.Modifiers.JumpLow, 
			2 => ScriptLocalization.Modifiers.JumpHigh, 
			3 => ScriptLocalization.Modifiers.Tiny_Hop, 
			_ => null, 
		};
	}

	public static string GetSprintSpeedValueString(int SprintSpeedMode)
	{
		return SprintSpeedMode switch
		{
			0 => ScriptLocalization.Modifiers.SpeedNormal, 
			1 => ScriptLocalization.Modifiers.Slowest, 
			2 => ScriptLocalization.Modifiers.Slower, 
			3 => ScriptLocalization.Modifiers.Faster, 
			4 => ScriptLocalization.Modifiers.Fastest, 
			_ => null, 
		};
	}

	public static string GetMultiJumpValueString(int MultiJumpMode)
	{
		return MultiJumpMode switch
		{
			0 => ScriptLocalization.RuleBook.Off, 
			1 => ScriptLocalization.Modifiers.Double, 
			2 => ScriptLocalization.Modifiers.Triple, 
			3 => ScriptLocalization.Modifiers.Unlimited, 
			_ => null, 
		};
	}

	public static string GetOnOffValueString(bool onOff)
	{
		if (!onOff)
		{
			return ScriptLocalization.RuleBook.Off;
		}
		return ScriptLocalization.RuleBook.On;
	}

	public static string GetProjectileExplosionValueString(int ProjectileExplosionMode)
	{
		return ProjectileExplosionMode switch
		{
			0 => ScriptLocalization.RuleBook.Off, 
			1 => ScriptLocalization.Modifiers.Small, 
			2 => ScriptLocalization.Modifiers.Medium, 
			3 => ScriptLocalization.Modifiers.Big, 
			4 => ScriptLocalization.Modifiers.Huge, 
			_ => null, 
		};
	}

	public static string GetCharacterSizeValueString(int CharacterSizeMode)
	{
		return CharacterSizeMode switch
		{
			0 => ScriptLocalization.Modifiers.CharacterSizeNormal, 
			1 => ScriptLocalization.Modifiers.CharacterWayTiny, 
			2 => ScriptLocalization.Modifiers.CharacterTiny, 
			3 => ScriptLocalization.Modifiers.CharacterBig, 
			4 => ScriptLocalization.Modifiers.CharacterHuge, 
			5 => ScriptLocalization.Modifiers.CharacterHuge, 
			_ => null, 
		};
	}

	public static string GetPostDeathBehaviorValueString(int PostDeathBehaviorMode)
	{
		return PostDeathBehaviorMode switch
		{
			0 => ScriptLocalization.RuleBook.Off, 
			1 => ScriptLocalization.Modifiers.Agony, 
			2 => ScriptLocalization.Modifiers.Ghost, 
			3 => ScriptLocalization.Modifiers.Zombie, 
			_ => null, 
		};
	}

	public static string GetCameraFlipValueString(int CameraFlipMode)
	{
		return CameraFlipMode switch
		{
			0 => ScriptLocalization.RuleBook.Off, 
			1 => ScriptLocalization.Modifiers.FlipLeftRight, 
			2 => ScriptLocalization.Modifiers.FlipUpDown, 
			3 => ScriptLocalization.Modifiers.FlipBoth, 
			_ => null, 
		};
	}

	public static string GetDoomsdayMeteorsValueString(int DoomsdayMeteorsMode, int DoomsdayMeteorsDelay)
	{
		if (DoomsdayMeteorsMode == 0)
		{
			return ScriptLocalization.RuleBook.Off;
		}
		return DoomsdayMeteorsDelay + " " + ScriptLocalization.RuleBook.secondsAbbreviation + " " + ScriptLocalization.RuleBook.Delay;
	}

	public static string GetDoomsdayLavaValueString(int DoomsdayLavaMode, int DoomsdayLavaDelay)
	{
		if (DoomsdayLavaMode == 0)
		{
			return ScriptLocalization.RuleBook.Off;
		}
		return DoomsdayLavaDelay + " " + ScriptLocalization.RuleBook.secondsAbbreviation + " " + ScriptLocalization.RuleBook.Delay;
	}

	public static string GetInvisibilityModeValueString(int invisibilityMode)
	{
		return (InvisibilityModes)invisibilityMode switch
		{
			InvisibilityModes.Off => ScriptLocalization.RuleBook.Off, 
			InvisibilityModes.WhenMoving => ScriptLocalization.Modifiers.WhenMoving, 
			InvisibilityModes.WhenStationary => ScriptLocalization.Modifiers.WhenStationary, 
			InvisibilityModes.Always => ScriptLocalization.RuleBook.Always, 
			_ => null, 
		};
	}

	public static void BeginModString()
	{
		stringBuilder = new StringBuilder();
	}

	public static void AddModString(string modName, string modValue = null)
	{
		if (anyModsPrinted)
		{
			stringBuilder.Append("\n");
		}
		else
		{
			anyModsPrinted = true;
		}
		stringBuilder.Append(modName);
		if (modValue != null)
		{
			stringBuilder.Append(ScriptLocalization.Modifiers.ColonSpace);
			stringBuilder.Append(modValue);
		}
	}

	public static void EndModString()
	{
		stringBuilder = null;
		anyModsPrinted = false;
	}

	public string GetCurrentModifierListString(bool forceModsApplied)
	{
		bool flag = modsApplied;
		if (forceModsApplied)
		{
			modsApplied = true;
		}
		BeginModString();
		if (ModsApplied)
		{
			if (GravityMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.Gravity, GetGravityValueString(GravityMode));
			}
			if (JumpSpeedMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.Jump_Strength, GetJumpSpeedValueString(JumpSpeedMode));
			}
			if (MultiJumpMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.MultiJump, GetMultiJumpValueString(MultiJumpMode));
			}
			if (playerPlayerCollisions)
			{
				AddModString(ScriptLocalization.Modifiers.PlayersCollide);
			}
			if (WallJumpsDisabled)
			{
				AddModString(ScriptLocalization.Modifiers.Walljumps, GetOnOffValueString(onOff: false));
			}
			if (SprintSpeedMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.SprintSpeed, GetSprintSpeedValueString(SprintSpeedMode));
			}
			if (GameSpeedMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.GameSpeed, GetSprintSpeedValueString(GameSpeedMode));
			}
			if (frictionless)
			{
				AddModString(ScriptLocalization.Modifiers.Frictionless, GetOnOffValueString(frictionless));
			}
			if (PostDeathBehaviorMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.PostDeathBehaviour, GetPostDeathBehaviorValueString(PostDeathBehaviorMode));
			}
			if (CharacterSizeMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.CharacterSize, GetCharacterSizeValueString(CharacterSizeMode));
			}
			if (jetpackMode)
			{
				AddModString(ScriptLocalization.Modifiers.JetPacksExclamation);
			}
			if (invisibilityMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.Invisible, GetInvisibilityModeValueString(invisibilityMode));
			}
			if (DanceInvincibility)
			{
				AddModString(ScriptLocalization.Modifiers.DanceInvincibility);
			}
			if (MirrorControls)
			{
				AddModString(ScriptLocalization.Modifiers.MirrorControls);
			}
			if (PlatformSpeedMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.MoveBlockSpeed, GetSprintSpeedValueString(PlatformSpeedMode));
			}
			if (RateOfFireMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.ProjectileRateOfFire, GetSprintSpeedValueString(RateOfFireMode));
			}
			if (ProjectileSpeedMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.ProjectileSpeed, GetSprintSpeedValueString(ProjectileSpeedMode));
			}
			if (ProjectileExplosionMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.ProjectileExplosions, GetProjectileExplosionValueString(ProjectileExplosionMode));
			}
			if (CameraFlipMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.MirrorLevel, GetCameraFlipValueString(CameraFlipMode));
			}
			if (DoomsdayMeteorsMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.DoomsdayMeteors, GetDoomsdayMeteorsValueString(DoomsdayMeteorsMode, DoomsdayMeteorsDelay));
			}
			if (DoomsdayLavaMode != 0)
			{
				AddModString(ScriptLocalization.Modifiers.DoomsdayLava, GetDoomsdayLavaValueString(DoomsdayLavaMode, DoomsdayLavaDelay));
			}
		}
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null)
		{
			AddModString(ScriptLocalization.Modifiers.PreviewInTreehouse, GetOnOffValueString(modsPreview));
		}
		modsApplied = flag;
		string text = null;
		text = ((!anyModsPrinted) ? ScriptLocalization.Modifiers.None : stringBuilder.ToString());
		EndModString();
		return text;
	}

	public PerLevelLavaSettings FindLavaSettingsForLevel(GameState.LevelName level)
	{
		for (int i = 0; i < perLevelLavaSettings.Count; i++)
		{
			if (perLevelLavaSettings[i].level == level)
			{
				return perLevelLavaSettings[i];
			}
		}
		return null;
	}
}
