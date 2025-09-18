using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameEvent;
using I2.Loc;
using Smooth;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Character : NetworkBehaviour, InputReceiver, IGameEventListener
{
	public enum AnimState
	{
		IDLE = 0,
		RUN = 1,
		SIT = 3,
		WALK = 2,
		JUMP = 4,
		LAND = 5,
		SLIDE = 6,
		DIE = 7,
		WIN = 8
	}

	public enum SecondaryAnimState
	{
		NONE,
		LOOKUP,
		CROUCH,
		DEADFALL
	}

	public enum Animals
	{
		NONE,
		CHICKEN,
		HORSE,
		SHEEP,
		RACCOON,
		CHAMELEON,
		SQUIRREL,
		ROBOT,
		ELEPHANT,
		MONKEY,
		SNAKE,
		HIPPO,
		TURTLE,
		PANDA,
		FOX,
		PLATYPUS
	}

	public enum AnimParam
	{
		STATE,
		SECONDARYSTATE,
		FLIP,
		DEATHTRIGGER,
		VERTICALDIRFLOAT,
		DEATHTRIGGERFORCE
	}

	public enum NonLocalColliderMode
	{
		Unpicked,
		PickedLocal,
		PickedNonLocal
	}

	private enum ColliderStates
	{
		Standing,
		Crouching,
		Dead
	}

	private static bool animHashSetup;

	private static int stateHash;

	private static int secondaryStateHash;

	private static int flipHash;

	private static int deathTriggerHash;

	private static int verticalDirFloatHash;

	private static int deathTriggerForce;

	private static int bubbleOn;

	private static Collider2D[] playerContactResultCache;

	private static ContactFilter2D playerContactFilter;

	private static ContactFilter2D footPhysicsContactFilter;

	[SyncVar]
	public int networkNumber;

	[SyncVar]
	public int localNumber;

	[SyncVar]
	public bool picked;

	[SyncVar]
	public bool FindPlayerOnSpawn;

	[SyncVar]
	private int flipSpriteX = 1;

	[SyncVar]
	private AnimState currentAnim;

	[SyncVar]
	private SecondaryAnimState secondaryAnim;

	private NetworkIdentity netID;

	public Character CharacterControlClonerInput;

	public GameObject[] AllArtmatchers;

	public int PlayerNumber;

	public Player LocalPlayer;

	private LobbyPlayer associatedLobbyPlayer;

	private GamePlayer associatedGamePlayer;

	public float RunSpeed;

	public float RunAccel;

	public float StopSpeed;

	public float AirInertia;

	public AnimationCurve ResponseTimeModifier;

	public float GroundFrictionForce;

	public float WallFrictionForce;

	public AnimationCurve JumpCapTransitionTime;

	public float VelocityPeak;

	public float forcedJumpPeakVelocity;

	public float JumpGraceTime;

	public float LandJumpGraceTime;

	public float PreJumpGraceTime;

	public AnimationCurve GravityCurve;

	public float loGravityMod;

	public float loGravityModDead;

	public float GravityModOnGround;

	public float FallTerminalVelocity;

	public float SlideTerminalVelocity;

	public float FastSlideTerminalVelocity;

	public float SlowSlideTerminalVelocity;

	public float WallStickTime;

	public float WallJumpInertiaModifier;

	public float WallSlidePressure;

	public Vector2 VMax;

	public float crouchStandupDelay;

	public float minDeathDelay;

	public float freezeDeath;

	public float maxDeathDelay;

	public float deathSettleTimeMax;

	public float fallingDeathSpeedUpAmount;

	public float boringDeathTimerLimit;

	public float minSkipDeathTime;

	public float IdleTime;

	public float DEATHFORCE;

	public float minHorizontalWallStickSpeed;

	public float minHorizontalGapSpeed = 2f;

	protected float forceCrouchTimer;

	private float boostEffectTimer;

	public Vector2 boostVMax;

	public Vector2 deathLimpForce = new Vector2(3f, 13f);

	public bool isGhost;

	[SyncVar]
	public bool isExtraCorpse;

	public bool waitingForExtraCorpse;

	public Character extraCorpse;

	public Character soul;

	public Fly zombieFliePrefab;

	protected List<Fly> spawnedFlies = new List<Fly>();

	public bool isZombie;

	public bool zombieLocallyDead;

	public bool waitingToTurnUndead;

	public bool suggestDance;

	public ViewDirection moveViewMax;

	[NonSerialized]
	public ViewDirection moveViewCurrent;

	public float moveViewDelay;

	[NonSerialized]
	protected ViewDirection moveViewTimer;

	public float moveViewSpeed;

	public float moveViewSpeedReturn;

	public bool isReplay;

	public GhostData replayData;

	private float replayStartTime;

	private float replayPauseTime;

	public bool isReplaying;

	public bool replayPaused;

	private GhostData.GhostDataPoint currentReplayFrame;

	public Character ReplayCharacter;

	public GhostRecorder ReplayRecorder;

	public AnimationCurve analogueModifier;

	[SyncVar]
	public Animals CharacterSprite;

	[SyncVar]
	private Color playerColor;

	public CollisionPiece Head;

	public CollisionPiece HazardHead;

	public CharacterRaycaster Left;

	public CharacterRaycaster Right;

	public CharacterRaycaster Feet;

	public Vector3 feetPositionAlive = new Vector3(0f, -0.3f, 0f);

	public Vector3 feetPositionDead = new Vector3(0f, -0.33f, 0f);

	public bool feedColliderDeadPosition;

	public CheckCollidingPlayer FeetPhysicsCollider;

	public CheckCollidingPlayer LowerBodyCollider;

	public CheckCollidingPlayer UpperBodyCollider;

	public Collider2D UpperBodyTrigger;

	public Collider2D LowerBodyTrigger;

	protected Vector2 DeadColliderInitialPosition;

	public Collider2D DeadCollider;

	public Collider2D LobbyCollider;

	public Spectator SpectatorImage;

	public NameTag nameTag;

	protected float nameBoxTimer;

	public ParticleSystem footIceParticleSystem;

	public ParticleSystem leftIceParticleSystem;

	public ParticleSystem rightIceParticleSystem;

	public CharacterGapLifter gapLifter;

	public Collider2D coinGrabber;

	public BoxCollider2D headCollider;

	public BoxCollider2D hazardHeadcollider;

	public ParticleSystem JetpackParticles;

	public GameObject JetpackHolder;

	public SpriteRenderer JetPackSR;

	public SpriteRenderer InvincibilityBubbleSR;

	public Animator InvincibilityAnimator;

	public Collider2D playerPlayerCollider;

	public Collider2D playerPlayerColliderCrouch;

	public Collider2D playerPlayerColliderDead;

	public GameObject SlowedIcon;

	public GameObject CoinCanvas;

	public Text CoinNumberText;

	public float audioVolumeMod;

	public float footstepTime = 0.25f;

	public float MinSmokeVelocity;

	public Material DefaultJetPackMaterial;

	public CharacterOpacityController opacityController;

	public bool Active;

	public bool InMenu;

	public bool LocallyDead;

	public bool Ready;

	public bool Waiting;

	public bool Sitting;

	public float SuicideTime = 2.2f;

	public float forceAllowSuicideTimer;

	public bool lockSuicide;

	public string LastDeath = "";

	private bool diedInPit;

	[SyncVar]
	public int LastFlagID = -1;

	public int CoinsCollected;

	[SyncVar]
	public bool WantsToRetry;

	public float agonyTimer;

	public float agonyJumpDelayTimer;

	public bool agonyStarted;

	protected bool reverb;

	[SyncVar]
	protected bool onGround;

	protected bool justLanded;

	protected bool onWall;

	protected bool gapFloat;

	[SyncVar]
	protected bool dying;

	[SyncVar]
	protected bool dead;

	protected bool inBlackHole;

	protected bool inCannon;

	[SyncVar]
	protected bool success;

	protected bool succeeding;

	protected bool paused;

	protected bool scoreboard;

	protected bool frozen;

	protected bool disabled;

	protected bool firstFrame;

	protected bool softPause;

	protected bool JetPackArtEnabled;

	protected bool lastJetpackState;

	protected bool canJump;

	protected bool jumping;

	protected bool walking;

	protected bool sprinting;

	protected bool movedFromWall = true;

	protected bool lookingUp;

	protected bool crouchingDown;

	protected bool dancing;

	protected bool loGravity;

	protected bool collidingHead;

	protected bool superInvincible;

	protected float invincibleTimer;

	protected bool tempLockInput;

	protected float maxJumpVel;

	protected float heightJumped;

	protected float lastFallSpeed;

	protected float leftResponseTime;

	protected float rightResponseTime;

	protected int facing;

	protected int wallJumpDirection;

	protected float impulseAdded;

	protected float lastFootstep;

	protected float wallStickTimer;

	protected float jumpGraceTimer;

	protected float PreJumpGraceTimer;

	protected float suicideTimer;

	protected float idleTimer;

	protected float gravityTransitionTime;

	protected float timeSinceJump;

	protected bool forcedJump;

	protected float deathTimer;

	protected float deathSettleTimer;

	protected float boringDeathTimer;

	protected float succeedTimer;

	protected Vector2 deathPauseVel = Vector2.zero;

	protected bool deathFrozen;

	protected Vector2 pauseVel = Vector2.zero;

	public bool Visible;

	public bool bodyHidden;

	public bool CustomItemSoundFX;

	protected SpriteRenderer sprite;

	protected Animator animator;

	protected Animator spectatorAnimator;

	public ArtMatcher OutfitArt;

	protected Rigidbody2D chrRigidBody;

	protected SmoothSync smoothSync;

	public int AirJumps;

	public float AirJumpGraceTime = 0.1f;

	public float AirJumpGraceTimer;

	private bool jetpackTouched;

	private bool pickedUpJetpack;

	private bool jetpackDestroyed;

	protected float crouchStandupTimer;

	protected float hideNameTimer;

	protected bool wantsToRetryUsed;

	private NonLocalColliderMode nonLocalColliderMode;

	private bool playerCollisionMode;

	private UsableProp standingAtProp;

	protected bool holdingRespawns;

	private CharacterRaycaster[] cachedRaycasters;

	private Collider2D[] cachedPlayerColliders;

	private Outfit[] cachedOutfits;

	public float groundFrictionModifier;

	public float wallFrictionModifier;

	protected float gravityModifier;

	protected float jumpForceModifier;

	protected float airInertiaModifier;

	protected float airInertiaModifierL;

	protected float airInertiaModifierR;

	protected float baseHorizontalMotion;

	protected float baseVerticalMotion;

	protected Vector2 characterInheritedMotion = new Vector2(0f, 0f);

	protected Vector2 lastInheritedMotion = new Vector2(0f, 0f);

	protected Vector3 previousPosition;

	protected float blackHoleX;

	protected float blackholeY;

	protected float windX;

	protected float windY;

	protected float previousVX;

	protected float previousVY;

	protected float previousPreviousVX;

	protected float previousPreviousVY;

	protected Dictionary<GameObject, PhysicsModifier> physicsModifiers = new Dictionary<GameObject, PhysicsModifier>();

	public bool jump;

	public bool sprint;

	public bool suicide;

	public bool dance;

	public bool leftTrigger;

	public bool jumpDown;

	public bool sprintDown;

	public bool suicideDown;

	public bool danceDown;

	public bool jumpUp;

	public bool sprintUp;

	public bool suicideUp;

	public bool danceUp;

	public bool rotateLeft;

	public bool rotateRight;

	public bool rotateLeftDown;

	public bool rotateRightDown;

	public float up;

	public float down;

	public float left;

	public float right;

	private float leftInput;

	private float rightInput;

	public float up2;

	public float down2;

	public float left2;

	public float right2;

	public bool forceNextJump;

	private bool backDownOnEnable;

	public float lastJumpDownTimer = 999f;

	private float cameraChangeTimeout;

	public HoldBToGiveUp holdBEToGiveUpPrefab;

	protected HoldBToGiveUp holdBToGiveUpInstance;

	public List<Cursor> HoveredCursors = new List<Cursor>();

	public Canvas AFKWarningCanvas;

	public Text AFKWarningText;

	public bool AFKWarningEnabled;

	private bool ignoreAFK;

	private bool AFKWarningVisible = true;

	protected float afkTimer;

	private float maxAFKTime;

	public bool pushedByDoorThisFrame;

	public static List<Character> AllCharacters;

	private bool wallJumpThisFrame;

	private bool forcedGravity;

	private Modifiers.GravityType forcedGravityMode;

	private float forcedGravityMultiplier = 1f;

	private Dictionary<(string, string, bool, bool, bool), string> cachedAudioStrings = new Dictionary<(string, string, bool, bool, bool), string>();

	private List<Collider2D> ignoredCollisions = new List<Collider2D>();

	private HashSet<Character> visited = new HashSet<Character>();

	private static int kCmdCmdEnable;

	private static int kRpcRpcEnable;

	private static int kCmdCmdDisable;

	private static int kRpcRpcDisable;

	private static int kCmdCmdClearExtraCorpse;

	private static int kCmdCmdShowSprite;

	private static int kRpcRpcShowprite;

	private static int kCmdCmdFreeze;

	private static int kRpcRpcFreeze;

	private static int kCmdCmdCommunicateOutfitsArray;

	private static int kRpcRpcCommunicateOutfitsArray;

	private static int kCmdCmdSetupDeath;

	private static int kRpcRpcSetupDeath;

	private static int kCmdCmdSetLocalPlayerID;

	private static int kRpcRpcFindLocalPlayer;

	private static int kCmdCmdGetLocalController;

	private static int kRpcRpcGetLocalController;

	private static int kCmdCmdSetPlayerColor;

	private static int kCmdCmdSetSuccess;

	private static int kRpcRpcSendPlayerSuccessEvent;

	private static int kCmdCmdSetDying;

	private static int kCmdCmdSetDead;

	private static int kCmdCmdSetLobbyCollider;

	private static int kRpcRpcSetLobbyCollider;

	private static int kCmdCmdSetPicked;

	private static int kRpcRpcSetRaycastsEnabled;

	private static int kRpcRpcSetReady;

	private static int kCmdCmdSwitchFreeMode;

	private static int kRpcRpcSwitchFreeMode;

	private static int kCmdCmdAnimatorTrigger;

	private static int kRpcRpcAnimatorTrigger;

	private static int kCmdCmdSetAnimatorInt;

	private static int kRpcRpcSetAnimatorInt;

	private static int kCmdCmdSetBubble;

	private static int kRpcRpcSetBubble;

	private static int kCmdCmdAudioEvent;

	private static int kRpcRpcAudioEvent;

	private static int kCmdCmdAudioEventExact;

	private static int kRpcRpcAudioEventExact;

	private static int kCmdCmdSetAnimatorFloat;

	private static int kRpcRpcSetAnimatorFloat;

	private static int kCmdCmdSetScaleX;

	private static int kCmdCmdSpawnDustCloud;

	private static int kRpcRpcSpawnDustCloud;

	private static int kCmdCmdSpawnJumpCloud;

	private static int kRpcRpcSpawnJumpCloud;

	private static int kCmdCmdSpawnWallJumpCloud;

	private static int kRpcRpcSpawnWallJumpCloud;

	private static int kCmdCmdSpawnHoneyStickers;

	private static int kRpcRpcSpawnHoneyStickers;

	private static int kCmdCmdPositionCharacter;

	private static int kRpcRpcPositionCharacter;

	private static int kCmdCmdRequestGrabCoin;

	private static int kRpcRpcGrantCoin;

	private static int kCmdCmdRequestBees;

	private static int kRpcRpcGrantBees;

	private static int kCmdCmdFinishedWithCoin;

	private static int kRpcRpcFinishedWithCoin;

	private static int kCmdCmdDroppedCoin;

	private static int kRpcRpcDroppedCoin;

	private static int kCmdCmdRequestGrabJetpack;

	private static int kRpcRpcSetJetPackTouched;

	private static int kRpcRpcGrantJetpack;

	private static int kCmdCmdSetLastFlagID;

	private static int kCmdCmdEnableAFKWarning;

	private static int kRpcRpcEnableAFKWarning;

	private static int kCmdCmdDisableAFKWarning;

	private static int kRpcRpcDisableAFKWarning;

	private static int kCmdCmdSetWantsToRetry;

	private static int kCmdCmdComeBackAsGhost;

	private static int kRpcRpcComeBackAsGhost;

	private static int kCmdCmdSpawnExtraCorpse;

	private static int kRpcRpcOnExtraCorpseSpawned;

	private static int kCmdCmdComeBackAsZombie;

	private static int kRpcRpcComeBackAsZombie;

	private static int kCmdCmdLoseLife;

	private static int kRpcRpcLoseLife;

	private static int kCmdCmdRespawn;

	private static int kRpcRpcRespawn;

	private static int kCmdCmdAgonize;

	private static int kRpcRpcAgonize;

	private static int kCmdCmdIShouldBeKicked;

	private static int kCmdCmdSetRemoteJetpackState;

	private static int kRpcRpcSetRemoteJetpackState;

	public bool Picked => picked;

	public int FlipSpriteX => flipSpriteX;

	public AnimState CurrentAnim => currentAnim;

	public SecondaryAnimState SecondaryAnim => secondaryAnim;

	public LobbyPlayer AssociatedLobbyPlayer
	{
		get
		{
			return associatedLobbyPlayer;
		}
		set
		{
			associatedLobbyPlayer = value;
		}
	}

	public GamePlayer AssociatedGamePlayer
	{
		get
		{
			return associatedGamePlayer;
		}
		set
		{
			associatedGamePlayer = value;
		}
	}

	public bool forceCrouch => forceCrouchTimer > 0f;

	public float AnimatorSpeed
	{
		get
		{
			if (isZombie)
			{
				return Modifiers.GetInstance().zombieAnimatorSpeed;
			}
			return 1f;
		}
	}

	public bool HasReplayData => replayData != null;

	public Color PlayerColor
	{
		get
		{
			return playerColor;
		}
		set
		{
			if (base.hasAuthority && netID.isClient)
			{
				CallCmdSetPlayerColor(value);
			}
			else
			{
				NetworkplayerColor = value;
			}
		}
	}

	public bool Dead => dead;

	public bool Dying => dying;

	public bool InBlackHole
	{
		get
		{
			return inBlackHole;
		}
		set
		{
			inBlackHole = value;
			if (value)
			{
				FeetPhysicsCollider.GetComponent<Collider2D>().enabled = false;
			}
		}
	}

	public bool InCannon
	{
		get
		{
			return inCannon;
		}
		set
		{
			inCannon = value;
		}
	}

	public bool Success => success;

	public bool Enabled => !disabled;

	public bool Paused => paused;

	public bool SoftPaused => softPause;

	public bool Frozen => frozen;

	public bool DeathFrozen => deathFrozen;

	public bool Invincible
	{
		get
		{
			if ((isGhost && !isZombie) || invincibleTimer > 0f || (dancing && Modifiers.GetInstance().DanceInvincibility))
			{
				return true;
			}
			return false;
		}
	}

	public bool Invisible
	{
		get
		{
			Modifiers instance = Modifiers.GetInstance();
			Modifiers.InvisibilityModes invisibilityMode = (Modifiers.InvisibilityModes)instance.invisibilityMode;
			if (Picked && instance.ModsApplied)
			{
				switch (invisibilityMode)
				{
				case Modifiers.InvisibilityModes.Always:
					return true;
				case Modifiers.InvisibilityModes.WhenMoving:
					if (currentAnim != AnimState.IDLE && currentAnim != AnimState.DIE)
					{
						return currentAnim != AnimState.WIN;
					}
					return false;
				case Modifiers.InvisibilityModes.WhenStationary:
					return currentAnim == AnimState.IDLE;
				}
			}
			return false;
		}
	}

	public bool CrouchingDown => crouchingDown;

	public bool Reverb
	{
		get
		{
			return reverb;
		}
		set
		{
			if (reverb && !value)
			{
				AkSoundEngine.PostEvent("Out_Enclosed_Area", base.gameObject);
			}
			else if (!reverb && value)
			{
				AkSoundEngine.PostEvent("In_Enclosed_Area", base.gameObject);
			}
			reverb = value;
		}
	}

	public bool OnGround
	{
		get
		{
			return onGround;
		}
		set
		{
			NetworkonGround = value;
		}
	}

	public bool IsDeadAndSettled
	{
		get
		{
			if (!Dead || !(deathTimer > minDeathDelay) || !(deathSettleTimer > deathSettleTimeMax))
			{
				if (!(deathTimer > maxDeathDelay))
				{
					return boringDeathTimer > boringDeathTimerLimit;
				}
				return true;
			}
			return true;
		}
	}

	public bool CanJump
	{
		get
		{
			if (!canJump)
			{
				return HasJetpack;
			}
			return true;
		}
		set
		{
			canJump = value;
		}
	}

	public bool HasJetpack
	{
		get
		{
			bool jetPackArtEnabled = JetPackArtEnabled;
			JetPackArtEnabled = Modifiers.GetInstance().JetpackMode || pickedUpJetpack;
			if (jetPackArtEnabled != JetPackArtEnabled)
			{
				SetupJetPackArt(JetPackArtEnabled);
			}
			return JetPackArtEnabled;
		}
	}

	public bool Jetpacking { get; protected set; }

	public bool LoGravity => loGravity;

	public int Facing
	{
		set
		{
			facing = value;
		}
	}

	public SpriteRenderer SpriteRenderer
	{
		get
		{
			return sprite;
		}
		protected set
		{
		}
	}

	public Vector2 Velocity => new Vector2(previousVX, previousVY);

	public Vector2 previousVelocity => new Vector2(previousPreviousVX, previousPreviousVY);

	public string CharacterSFXName
	{
		get
		{
			if ((AssociatedGamePlayer != null && AssociatedGamePlayer.IsWearingSkin) || (associatedLobbyPlayer != null && associatedLobbyPlayer.IsWearingSkin))
			{
				switch (CharacterSprite)
				{
				case Animals.NONE:
					return null;
				case Animals.ROBOT:
					return "RegularBunny";
				case Animals.MONKEY:
					return "RobotMonkey";
				}
			}
			if (CustomItemSoundFX && CharacterSprite == Animals.SNAKE)
			{
				return "HoverboardSnake";
			}
			return CharacterSprite switch
			{
				Animals.NONE => null, 
				Animals.CHICKEN => "Chicken", 
				Animals.HORSE => "Horse", 
				Animals.SHEEP => "Sheep", 
				Animals.RACCOON => "Raccoon", 
				Animals.CHAMELEON => "Chameleon", 
				Animals.SQUIRREL => "Squirrel", 
				Animals.ROBOT => "Robot", 
				Animals.ELEPHANT => "Elephant", 
				Animals.MONKEY => "Monkey", 
				Animals.SNAKE => "Snake", 
				Animals.HIPPO => "Hippo", 
				Animals.TURTLE => "Turtle", 
				Animals.PANDA => "Panda", 
				Animals.FOX => "Fox", 
				Animals.PLATYPUS => "Platypus", 
				_ => null, 
			};
		}
	}

	public string CharacterSFXNameNoCustom
	{
		get
		{
			if ((AssociatedGamePlayer != null && AssociatedGamePlayer.IsWearingSkin) || (associatedLobbyPlayer != null && associatedLobbyPlayer.IsWearingSkin))
			{
				switch (CharacterSprite)
				{
				case Animals.NONE:
					return null;
				case Animals.ROBOT:
					return "RegularBunny";
				case Animals.MONKEY:
					return "RobotMonkey";
				}
			}
			return CharacterSprite switch
			{
				Animals.NONE => null, 
				Animals.CHICKEN => "Chicken", 
				Animals.HORSE => "Horse", 
				Animals.SHEEP => "Sheep", 
				Animals.RACCOON => "Raccoon", 
				Animals.CHAMELEON => "Chameleon", 
				Animals.SQUIRREL => "Squirrel", 
				Animals.ROBOT => "Robot", 
				Animals.ELEPHANT => "Elephant", 
				Animals.MONKEY => "Monkey", 
				Animals.SNAKE => "Snake", 
				Animals.HIPPO => "Hippo", 
				Animals.TURTLE => "Turtle", 
				Animals.PANDA => "Panda", 
				Animals.FOX => "Fox", 
				Animals.PLATYPUS => "Platypus", 
				_ => null, 
			};
		}
	}

	public string LocalizedName
	{
		get
		{
			if (AssociatedGamePlayer != null)
			{
				return GetLocalizedAnimal(CharacterSprite, AssociatedGamePlayer.IsWearingSkin);
			}
			if (AssociatedLobbyPlayer != null)
			{
				return GetLocalizedAnimal(CharacterSprite, AssociatedLobbyPlayer.IsWearingSkin);
			}
			return GetLocalizedAnimal(CharacterSprite, altSkin: false);
		}
	}

	public string LocalizedNameWinMessage
	{
		get
		{
			if (AssociatedGamePlayer != null)
			{
				return GetLocalizedAnimalWinMessage(CharacterSprite, AssociatedGamePlayer.IsWearingSkin);
			}
			if (AssociatedLobbyPlayer != null)
			{
				return GetLocalizedAnimalWinMessage(CharacterSprite, AssociatedLobbyPlayer.IsWearingSkin);
			}
			return GetLocalizedAnimalWinMessage(CharacterSprite, altSkin: false);
		}
	}

	public float TimeSpentAFK => afkTimer;

	public bool IsControllerAFK
	{
		get
		{
			if (AssociatedGamePlayer != null && AssociatedGamePlayer.LocalPlayer != null && AssociatedGamePlayer.LocalPlayer.UseController != null)
			{
				return AssociatedGamePlayer.LocalPlayer.UseController.TimeSinceLastInput > 1f;
			}
			return false;
		}
	}

	public bool HasExceededAFKLimit
	{
		get
		{
			if (maxAFKTime != (float)GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime)
			{
				return false;
			}
			if (AFKWarningEnabled && maxAFKTime > 0f)
			{
				return afkTimer > maxAFKTime;
			}
			return false;
		}
	}

	public bool AffectedByImpulse => impulseAdded > 0f;

	private Character StandingOnCharacter
	{
		get
		{
			int num = FeetPhysicsCollider.GetComponent<Collider2D>().OverlapCollider(playerContactFilter, playerContactResultCache);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					Character componentInParent = playerContactResultCache[i].gameObject.GetComponentInParent<Character>();
					if (componentInParent != null && componentInParent != this && componentInParent.nonLocalColliderMode != NonLocalColliderMode.Unpicked)
					{
						return componentInParent;
					}
				}
			}
			return null;
		}
	}

	private Character StandingOnCharacterNested
	{
		get
		{
			visited.Clear();
			visited.Add(this);
			Character character = this;
			Character standingOnCharacter = StandingOnCharacter;
			while (standingOnCharacter != null && !visited.Contains(standingOnCharacter))
			{
				character = standingOnCharacter;
				visited.Add(standingOnCharacter);
				standingOnCharacter = character.StandingOnCharacter;
			}
			return character;
		}
	}

	private bool IsBeingStomped
	{
		get
		{
			int num = headCollider.OverlapCollider(playerContactFilter, playerContactResultCache);
			if (num > 0)
			{
				bool result = false;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					Character componentInParent = playerContactResultCache[i].gameObject.GetComponentInParent<Character>();
					if (componentInParent != null && componentInParent != this && componentInParent.nonLocalColliderMode != NonLocalColliderMode.Unpicked)
					{
						result = true;
						num2 = i;
						break;
					}
				}
				for (int j = num2; j < num; j++)
				{
					playerContactResultCache[j] = null;
				}
				return result;
			}
			return false;
		}
	}

	public bool IsDeadAndDiedInPit
	{
		get
		{
			if (Dead)
			{
				return diedInPit;
			}
			return false;
		}
	}

	private bool BoostEffect => boostEffectTimer > 0f;

	private float BoostVMaxX
	{
		get
		{
			GameSettings instance = GameSettings.GetInstance();
			float time = boostEffectTimer / instance.boostEffectDuration;
			return Mathf.Lerp(VMax.x, boostVMax.x, instance.boostEffectCurve.Evaluate(time));
		}
	}

	private float BoostVMaxY
	{
		get
		{
			GameSettings instance = GameSettings.GetInstance();
			float time = boostEffectTimer / instance.boostEffectDuration;
			return Mathf.Lerp(VMax.y, boostVMax.y, instance.boostEffectCurve.Evaluate(time));
		}
	}

	private bool ForceAllowSuicide
	{
		get
		{
			if (!(forceAllowSuicideTimer > 0f))
			{
				return forceAllowSuicideTimer == -1f;
			}
			return true;
		}
	}

	private bool RunTimerHit
	{
		get
		{
			if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null)
			{
				VersusControl versusControl = LobbyManager.instance.CurrentGameController as VersusControl;
				if (versusControl != null)
				{
					return versusControl.runTimer.Tripped;
				}
			}
			return false;
		}
	}

	public int NetworknetworkNumber
	{
		get
		{
			return networkNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref networkNumber, 1u);
		}
	}

	public int NetworklocalNumber
	{
		get
		{
			return localNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref localNumber, 2u);
		}
	}

	public bool Networkpicked
	{
		get
		{
			return picked;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref picked, 4u);
		}
	}

	public bool NetworkFindPlayerOnSpawn
	{
		get
		{
			return FindPlayerOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FindPlayerOnSpawn, 8u);
		}
	}

	public int NetworkflipSpriteX
	{
		get
		{
			return flipSpriteX;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref flipSpriteX, 16u);
		}
	}

	public AnimState NetworkcurrentAnim
	{
		get
		{
			return currentAnim;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref currentAnim, 32u);
		}
	}

	public SecondaryAnimState NetworksecondaryAnim
	{
		get
		{
			return secondaryAnim;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref secondaryAnim, 64u);
		}
	}

	public bool NetworkisExtraCorpse
	{
		get
		{
			return isExtraCorpse;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isExtraCorpse, 128u);
		}
	}

	public Animals NetworkCharacterSprite
	{
		get
		{
			return CharacterSprite;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref CharacterSprite, 256u);
		}
	}

	public Color NetworkplayerColor
	{
		get
		{
			return playerColor;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref playerColor, 512u);
		}
	}

	public int NetworkLastFlagID
	{
		get
		{
			return LastFlagID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref LastFlagID, 1024u);
		}
	}

	public bool NetworkWantsToRetry
	{
		get
		{
			return WantsToRetry;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref WantsToRetry, 2048u);
		}
	}

	public bool NetworkonGround
	{
		get
		{
			return onGround;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref onGround, 4096u);
		}
	}

	public bool Networkdying
	{
		get
		{
			return dying;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref dying, 8192u);
		}
	}

	public bool Networkdead
	{
		get
		{
			return dead;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref dead, 16384u);
		}
	}

	public bool Networksuccess
	{
		get
		{
			return success;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref success, 32768u);
		}
	}

	private static int AnimToString(AnimParam input)
	{
		return input switch
		{
			AnimParam.STATE => stateHash, 
			AnimParam.SECONDARYSTATE => secondaryStateHash, 
			AnimParam.FLIP => flipHash, 
			AnimParam.DEATHTRIGGER => deathTriggerHash, 
			AnimParam.VERTICALDIRFLOAT => verticalDirFloatHash, 
			AnimParam.DEATHTRIGGERFORCE => deathTriggerForce, 
			_ => 0, 
		};
	}

	private static ContactFilter2D GetHeadCheckContactFilter()
	{
		ContactFilter2D result = default(ContactFilter2D);
		int num = 4096;
		num |= 0x4000000;
		result.SetLayerMask(num);
		return result;
	}

	private static ContactFilter2D GetFootPhysicsContactFilter()
	{
		ContactFilter2D result = default(ContactFilter2D);
		int num = 4096;
		num |= 0x4000000;
		num = ~num;
		result.SetLayerMask(num);
		return result;
	}

	public static string GetLocalizedAnimal(Animals animal, bool altSkin)
	{
		switch (animal)
		{
		case Animals.CHICKEN:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Chicken;
			}
			return ScriptLocalization.CharacterNames.Macaw;
		case Animals.HORSE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Horse;
			}
			return ScriptLocalization.CharacterNames.Zebra;
		case Animals.RACCOON:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Raccoon;
			}
			return ScriptLocalization.CharacterNames.RedPanda;
		case Animals.SHEEP:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Sheep;
			}
			return ScriptLocalization.CharacterNames.Ram;
		case Animals.CHAMELEON:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Chameleon;
			}
			return ScriptLocalization.CharacterNames.Axolotl;
		case Animals.SQUIRREL:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Squirrel;
			}
			return ScriptLocalization.CharacterNames.Skunk;
		case Animals.ROBOT:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Bunny;
			}
			return ScriptLocalization.CharacterNames.RealBunny;
		case Animals.ELEPHANT:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Elephant;
			}
			return ScriptLocalization.CharacterNames.Mammoth;
		case Animals.MONKEY:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Monkey;
			}
			return ScriptLocalization.CharacterNames.RobotMonkey;
		case Animals.SNAKE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Snake;
			}
			return ScriptLocalization.CharacterNames.Cobra;
		case Animals.HIPPO:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Hippo;
			}
			return ScriptLocalization.CharacterNames.Triceratops;
		case Animals.TURTLE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Turtle;
			}
			return ScriptLocalization.CharacterNames.Armadillo;
		case Animals.PANDA:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Panda;
			}
			return ScriptLocalization.CharacterNames.Bear;
		case Animals.FOX:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Fox;
			}
			return ScriptLocalization.CharacterNames.DireWolf;
		case Animals.PLATYPUS:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames.Platypus;
			}
			return ScriptLocalization.CharacterNames.Toucan;
		default:
			return "";
		}
	}

	public static string GetLocalizedAnimalWinMessage(Animals animal, bool altSkin)
	{
		switch (animal)
		{
		case Animals.CHICKEN:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Chicken;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Macaw;
		case Animals.HORSE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Horse;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Zebra;
		case Animals.RACCOON:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Raccoon;
			}
			return ScriptLocalization.CharacterNames_WinMessage.RedPanda;
		case Animals.SHEEP:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Sheep;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Ram;
		case Animals.CHAMELEON:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Chameleon;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Axolotl;
		case Animals.SQUIRREL:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Squirrel;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Skunk;
		case Animals.ROBOT:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Bunny;
			}
			return ScriptLocalization.CharacterNames_WinMessage.RealBunny;
		case Animals.ELEPHANT:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Elephant;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Mammoth;
		case Animals.MONKEY:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Monkey;
			}
			return ScriptLocalization.CharacterNames_WinMessage.RobotMonkey;
		case Animals.SNAKE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Snake;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Cobra;
		case Animals.HIPPO:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Hippo;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Triceratops;
		case Animals.TURTLE:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Turtle;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Armadillo;
		case Animals.PANDA:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Panda;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Bear;
		case Animals.FOX:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Fox;
			}
			return ScriptLocalization.CharacterNames_WinMessage.DireWolf;
		case Animals.PLATYPUS:
			if (!altSkin)
			{
				return ScriptLocalization.CharacterNames_WinMessage.Platypus;
			}
			return ScriptLocalization.CharacterNames_WinMessage.Toucan;
		default:
			return "";
		}
	}

	public static string GetLocalizedTermInSheet(Animals animal)
	{
		string text = "CharacterNames/";
		switch (animal)
		{
		case Animals.NONE:
			text = text ?? "";
			break;
		case Animals.CHICKEN:
			text += "Chicken";
			break;
		case Animals.HORSE:
			text += "Horse";
			break;
		case Animals.SHEEP:
			text += "Sheep";
			break;
		case Animals.RACCOON:
			text += "Raccoon";
			break;
		case Animals.CHAMELEON:
			text += "Chameleon";
			break;
		case Animals.SQUIRREL:
			text += "Squirrel";
			break;
		case Animals.ROBOT:
			text += "Bunny";
			break;
		case Animals.ELEPHANT:
			text += "Elephant";
			break;
		case Animals.MONKEY:
			text += "Monkey";
			break;
		case Animals.SNAKE:
			text += "Snake";
			break;
		case Animals.HIPPO:
			text += "Hippo";
			break;
		case Animals.TURTLE:
			text += "Turtle";
			break;
		case Animals.PANDA:
			text += "Panda";
			break;
		case Animals.FOX:
			text += "Fox";
			break;
		case Animals.PLATYPUS:
			text += "Platypus";
			break;
		}
		return text;
	}

	private void Awake()
	{
		AllCharacters.Add(this);
		if (!animHashSetup)
		{
			animHashSetup = true;
			stateHash = Animator.StringToHash("State");
			secondaryStateHash = Animator.StringToHash("SecondaryState");
			flipHash = Animator.StringToHash("Flip");
			deathTriggerHash = Animator.StringToHash("DeathTrigger");
			verticalDirFloatHash = Animator.StringToHash("VerticalDirFloat");
			deathTriggerForce = Animator.StringToHash("DeathTriggerForce");
			bubbleOn = Animator.StringToHash("BubbleOn");
		}
		sprite = GetComponentInChildren<SpriteRenderer>();
		animator = GetComponentInChildren<Animator>();
		smoothSync = GetComponent<SmoothSync>();
		spectatorAnimator = SpectatorImage.GetComponent<Animator>();
		chrRigidBody = GetComponent<Rigidbody2D>();
		cachedRaycasters = GetComponentsInChildren<CharacterRaycaster>(includeInactive: true);
		cachedPlayerColliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
		cachedOutfits = GetComponentsInChildren<Outfit>(includeInactive: true);
		netID = GetComponent<NetworkIdentity>();
		DeadColliderInitialPosition = DeadCollider.transform.localPosition;
		DeadColliderSwitch(enabled: false);
		holdBToGiveUpInstance = UnityEngine.Object.Instantiate(holdBEToGiveUpPrefab, base.transform.position, Quaternion.identity);
		holdBToGiveUpInstance.transform.parent = base.transform;
		holdBToGiveUpInstance.InstantHide();
		UpdateHoldBIndicator();
		frozen = true;
		Visible = true;
		DisableAFKWarning(initializing: true);
		ignoreAFK = false;
		playerPlayerCollider.gameObject.SetActive(value: false);
		playerPlayerColliderCrouch.gameObject.SetActive(value: false);
		SetNonLocalColliderMode(NonLocalColliderMode.Unpicked, playerCollisionMode: false);
	}

	private void hideHoldBIndicator()
	{
		GameSettings instance = GameSettings.GetInstance();
		if (LobbyManager.instance.CurrentGameController != null && (instance.GameMode == GameState.GameMode.PARTY || instance.GameMode == GameState.GameMode.CREATIVE))
		{
			LobbyManager.instance.CurrentGameController.livesDisplayController?.SetRespawnButtonFill(AssociatedGamePlayer.networkNumber, 0f);
		}
		holdBToGiveUpInstance.Hide();
	}

	private void setHoldBFillAmount(float fillAmount)
	{
		GameSettings instance = GameSettings.GetInstance();
		if (instance.GameMode == GameState.GameMode.PARTY || instance.GameMode == GameState.GameMode.CREATIVE)
		{
			bool flag = false;
			RespawnMode respawnMode = instance.respawnMode;
			if ((uint)respawnMode > 1u && (uint)(respawnMode - 2) <= 1u && associatedGamePlayer != null)
			{
				flag = (dead || dying) && associatedGamePlayer.lives > 0 && !holdingRespawns;
			}
			if (flag)
			{
				LobbyManager.instance.CurrentGameController.livesDisplayController.SetRespawnButtonFill(AssociatedGamePlayer.networkNumber, fillAmount);
			}
		}
		holdBToGiveUpInstance.SetFillAmount(fillAmount);
	}

	private void UpdateHoldBIndicator()
	{
		GameSettings instance = GameSettings.GetInstance();
		switch (instance.GameMode)
		{
		case GameState.GameMode.CREATIVE:
		case GameState.GameMode.PARTY:
		{
			bool flag = false;
			RespawnMode respawnMode = instance.respawnMode;
			if ((uint)respawnMode > 1u && (uint)(respawnMode - 2) <= 1u)
			{
				flag = (dying || (isGhost && !isZombie) || diedInPit) && associatedGamePlayer.lives > 0 && !holdingRespawns;
			}
			if (flag)
			{
				holdBToGiveUpInstance.text.text = LocalizationManager.GetTranslation("InGameText/Respawn");
				SuicideTime = 0.5f;
				LobbyManager.instance.CurrentGameController.livesDisplayController.SetCanRespawn(AssociatedGamePlayer.networkNumber, canRespawn: true);
				break;
			}
			Modifiers instance2 = Modifiers.GetInstance();
			if (instance2.PostDeathBehavior == Modifiers.PostDeathBehaviors.Zombie && dying && !isZombie)
			{
				holdBToGiveUpInstance.text.text = LocalizationManager.GetTranslation("InGameText/Become Zombie");
				SuicideTime = instance2.zombificationTime;
			}
			else
			{
				holdBToGiveUpInstance.text.text = LocalizationManager.GetTranslation("InGameText/Give Up");
				SuicideTime = 2.2f;
			}
			if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null && LobbyManager.instance.CurrentGameController.livesDisplayController != null && AssociatedGamePlayer != null)
			{
				LobbyManager.instance.CurrentGameController.livesDisplayController.SetCanRespawn(AssociatedGamePlayer.networkNumber, canRespawn: false);
			}
			break;
		}
		case GameState.GameMode.FREEPLAY:
			holdBToGiveUpInstance.text.text = LocalizationManager.GetTranslation("InGameText/Build");
			SuicideTime = 0.5f;
			break;
		case GameState.GameMode.CHALLENGE:
			holdBToGiveUpInstance.text.text = LocalizationManager.GetTranslation("Scoreboard/Challenge/Retry");
			SuicideTime = 0.5f;
			break;
		}
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
		GameEventManager.ChangeListener<SoftPauseEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<HoldRespawnEvent>(this, adding);
	}

	private void Start()
	{
		holdingRespawns = false;
		ChangeListener(adding: true);
		SetSprites(CharacterSprite);
		SetupJetPackArt(enabled: false);
		if (AssociatedGamePlayer != null)
		{
			SetOutfitsFromArray(AssociatedGamePlayer.characterOutfitsList);
		}
		AkSoundEngine.SetSwitch("Character", CharacterSprite.ToString(), base.gameObject);
		Physics.IgnoreLayerCollision(12, 12, ignore: true);
		audioEvent("_Move_Stop", base.gameObject);
		audioEvent("_Sprint_Stop", base.gameObject);
		audioEvent("_Dance_Stop", base.gameObject);
		audioEvent("_WallSlideStop", base.gameObject);
		if (base.hasAuthority && FindPlayerOnSpawn)
		{
			Debug.Log(base.name + " Finding player " + localNumber);
			CallCmdSetLocalPlayerID(localNumber);
		}
		setRaycastsEnabled(picked);
		if ((bool)CharacterControlClonerInput)
		{
			RunSpeed = CharacterControlClonerInput.RunSpeed;
			RunAccel = CharacterControlClonerInput.RunAccel;
			StopSpeed = CharacterControlClonerInput.StopSpeed;
			AirInertia = CharacterControlClonerInput.AirInertia;
			ResponseTimeModifier = new AnimationCurve(CharacterControlClonerInput.ResponseTimeModifier.keys);
			GroundFrictionForce = CharacterControlClonerInput.GroundFrictionForce;
			WallFrictionForce = CharacterControlClonerInput.WallFrictionForce;
			JumpCapTransitionTime = new AnimationCurve(CharacterControlClonerInput.JumpCapTransitionTime.keys);
			VelocityPeak = CharacterControlClonerInput.VelocityPeak;
			forcedJumpPeakVelocity = CharacterControlClonerInput.forcedJumpPeakVelocity;
			JumpGraceTime = CharacterControlClonerInput.JumpGraceTime;
			LandJumpGraceTime = CharacterControlClonerInput.LandJumpGraceTime;
			PreJumpGraceTime = CharacterControlClonerInput.PreJumpGraceTime;
			GravityModOnGround = CharacterControlClonerInput.GravityModOnGround;
			GravityCurve = new AnimationCurve(CharacterControlClonerInput.GravityCurve.keys);
			loGravityMod = CharacterControlClonerInput.loGravityMod;
			loGravityModDead = CharacterControlClonerInput.loGravityModDead;
			FallTerminalVelocity = CharacterControlClonerInput.FallTerminalVelocity;
			SlideTerminalVelocity = CharacterControlClonerInput.SlideTerminalVelocity;
			WallStickTime = CharacterControlClonerInput.WallStickTime;
			WallJumpInertiaModifier = CharacterControlClonerInput.WallJumpInertiaModifier;
			WallSlidePressure = CharacterControlClonerInput.WallSlidePressure;
			VMax = CharacterControlClonerInput.VMax;
			boostVMax = CharacterControlClonerInput.boostVMax;
			crouchStandupDelay = CharacterControlClonerInput.crouchStandupDelay;
			maxDeathDelay = CharacterControlClonerInput.maxDeathDelay;
			minDeathDelay = CharacterControlClonerInput.minDeathDelay;
			freezeDeath = CharacterControlClonerInput.freezeDeath;
			deathSettleTimeMax = CharacterControlClonerInput.deathSettleTimeMax;
			fallingDeathSpeedUpAmount = CharacterControlClonerInput.fallingDeathSpeedUpAmount;
			boringDeathTimerLimit = CharacterControlClonerInput.boringDeathTimerLimit;
			FastSlideTerminalVelocity = CharacterControlClonerInput.FastSlideTerminalVelocity;
			SlowSlideTerminalVelocity = CharacterControlClonerInput.SlowSlideTerminalVelocity;
			moveViewMax = CharacterControlClonerInput.moveViewMax;
			moveViewDelay = CharacterControlClonerInput.moveViewDelay;
			moveViewSpeed = CharacterControlClonerInput.moveViewSpeed;
			moveViewSpeedReturn = CharacterControlClonerInput.moveViewSpeedReturn;
			minHorizontalWallStickSpeed = CharacterControlClonerInput.minHorizontalWallStickSpeed;
			minSkipDeathTime = CharacterControlClonerInput.minSkipDeathTime;
		}
	}

	private void SetLayerRecursively(int layerNumber)
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layerNumber;
		}
	}

	private IEnumerator UpdateNetworkTransform()
	{
		while (true)
		{
			if (base.hasAuthority && Enabled)
			{
				NetworkTransform component = GetComponent<NetworkTransform>();
				if (component != null)
				{
					component.SetDirtyBit(uint.MaxValue);
				}
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private void Update()
	{
		lastJumpDownTimer += Time.deltaTime;
		if (AssociatedLobbyPlayer != null)
		{
			if (!string.IsNullOrEmpty(AssociatedLobbyPlayer.ValidatedDisplayName) && nameTag.Currentname != AssociatedLobbyPlayer.ValidatedDisplayName)
			{
				nameTag.setNameBoxText(AssociatedLobbyPlayer.ValidatedDisplayName, this);
			}
		}
		else if (AssociatedGamePlayer != null)
		{
			if (!string.IsNullOrEmpty(AssociatedGamePlayer.ValidatedDisplayName) && nameTag.Currentname != AssociatedGamePlayer.ValidatedDisplayName)
			{
				nameTag.setNameBoxText(AssociatedGamePlayer.ValidatedDisplayName, this);
			}
		}
		else if (nameTag.Currentname != "")
		{
			nameTag.setNameBoxText("", this);
			nameTag.currentAlpha = 0f;
		}
		if (!Active && AssociatedGamePlayer != null)
		{
			Active = true;
		}
		if (AssociatedLobbyPlayer != null)
		{
			nameTag.UpdateIcons(AssociatedLobbyPlayer);
		}
		else
		{
			LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(networkNumber);
			if (lobbyPlayer != null)
			{
				nameTag.UpdateIcons(lobbyPlayer);
			}
		}
		if (cameraChangeTimeout > 0f)
		{
			cameraChangeTimeout -= Time.unscaledDeltaTime;
		}
		if (base.hasAuthority)
		{
			SetBubble(bubbleOn, Invincible && dancing && Modifiers.GetInstance().DanceInvincibility, playerColor);
		}
		if (hideNameTimer > 0f)
		{
			nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime * 3f);
			hideNameTimer -= Time.deltaTime;
		}
		else if (nameTag.Currentname != "" && LobbyManager.instance.IsInOnlineGame && Visible)
		{
			if (leftTrigger)
			{
				if (LocalPlayer != null && LocalPlayer.UseController.GetControllerType() == Controller.ControllerType.KEYBOARD)
				{
					hideNameTimer = GameSettings.GetInstance().emoteUIDisplayTime * 2f;
				}
				nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime * 3f);
			}
			else if (GameSettings.GetInstance().OnlinePlayerNames == OnlinePlayerNames.Auto)
			{
				if (Mathf.Abs(chrRigidBody.velocity.x) > 0.1f)
				{
					nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
					if ((double)nameTag.currentAlpha < 0.4)
					{
						nameBoxTimer = GameSettings.GetInstance().NameBoxTime;
					}
				}
				else
				{
					nameBoxTimer -= Time.unscaledDeltaTime;
					if (nameBoxTimer < 0f)
					{
						nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 1f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
					}
				}
			}
			else if (GameSettings.GetInstance().OnlinePlayerNames == OnlinePlayerNames.AlwaysOff)
			{
				nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
			}
			else
			{
				nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 1f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
			}
		}
		else
		{
			nameTag.currentAlpha = 0f;
		}
		AnimateIceStreaks();
		if (maxAFKTime != (float)GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime)
		{
			maxAFKTime = GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime;
			afkTimer = 0f;
		}
		if (LobbyManager.instance.IsInOnlineGame && maxAFKTime > 0f)
		{
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			if ((uint)(gameMode - 1) > 1u)
			{
				return;
			}
			if (AssociatedGamePlayer != null && AssociatedGamePlayer.LocalPlayer != null && !ignoreAFK)
			{
				if (IsControllerAFK && !Dying && !Dead && !Success && !Waiting)
				{
					afkTimer += Time.unscaledDeltaTime;
					SetAFKWarningTime(maxAFKTime - afkTimer, GameSettings.GetInstance().AFKWarningTime);
				}
				else
				{
					DisableAFKWarning();
				}
			}
			else if (AFKWarningEnabled)
			{
				afkTimer += Time.unscaledDeltaTime;
				SetAFKWarningTime(maxAFKTime - afkTimer, GameSettings.GetInstance().AFKWarningTime);
			}
		}
		else
		{
			afkTimer = 0f;
			if (AFKWarningVisible)
			{
				HideAFKWarning();
			}
		}
	}

	private void FixedUpdate()
	{
		if (frozen)
		{
			chrRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
		}
		else
		{
			chrRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
		if (feedColliderDeadPosition)
		{
			Feet.transform.position = DeadCollider.transform.position + feetPositionDead * base.transform.localScale.x;
		}
		Modifiers instance = Modifiers.GetInstance();
		SetPlayerPlayerColliders(instance.PlayerPlayerCollisions && !frozen && !Sitting, (secondaryAnim == SecondaryAnimState.CROUCH) ? ColliderStates.Crouching : ((dead || dying) ? ColliderStates.Dead : ColliderStates.Standing));
		bool flag = (AssociatedGamePlayer != null && !AssociatedGamePlayer.IsLocalPlayer) || (AssociatedLobbyPlayer != null && !AssociatedLobbyPlayer.IsLocalPlayer);
		bool flag2 = AssociatedGamePlayer != null || AssociatedLobbyPlayer != null;
		bool playerPlayerCollisions = Modifiers.GetInstance().PlayerPlayerCollisions;
		NonLocalColliderMode nonLocalColliderMode = NonLocalColliderMode.PickedNonLocal;
		if (Sitting)
		{
			nonLocalColliderMode = NonLocalColliderMode.PickedNonLocal;
		}
		else if (flag2)
		{
			if (!flag)
			{
				nonLocalColliderMode = NonLocalColliderMode.PickedLocal;
			}
		}
		else
		{
			nonLocalColliderMode = NonLocalColliderMode.Unpicked;
		}
		if (this.nonLocalColliderMode != nonLocalColliderMode || playerCollisionMode != playerPlayerCollisions)
		{
			SetNonLocalColliderMode(nonLocalColliderMode, playerPlayerCollisions);
		}
		Left.RaycastUpdate();
		Right.RaycastUpdate();
		Feet.RaycastUpdate();
		if (flag2 && !flag)
		{
			fullUpdate();
		}
		else if (isReplay)
		{
			replayUpdate();
		}
		else
		{
			clientUpdate();
		}
		if (base.hasAuthority && WantsToRetry && !disabled && GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
		{
			KillCharacter("Retry", deathFreezeOn: true, 0, force: true);
			SetLocalDeadCmdDead();
		}
		pushedByDoorThisFrame = false;
	}

	private void SetPlayerPlayerColliders(bool on, ColliderStates colliderState)
	{
		if (on)
		{
			switch (colliderState)
			{
			case ColliderStates.Standing:
				if (!IsBeingStomped && !playerPlayerCollider.gameObject.activeSelf)
				{
					playerPlayerCollider.gameObject.SetActive(value: true);
				}
				if (!playerPlayerColliderCrouch.gameObject.activeSelf)
				{
					playerPlayerColliderCrouch.gameObject.SetActive(value: true);
				}
				if (playerPlayerColliderDead.gameObject.activeSelf)
				{
					playerPlayerColliderDead.gameObject.SetActive(value: false);
				}
				break;
			case ColliderStates.Crouching:
				if (playerPlayerCollider.gameObject.activeSelf)
				{
					playerPlayerCollider.gameObject.SetActive(value: false);
				}
				if (!playerPlayerColliderCrouch.gameObject.activeSelf)
				{
					playerPlayerColliderCrouch.gameObject.SetActive(value: true);
				}
				if (playerPlayerColliderDead.gameObject.activeSelf)
				{
					playerPlayerColliderDead.gameObject.SetActive(value: false);
				}
				break;
			case ColliderStates.Dead:
				if (playerPlayerCollider.gameObject.activeSelf)
				{
					playerPlayerCollider.gameObject.SetActive(value: false);
				}
				if (playerPlayerColliderCrouch.gameObject.activeSelf)
				{
					playerPlayerColliderCrouch.gameObject.SetActive(value: false);
				}
				if (!playerPlayerColliderDead.gameObject.activeSelf)
				{
					playerPlayerColliderDead.gameObject.SetActive(value: true);
				}
				break;
			}
		}
		else
		{
			if (playerPlayerCollider.gameObject.activeSelf)
			{
				playerPlayerCollider.gameObject.SetActive(value: false);
			}
			if (playerPlayerColliderCrouch.gameObject.activeSelf)
			{
				playerPlayerColliderCrouch.gameObject.SetActive(value: false);
			}
			if (playerPlayerColliderDead.gameObject.activeSelf)
			{
				playerPlayerColliderDead.gameObject.SetActive(value: false);
			}
		}
	}

	private void SetNonLocalColliderMode(NonLocalColliderMode mode, bool playerCollisionMode)
	{
		if (nonLocalColliderMode != mode || this.playerCollisionMode != playerCollisionMode)
		{
			nonLocalColliderMode = mode;
			this.playerCollisionMode = playerCollisionMode;
			if (mode == NonLocalColliderMode.PickedLocal)
			{
				SetLayerRecursively(LayerMask.NameToLayer("Player"));
			}
			else
			{
				SetLayerRecursively(LayerMask.NameToLayer("NonLocalPlayer"));
			}
			if (mode == NonLocalColliderMode.PickedLocal)
			{
				headCollider.gameObject.layer = LayerMask.NameToLayer("PlayerNoTrigger");
				hazardHeadcollider.gameObject.layer = LayerMask.NameToLayer("PlayerNoTrigger");
				coinGrabber.gameObject.layer = LayerMask.NameToLayer("ItemTrigger");
			}
			if (playerCollisionMode && mode != NonLocalColliderMode.Unpicked)
			{
				int layer = LayerMask.NameToLayer("PlayerOnlyPhysics");
				playerPlayerCollider.gameObject.layer = layer;
				playerPlayerColliderCrouch.gameObject.layer = layer;
				playerPlayerColliderDead.gameObject.layer = layer;
			}
		}
	}

	private void fullUpdate()
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		if (paused || scoreboard || (frozen && !ForceAllowSuicide) || disabled)
		{
			ResetInput();
			return;
		}
		if (frozen && ForceAllowSuicide)
		{
			if (forceAllowSuicideTimer > 0f)
			{
				forceAllowSuicideTimer -= Time.deltaTime;
				if (forceAllowSuicideTimer < 0f)
				{
					forceAllowSuicideTimer = 0f;
				}
			}
			UpdateSuicidalState();
			ResetInput();
			return;
		}
		if (firstFrame)
		{
			ResetInput();
			firstFrame = false;
			return;
		}
		if (boostEffectTimer > 0f && !Waiting)
		{
			boostEffectTimer -= Time.deltaTime;
			if (boostEffectTimer < 0f)
			{
				boostEffectTimer = 0f;
			}
		}
		if (BoostEffect)
		{
			float b = instance.boostEffectLeftRightCurve.Evaluate(boostEffectTimer / instance.boostEffectDuration);
			left = Mathf.Min(left, b);
			right = Mathf.Min(right, b);
		}
		if (invincibleTimer > 0f && !Waiting)
		{
			invincibleTimer -= Time.deltaTime;
		}
		if (forceCrouchTimer > 0f && !Waiting)
		{
			forceCrouchTimer -= Time.deltaTime;
		}
		if (agonyTimer > 0f)
		{
			agonyTimer -= Time.deltaTime;
			if (agonyTimer < 0f)
			{
				agonyTimer = 0f;
			}
		}
		if (!leftTrigger)
		{
			if (left2 > 0.1f)
			{
				moveViewCurrent.left = Mathf.MoveTowards(moveViewCurrent.left, moveViewMax.left, moveViewSpeed * Time.fixedUnscaledDeltaTime * left2);
			}
			if (right2 > 0.1f)
			{
				moveViewCurrent.right = Mathf.MoveTowards(moveViewCurrent.right, moveViewMax.right, moveViewSpeed * Time.fixedUnscaledDeltaTime * right2);
			}
			if (up2 > 0.1f)
			{
				moveViewCurrent.up = Mathf.MoveTowards(moveViewCurrent.up, moveViewMax.down, moveViewSpeed * Time.fixedUnscaledDeltaTime * up2);
			}
			if (down2 > 0.1f)
			{
				moveViewCurrent.down = Mathf.MoveTowards(moveViewCurrent.down, moveViewMax.down, moveViewSpeed * Time.fixedUnscaledDeltaTime * down2);
			}
		}
		if (lookingUp)
		{
			moveViewTimer.up += Time.fixedDeltaTime;
			moveViewTimer.down = 0f;
			moveViewCurrent.down = Mathf.MoveTowards(moveViewCurrent.down, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		}
		else if (crouchingDown)
		{
			moveViewTimer.up = 0f;
			moveViewCurrent.up = Mathf.MoveTowards(moveViewCurrent.up, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		}
		else
		{
			moveViewTimer.up = 0f;
		}
		moveViewCurrent.up = Mathf.MoveTowards(moveViewCurrent.up, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		moveViewCurrent.left = Mathf.MoveTowards(moveViewCurrent.left, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		moveViewCurrent.right = Mathf.MoveTowards(moveViewCurrent.right, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		moveViewCurrent.down = Mathf.MoveTowards(moveViewCurrent.down, 0f, moveViewSpeedReturn * Time.fixedUnscaledDeltaTime);
		if (moveViewTimer.up > moveViewDelay)
		{
			moveViewCurrent.up = Mathf.MoveTowards(moveViewCurrent.up, moveViewMax.up, moveViewSpeed * Time.fixedUnscaledDeltaTime);
		}
		if (leftInput > 0.1f && !onWall)
		{
			leftResponseTime += Time.fixedDeltaTime;
		}
		else if (onWall)
		{
			leftResponseTime = 1f;
		}
		else if (rightInput > 0.1f)
		{
			leftResponseTime = 0f;
		}
		if (rightInput > 0.1f && !onWall)
		{
			rightResponseTime += Time.fixedDeltaTime;
		}
		else if (onWall)
		{
			rightResponseTime = 1f;
		}
		else if (leftInput > 0.1f)
		{
			rightResponseTime = 0f;
		}
		float num = chrRigidBody.velocity.x - baseHorizontalMotion;
		float num2 = chrRigidBody.velocity.y - baseVerticalMotion;
		if (instance2.PlayerPlayerCollisions)
		{
			num -= characterInheritedMotion.x;
			num2 -= characterInheritedMotion.y;
		}
		if (!Waiting && (LowerBodyCollider.Hazard || UpperBodyCollider.Hazard) && !succeeding && !success && (isZombie ? (!zombieLocallyDead) : (!LocallyDead)) && !dying)
		{
			string text = (LowerBodyCollider.Hazard ? LowerBodyCollider.HazardType : UpperBodyCollider.HazardType);
			bool flag = (LowerBodyCollider.Hazard ? LowerBodyCollider.DeathPit : UpperBodyCollider.DeathPit);
			int causedByPlayerNumber = (LowerBodyCollider.Hazard ? LowerBodyCollider.HazardPlacedByPlayer : UpperBodyCollider.HazardPlacedByPlayer);
			if (text.NullOrEmpty())
			{
				text = "World";
			}
			if (!Invincible || flag)
			{
				if (text == "WreckingBall" || text == "RollerCoaster")
				{
					setupDeath(text, deathFreezeOn: false, causedByPlayerNumber);
				}
				else
				{
					setupDeath(text, deathFreezeOn: true, causedByPlayerNumber);
				}
			}
		}
		if (!onGround && Feet.Colliding && !InBlackHole && (!HasJetpack || !jumping))
		{
			justLanded = true;
			if (dying && instance2.PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony)
			{
				audioEvent("_Agony_Land", base.gameObject);
			}
			else
			{
				audioEvent("_Land", base.gameObject);
			}
		}
		else
		{
			justLanded = false;
		}
		if (justLanded && lastFallSpeed < 0f - MinSmokeVelocity)
		{
			SpawnDustCloud();
		}
		bool flag2 = false;
		if (gapLifter.CanFloat())
		{
			bool flag3 = !gapLifter.speedCheck || (sprinting && Mathf.Abs(num) > minHorizontalGapSpeed);
			bool flag4 = !gapLifter.insideWallCheck || gapLifter.lastCollisionDistance > 0.01f;
			flag2 = !Feet.Colliding && flag3 && flag4;
		}
		int num3;
		int num4;
		if (!Dying && !Dead)
		{
			num3 = (Invincible ? 1 : 0);
			if (num3 == 0 && HasJetpack)
			{
				num4 = (jumping ? 1 : 0);
				goto IL_07db;
			}
		}
		else
		{
			num3 = 1;
		}
		num4 = 0;
		goto IL_07db;
		IL_07db:
		bool flag5 = (byte)num4 != 0;
		bool flag6 = num3 != 0 && Feet.CollidingHazard;
		NetworkonGround = (Feet.Colliding && !flag5 && !inBlackHole) || flag6 || flag2 || !Picked;
		bool flag7 = !instance2.WallSlidesDisabled && Left.CollidingWall;
		bool flag8 = !instance2.WallSlidesDisabled && Right.CollidingWall;
		bool flag9 = (onWall ? (!onGround && (flag7 || flag8) && !crouchingDown && !dying) : ((!(Mathf.Abs(previousPreviousVX) > minHorizontalWallStickSpeed)) ? (!onGround && ((flag7 && left > 0f) || (flag8 && right > 0f)) && !crouchingDown && !dying && UpperBodyCollider.Enabled) : (!onGround && (flag7 || flag8) && !crouchingDown && !dying && UpperBodyCollider.Enabled)));
		if (HasJetpack && jump)
		{
			flag9 = false;
		}
		if (flag9)
		{
			if (!onWall)
			{
				audioEvent("_WallSlideStart", base.gameObject);
			}
		}
		else if (onWall)
		{
			audioEvent("_WallSlideStop", base.gameObject);
		}
		onWall = flag9;
		if (Head.Colliding)
		{
			if (!collidingHead && !crouchingDown && !inBlackHole)
			{
				audioEvent("_HitObject", base.gameObject);
			}
			collidingHead = true;
		}
		else if (num2 < 0f)
		{
			collidingHead = false;
		}
		if ((!Dead && OnGround) || Mathf.Abs(num2) < 1.5f)
		{
			UpperBodyCollider.DisableClaw();
			LowerBodyCollider.DisableClaw();
		}
		else
		{
			if (crouchingDown)
			{
				UpperBodyCollider.DisableClaw();
			}
			else
			{
				UpperBodyCollider.EnableClaw();
			}
			LowerBodyCollider.EnableClaw();
		}
		float num5 = (instance2.Frictionless ? instance2.FrictionlessFrictionValue : Mathf.Clamp(groundFrictionModifier, -1f, 1f));
		if (BoostEffect)
		{
			num5 = instance2.FrictionlessFrictionValue;
		}
		if (dying)
		{
			deathTimer += Time.fixedDeltaTime;
			if ((Mathf.Abs(num) < 0.01f && Mathf.Abs(num2) < 0.01f) || inBlackHole)
			{
				deathSettleTimer += Time.fixedDeltaTime;
			}
			if (!jumping && !OnGround && !onWall)
			{
				boringDeathTimer += Time.fixedDeltaTime;
			}
			else
			{
				boringDeathTimer = 0f;
			}
			if (deathFrozen && deathTimer > freezeDeath)
			{
				deathTimer = 0f;
				deathFrozen = false;
				Vector3 vector = (LowerBodyCollider.Hazard ? LowerBodyCollider.HazardPoint : UpperBodyCollider.HazardPoint);
				Vector2 vector2 = base.transform.position - vector;
				vector2.y = Mathf.Abs(Mathf.Max(vector2.y, vector2.x));
				vector2.Normalize();
				num = deathPauseVel.x + vector2.x * DEATHFORCE;
				num2 = deathPauseVel.y + vector2.y * DEATHFORCE;
			}
			if (!waitingToTurnUndead && !dead && (!LocallyDead || (isZombie && !zombieLocallyDead)) && !succeeding)
			{
				if (deathTimer > minDeathDelay && deathSettleTimer > deathSettleTimeMax)
				{
					SetLocalDeadCmdDead();
				}
				else if (deathTimer > maxDeathDelay || boringDeathTimer > boringDeathTimerLimit)
				{
					SetLocalDeadCmdDead();
				}
			}
		}
		if (!dying && !succeeding)
		{
			if (forceCrouch)
			{
				crouchingDown = true;
			}
			else if (onGround)
			{
				if (down > 0.7f)
				{
					if (!crouchingDown)
					{
						audioEvent("_Crouch", base.gameObject);
					}
					crouchingDown = true;
				}
				else if (crouchingDown && down < 0.3f)
				{
					crouchingDown = false;
				}
				bool flag10 = up > 0.7f && !crouchingDown;
				if (instance2.playerPlayerCollisions && flag10)
				{
					flag10 = !Head.Colliding && !HazardHead.CollidingHazard && !IsBeingStomped;
				}
				if (flag10)
				{
					if (!lookingUp)
					{
						audioEvent("_Lookup", base.gameObject);
					}
					lookingUp = true;
				}
				else
				{
					lookingUp = false;
				}
			}
			else
			{
				crouchingDown = false;
				lookingUp = false;
			}
			bool flag11 = onGround;
			if (instance2.DanceInvincibility)
			{
				flag11 = flag11 || (dancing && !jump);
			}
			if (dance && flag11 && !crouchingDown && !lookingUp)
			{
				if (!dancing)
				{
					audioEvent("_Dance_Start", base.gameObject);
				}
				dancing = true;
			}
			else
			{
				if (dancing)
				{
					audioEvent("_Dance_Stop", base.gameObject);
				}
				dancing = false;
			}
			if (!crouchingDown)
			{
				if (crouchStandupTimer >= crouchStandupDelay)
				{
					bool flag12 = !Head.Colliding && !Head.CollidingWall;
					if (instance2.PlayerPlayerCollisions && flag12 && !UpperBodyTrigger.enabled)
					{
						flag12 = !IsBeingStomped;
					}
					if (flag12)
					{
						UpperBodyEnable(onOff: true);
						Left.raycastEnabled = true;
						Right.raycastEnabled = true;
					}
					else if (onGround)
					{
						crouchingDown = true;
						num2 = 0f;
					}
				}
				else
				{
					crouchStandupTimer += Time.fixedDeltaTime;
				}
			}
			if (crouchingDown)
			{
				UpperBodyEnable(onOff: false);
				Left.raycastEnabled = false;
				Right.raycastEnabled = false;
				crouchStandupTimer = 0f;
			}
		}
		UpdateSuicidalState();
		if (Ready || Waiting)
		{
			if (Ready)
			{
				if (Sitting)
				{
					if (animator.GetInteger(AnimToString(AnimParam.STATE)) != 3)
					{
						audioEvent("_Move_Stop", base.gameObject);
						audioEvent("_Sprint_Stop", base.gameObject);
						AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
					}
					SetAnimatorInt(AnimParam.STATE, 3);
				}
				else
				{
					if (animator.GetInteger("State") != 8)
					{
						audioEvent("_Move_Stop", base.gameObject);
						audioEvent("_Sprint_Stop", base.gameObject);
						AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
					}
					SetAnimatorInt(AnimParam.STATE, 8);
				}
			}
			if (Waiting)
			{
				num = 0f;
				num2 = 0f;
			}
			else if (!onGround)
			{
				num2 = Fall(num2);
				if (!BoostEffect)
				{
					num *= 0.95f;
				}
				else
				{
					float time = boostEffectTimer / instance.boostEffectDuration;
					num *= Mathf.Lerp(0.95f, 1f, instance.boostEffectCurve.Evaluate(time));
				}
			}
			else
			{
				if (num != 0f)
				{
					num = 0f;
					audioEvent("_Move_Stop", base.gameObject);
					AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
				}
				num2 = Fall(num2);
			}
			baseHorizontalMotion = 0f;
			baseVerticalMotion = 0f;
			characterInheritedMotion = Vector2.zero;
			chrRigidBody.velocity = new Vector2(num, num2);
			ResetVariables();
			return;
		}
		if (succeeding)
		{
			if (animator.GetInteger("State") != 8)
			{
				audioEvent("_Move_Stop", base.gameObject);
				audioEvent("_Sprint_Stop", base.gameObject);
				AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
			}
			SetAnimatorInt(AnimParam.STATE, 8);
			SetAnimatorInt(AnimParam.SECONDARYSTATE, 0);
			if (!success)
			{
				Networksuccess = true;
				CallCmdSetSuccess(s: true);
				succeedTimer = 0f;
				if (HasJetpack)
				{
					num2 = 0f;
				}
			}
			if (!onGround)
			{
				num2 = Fall(num2);
				num *= 0.95f;
			}
			else
			{
				num = 0f;
			}
			num += baseHorizontalMotion;
			if (instance2.PlayerPlayerCollisions)
			{
				num += characterInheritedMotion.x;
			}
			succeedTimer += Time.deltaTime;
			if (HasJetpack)
			{
				if (succeedTimer <= instance2.JetpackWinSpiralTime)
				{
					SetJetpackEmitting(onOff: true);
					float num6 = UnityEngine.Random.Range(0f - instance2.JetpackWinSpiralNoise, instance2.JetpackWinSpiralNoise);
					float angle = (succeedTimer + num6) * 360f * instance2.JetpackWinSpiralRPS;
					float num7 = instance2.JetpackWinSpiralCenterAccel * Time.fixedDeltaTime;
					Vector3 vector3 = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up * num7;
					vector3.y += instance2.JetpackWinSpiralUpAccel * Time.fixedDeltaTime;
					float time2 = succeedTimer / instance2.JetpackWinSpiralTime;
					vector3 *= instance2.JetPackWinSpiralDecay.Evaluate(time2);
					num += vector3.x;
					num2 += vector3.y;
				}
				else
				{
					SetJetpackEmitting(onOff: false);
				}
			}
			chrRigidBody.velocity = new Vector2(num, num2);
			ResetVariables();
			return;
		}
		if (dying || (LocallyDead && !isZombie && !isGhost))
		{
			SetAnimatorInt(AnimParam.STATE, 7);
			num2 = Fall(num2);
			SetAnimatorFloat(AnimParam.VERTICALDIRFLOAT, num2);
			if (!onGround || impulseAdded > 0f)
			{
				num *= 0.95f;
				SetAnimatorInt(AnimParam.SECONDARYSTATE, 3);
			}
			else
			{
				num *= 0.9f;
				SetAnimatorInt(AnimParam.SECONDARYSTATE, 0);
			}
			float num8 = baseHorizontalMotion + blackHoleX;
			if (instance2.PlayerPlayerCollisions)
			{
				num8 += characterInheritedMotion.x;
			}
			float num9 = 0f;
			num9 = (onGround ? ((num5 >= 0f) ? 0f : (1f - num5)) : 1f);
			num8 += windX * num9;
			num2 += windY * num9;
			num2 += blackholeY;
			num += num8;
			if (instance2.PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony && agonyStarted && !WantsToRetry)
			{
				bool flag13 = false;
				CheckCollidingPlayer component = DeadCollider.GetComponent<CheckCollidingPlayer>();
				if (component.Hazard && component.HazardType == "Drowning_In_Lava")
				{
					flag13 = true;
				}
				if (deathFrozen || flag13)
				{
					chrRigidBody.velocity = Vector2.zero;
				}
				else
				{
					agonyJumpDelayTimer = Mathf.Max(0f, agonyJumpDelayTimer - Time.deltaTime);
					float num10 = (sprint ? 1f : 0.5f);
					if (onGround)
					{
						if (Mathf.Abs(num - num8) > 0f && impulseAdded < 0.01f)
						{
							float num11 = num8 - num;
							num11 *= 0.9f;
							num = num8 - num11;
							chrRigidBody.velocity = new Vector2(num, num2);
						}
						loGravity = true;
						if (!RunTimerHit && jumpDown && agonyJumpDelayTimer == 0f)
						{
							agonyJumpDelayTimer = 0.1f;
							float x = (left * (0f - deathLimpForce.x) + right * deathLimpForce.x) * num10;
							AddImpulse(new Vector2(x, deathLimpForce.y));
							audioEvent("_Agony_Jump", base.gameObject);
							deathTimer = 0f;
							boringDeathTimer = 0f;
							SpawnJumpCloud();
							int num12 = flipSpriteX;
							if (left > 0.4f)
							{
								facing = -1;
							}
							else if (right > 0.4f)
							{
								facing = 1;
							}
							NetworkflipSpriteX = facing;
							if (num12 != flipSpriteX)
							{
								CallCmdSetScaleX(flipSpriteX);
								animator.transform.localScale = new Vector3(flipSpriteX, 1f, 1f);
							}
							CallCmdAgonize();
						}
					}
					else
					{
						if (left > 0.4f)
						{
							num += -1f * instance2.CharacterSizeSpeedMultiplier * num10 * deathLimpForce.x * Time.deltaTime;
						}
						else if (right > 0.4f)
						{
							num += 1f * instance2.CharacterSizeSpeedMultiplier * num10 * deathLimpForce.x * Time.deltaTime;
						}
						chrRigidBody.velocity = new Vector2(num, num2);
					}
				}
				if (HasJetpack)
				{
					SetJetpackEmitting(onOff: false);
				}
			}
			else if (deathFrozen)
			{
				chrRigidBody.velocity = Vector2.zero;
			}
			else
			{
				bool flag14 = HasJetpack && !diedInPit;
				switch (instance2.PostDeathBehavior)
				{
				case Modifiers.PostDeathBehaviors.Ghost:
					if (isExtraCorpse)
					{
						flag14 = false;
					}
					break;
				case Modifiers.PostDeathBehaviors.Agony:
					if (!HasJetpack)
					{
						agonyStarted = true;
					}
					break;
				}
				if (flag14)
				{
					if (deathTimer <= instance2.JetpackDeathSpiralTime)
					{
						SetJetpackEmitting(onOff: true);
						float num13 = UnityEngine.Random.Range(0f - instance2.JetpackDeathSpiralNoise, instance2.JetpackDeathSpiralNoise);
						float angle2 = (deathTimer + num13) * 360f * instance2.JetpackDeathSpiralRPS;
						float num14 = instance2.JetpackDeathSpiralCenterAccel * Time.fixedDeltaTime;
						Vector3 vector4 = Quaternion.AngleAxis(angle2, Vector3.forward) * Vector3.up * num14;
						vector4.y += instance2.JetpackDeathSpiralUpAccel * Time.fixedDeltaTime;
						float time3 = deathTimer / instance2.JetpackDeathSpiralTime;
						vector4 *= instance2.JetPackDeathSpiralDecay.Evaluate(time3);
						num += vector4.x;
						num2 += vector4.y;
						boringDeathTimer = 0f;
					}
					else
					{
						if (instance2.PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony)
						{
							agonyStarted = true;
							CallCmdAgonize();
						}
						SetJetpackEmitting(onOff: false);
					}
				}
				else if (HasJetpack)
				{
					SetJetpackEmitting(onOff: false);
				}
				chrRigidBody.velocity = new Vector2(num, num2);
			}
			ResetVariables();
			return;
		}
		if (Active)
		{
			idleTimer += Time.fixedDeltaTime;
		}
		else
		{
			idleTimer = 0f;
		}
		if (idleTimer >= IdleTime)
		{
			idleTimer = 0f;
		}
		if (onGround)
		{
			lastFallSpeed = 0f;
			if (CanJump)
			{
				jumpGraceTimer = 0f;
			}
			airInertiaModifierL = 0f;
			airInertiaModifierR = 0f;
			movedFromWall = true;
		}
		else
		{
			if (onWall)
			{
				airInertiaModifierL = 0f;
				airInertiaModifierR = 0f;
				if ((Right.Colliding && (right != 0f || left == 0f)) || (Left.Colliding && (left != 0f || right == 0f)))
				{
					wallStickTimer = 0f;
					movedFromWall = false;
				}
			}
			else
			{
				movedFromWall = true;
			}
			if (!jumping)
			{
				jumpGraceTimer += Time.fixedDeltaTime;
			}
			else
			{
				jumpGraceTimer = JumpGraceTime;
			}
		}
		if (movedFromWall)
		{
			wallStickTimer += Time.fixedDeltaTime;
		}
		if (Head.Colliding && !HasJetpack)
		{
			jumping = false;
		}
		if (instance2.PlayerPlayerCollisions)
		{
			DuplicateCharacterMotion();
		}
		else
		{
			characterInheritedMotion = Vector2.zero;
		}
		float num15 = analogueModifier.Evaluate(Mathf.Abs(right - left));
		float num16 = (sprint ? 1 : 0);
		float num17 = ResponseTimeModifier.Evaluate(Mathf.Max(leftResponseTime, rightResponseTime));
		if (onGround)
		{
			float num18 = RunSpeed * instance2.SprintSpeed * instance2.CharacterSizeSpeedMultiplier * (1f + num16 * instance2.DefaultSprintExtraSpeed) * ((num5 > 0f) ? (1f - num5) : 1f);
			float num19 = num15 * RunAccel * num18 * Time.fixedDeltaTime * ((num5 < 0f) ? (1f + num5 / 2f) : (1f - num5)) * ((num5 > 0f) ? 1f : (1f + num5 / 2f));
			if (isZombie)
			{
				num18 *= instance2.zombificationSpeedMultiplier;
				num19 *= instance2.zombificationSpeedMultiplier;
			}
			if (crouchingDown || (lookingUp && (Feet.NumberRayHits > 2 || (left < 0.3f && right < 0.3f))) || dancing)
			{
				if (left > 0.4f)
				{
					if (facing == 1)
					{
						AnimatorTrigger(AnimParam.FLIP);
						audioEvent("_FlipOnGround", base.gameObject);
					}
					facing = -1;
				}
				else if (right > 0.4f)
				{
					if (facing == -1)
					{
						AnimatorTrigger(AnimParam.FLIP);
						audioEvent("_FlipOnGround", base.gameObject);
					}
					facing = 1;
				}
				float target = 0f;
				bool flag15 = Head.CollidingWall || Head.Colliding || HazardHead.CollidingHazard;
				if (instance2.playerPlayerCollisions && !flag15)
				{
					flag15 = IsBeingStomped;
				}
				if (flag15)
				{
					target = 2f * num19 * (float)facing;
				}
				float num20 = 1f;
				if (crouchingDown && groundFrictionModifier > -0.1f && !justLanded)
				{
					num20 = 5f;
				}
				num = Mathf.MoveTowards(num, target, GroundFrictionForce * num20 * Time.fixedDeltaTime * ((num5 > 0f) ? 1f : (1f + num5)));
			}
			else if (left > 0f && right == 0f)
			{
				if (facing == 1)
				{
					AnimatorTrigger(AnimParam.FLIP);
					audioEvent("_FlipOnGround", base.gameObject);
				}
				if (num > 0f)
				{
					num = Mathf.MoveTowards(num, 0f, GroundFrictionForce * Time.fixedDeltaTime * ((num5 > 0f) ? 1f : (1f + num5)));
				}
				num -= num19;
				facing = -1;
				if (sprint)
				{
					GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.SPRINT);
				}
			}
			else if (right > 0f && left == 0f)
			{
				if (facing == -1)
				{
					AnimatorTrigger(AnimParam.FLIP);
					audioEvent("_FlipOnGround", base.gameObject);
				}
				if (num < 0f)
				{
					num = Mathf.MoveTowards(num, 0f, GroundFrictionForce * Time.fixedDeltaTime * ((num5 > 0f) ? 1f : (1f + num5)));
				}
				num += num19;
				facing = 1;
				if (sprint)
				{
					GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.SPRINT);
				}
			}
			else
			{
				num = Mathf.MoveTowards(num, 0f, GroundFrictionForce * Time.fixedDeltaTime * ((num5 > 0f) ? 1f : (1f + num5)));
				if (Mathf.Abs(num) < StopSpeed)
				{
					num = 0f;
				}
			}
			if (num > num18)
			{
				num = num18;
			}
			if (num < 0f - num18)
			{
				num = 0f - num18;
			}
			lastFootstep += Time.fixedDeltaTime;
			if (Mathf.Abs(num) > 0f)
			{
				if (!walking && !sprinting)
				{
					audioEvent("_Move_Start", base.gameObject);
					walking = true;
				}
				else if (!sprinting && num16 > 0f)
				{
					audioEvent("_Sprint_Start", base.gameObject);
					sprinting = true;
				}
				AkSoundEngine.SetRTPCValue("Character_Move_Speed", Mathf.Abs(num / (RunSpeed * instance2.SprintSpeed * instance2.CharacterSizeSpeedMultiplier * (1f + num16 * instance2.DefaultSprintExtraSpeed))), base.gameObject);
				if (sprinting && num16 == 0f)
				{
					audioEvent("_Sprint_Stop", base.gameObject);
					audioEvent("_Move_Start", base.gameObject);
					sprinting = false;
				}
			}
			else
			{
				if (walking)
				{
					audioEvent("_Move_Stop", base.gameObject);
					AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
				}
				walking = false;
				if (sprinting)
				{
					audioEvent("_Sprint_Stop", base.gameObject);
				}
				sprinting = false;
			}
		}
		else
		{
			if (walking)
			{
				audioEvent("_Move_Stop", base.gameObject);
				AkSoundEngine.SetRTPCValue("Character_Move_Speed", 0f, base.gameObject);
			}
			walking = false;
			if (sprinting)
			{
				audioEvent("_Sprint_Stop", base.gameObject);
			}
			sprinting = false;
			float num21 = RunSpeed * instance2.SprintSpeed * instance2.CharacterSizeSpeedMultiplier * (1f + num16 * instance2.DefaultSprintExtraSpeed) * num17;
			if (BoostEffect)
			{
				num21 = GetBoostLateralSpeedLimit(num21);
			}
			lastFootstep = 0f;
			float num22 = num15 * RunAccel * num21 * Time.fixedDeltaTime;
			if (isZombie)
			{
				num21 *= instance2.zombificationSpeedMultiplier;
				num22 *= instance2.zombificationSpeedMultiplier;
			}
			float num23 = AirInertia + airInertiaModifier;
			if (left > 0f)
			{
				if (sprint)
				{
					GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.SPRINT);
				}
				if (!onWall)
				{
					if (facing == 1)
					{
						AnimatorTrigger(AnimParam.FLIP);
					}
					facing = -1;
				}
				airInertiaModifierR = 0f;
				num23 += airInertiaModifierL;
				if (onWall && Right.Colliding)
				{
					movedFromWall = true;
					if (wallStickTimer >= WallStickTime)
					{
						num -= num22 * num23;
					}
				}
				else
				{
					num -= num22 * num23;
				}
			}
			else if (right > 0f)
			{
				if (sprint)
				{
					GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.SPRINT);
				}
				if (!onWall)
				{
					if (facing == -1)
					{
						AnimatorTrigger(AnimParam.FLIP);
					}
					facing = 1;
				}
				airInertiaModifierL = 0f;
				num23 += airInertiaModifierR;
				if (onWall && Left.Colliding)
				{
					movedFromWall = true;
					if (wallStickTimer >= WallStickTime)
					{
						num += num22 * num23;
					}
				}
				else
				{
					num += num22 * num23;
				}
			}
			else
			{
				if (onWall && !jumping)
				{
					if (Left.Colliding)
					{
						num -= WallSlidePressure;
					}
					if (Right.Colliding)
					{
						num += WallSlidePressure;
					}
				}
				airInertiaModifierR = 0f;
				airInertiaModifierL = 0f;
			}
			if (num > num21)
			{
				num = num21;
			}
			if (num < 0f - num21)
			{
				num = 0f - num21;
			}
		}
		float num24 = (isZombie ? instance2.zombificationJumpSpeedMultiplier : 1f);
		if ((onWall && !instance2.WallSlidesDisabled) || onGround)
		{
			AirJumps = 0;
		}
		if (forcedJump && !HasJetpack && (!jumpDown || AirJumps >= instance2.MaxAirJumps))
		{
			if (jumpDown && CanJump)
			{
				PreJumpGraceTimer = PreJumpGraceTime;
				timeSinceJump = 0f;
			}
			if (jump)
			{
				PreJumpGraceTimer -= Time.fixedDeltaTime;
			}
			if (onWall && PreJumpGraceTimer > 0f)
			{
				forcedJump = false;
				loGravity = false;
				jumping = false;
				if (onWall || onGround)
				{
					CanJump = true;
				}
			}
			timeSinceJump += Time.fixedDeltaTime;
			AirJumpGraceTimer -= Time.deltaTime;
			if (AirJumpGraceTimer < 0f)
			{
				AirJumpGraceTimer = 0f;
			}
			loGravity = true;
			if (jump && num2 <= VelocityPeak)
			{
				loGravity = false;
			}
			else if (!jump && num2 <= forcedJumpPeakVelocity)
			{
				loGravity = false;
			}
			if (!jump)
			{
				CanJump = true;
			}
			if (num2 <= 0f)
			{
				jumping = false;
				loGravity = false;
				forcedJump = false;
				CanJump = false;
			}
		}
		else
		{
			bool flag16 = false;
			if (!onGround && !onWall && AirJumps < instance2.MaxAirJumps && boostEffectTimer < instance.cannonAirJumpTimeOut)
			{
				AirJumpGraceTimer -= Time.deltaTime;
				if (AirJumpGraceTimer < 0f)
				{
					AirJumpGraceTimer = 0f;
				}
				if (!(jumpGraceTimer < JumpGraceTime) && jumpDown && AirJumpGraceTimer == 0f)
				{
					AirJumpGraceTimer += AirJumpGraceTime;
					AirJumps++;
					flag16 = true;
					loGravity = true;
					jumping = true;
					CanJump = false;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					num2 += instance2.JumpSpeed * instance2.CharacterSizeSpeedMultiplier * num24 * (1f + jumpForceModifier);
					if (HasJetpack)
					{
						num2 *= instance2.JetpackJumpSpeedModifier;
					}
					audioEvent("_Jump", base.gameObject);
					PreJumpGraceTimer = 0f;
					heightJumped = 0f;
					SpawnJumpCloud(multiJump: true);
					StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true)?.IncrementStat("Jumps");
				}
			}
			if (!flag16 && (onGround || (jumpGraceTimer < JumpGraceTime && num2 <= 0f)))
			{
				if (jump || jumpDown || forceNextJump)
				{
					if (justLanded && jumpDown)
					{
						CanJump = true;
						jumping = false;
						heightJumped = 0f;
						loGravity = true;
					}
					if (((!jumping && CanJump) || PreJumpGraceTimer > 0f) && impulseAdded <= 0f)
					{
						loGravity = true;
						if (num2 < 0f)
						{
							num2 = 0f;
						}
						num2 += instance2.JumpSpeed * instance2.CharacterSizeSpeedMultiplier * num24 * (1f + jumpForceModifier);
						if (HasJetpack)
						{
							num2 *= instance2.JetpackJumpSpeedModifier;
						}
						jumping = true;
						if (dancing)
						{
							audioEvent("_Dance_Stop", base.gameObject);
						}
						dancing = false;
						CanJump = false;
						if (!forceNextJump)
						{
							audioEvent("_Jump", base.gameObject);
						}
						PreJumpGraceTimer = 0f;
						heightJumped = 0f;
						SpawnJumpCloud();
						SpawnHoneyStickers();
						StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true)?.IncrementStat("Jumps");
					}
					if (forceNextJump)
					{
						jumpDown = false;
						jump = false;
						forceNextJump = false;
					}
				}
				else
				{
					PreJumpGraceTimer = 0f;
					CanJump = true;
					jumping = false;
					heightJumped = 0f;
					loGravity = true;
				}
				timeSinceJump = 0f;
			}
			else if (jump)
			{
				if (jumpDown && !jumping)
				{
					PreJumpGraceTimer = PreJumpGraceTime;
					timeSinceJump = 0f;
				}
				PreJumpGraceTimer -= Time.fixedDeltaTime;
				if (jumping)
				{
					timeSinceJump += Time.fixedDeltaTime;
				}
				loGravity = true;
				if (HasJetpack)
				{
					loGravity = false;
					jumping = true;
					num2 += instance2.jetpackThrust * instance2.jetpackVelocityModifier.Evaluate(num2) * Time.fixedDeltaTime;
					if (facing == 0)
					{
						facing = flipSpriteX;
					}
					num += (float)facing * instance2.JetpackHorizontalForcedSpeed * Time.fixedDeltaTime;
				}
				if (num2 <= VelocityPeak && !HasJetpack)
				{
					jumping = false;
					loGravity = false;
				}
				bool flag17 = onWall || (instance2.WallSlidesDisabled && (Left.CollidingWall || Right.CollidingWall));
				if ((onWall || flag17) && !instance2.WallJumpsDisabled && PreJumpGraceTimer > 0f)
				{
					jumping = true;
					CanJump = false;
					num = 0f;
					PreJumpGraceTimer = 0f;
					float num25 = RunSpeed * instance2.WallJumpHorizontalPush * (1f + num16 * instance2.DefaultSprintExtraSpeed) * (1f + jumpForceModifier);
					if (isZombie)
					{
						num25 *= instance2.zombificationSpeedMultiplier;
					}
					if (HasJetpack)
					{
						num25 *= instance2.JetpackWallJumpSpeedModifier;
					}
					if (Left.Colliding)
					{
						wallJumpThisFrame = true;
						num += num25;
						if (left != 0f)
						{
							facing = -1;
							airInertiaModifierL = WallJumpInertiaModifier;
						}
						else
						{
							facing = 1;
						}
						SpawnWallJumpCloud(right: false);
					}
					if (Right.Colliding)
					{
						wallJumpThisFrame = true;
						num -= num25;
						if (right != 0f)
						{
							airInertiaModifierR = WallJumpInertiaModifier;
							facing = 1;
						}
						else
						{
							facing = -1;
						}
						SpawnWallJumpCloud(right: true);
					}
					num2 = instance2.WallJumpVerticalPush * (1f + jumpForceModifier / 2f);
					if (HasJetpack)
					{
						num2 *= instance2.JetpackJumpSpeedModifier;
					}
					loGravity = false;
					heightJumped = 0f;
					audioEvent("_WallJump", base.gameObject);
					SpawnHoneyStickers();
					SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true);
					if (saveFileDataForLocalPlayer != null)
					{
						StatCount stat = saveFileDataForLocalPlayer.GetStat<StatCount>("WallJumps");
						stat.Increment(1);
						if (stat.count == 1000)
						{
							AchievementChecker.Instance.Neat_and_Nimble_AchievementChecks(saveFileDataForLocalPlayer);
						}
					}
				}
			}
			else
			{
				jumping = false;
				loGravity = false;
				if (onWall)
				{
					CanJump = true;
					loGravity = false;
					if (num2 <= VelocityPeak)
					{
						loGravity = false;
					}
				}
				PreJumpGraceTimer = 0f;
			}
		}
		if (!AffectedByImpulse && !flag2 && (!Feet.Colliding || gapLifter.obstacleStraightDown || Mathf.Abs(num) < minHorizontalGapSpeed || !sprinting))
		{
			num2 = Fall(num2);
		}
		if (jumping && num2 > 0f)
		{
			heightJumped += num2 * Time.fixedDeltaTime;
		}
		if (HasJetpack)
		{
			if (!JetPackSR.enabled)
			{
				JetPackSR.enabled = true;
			}
		}
		else if (JetPackSR.enabled)
		{
			JetPackSR.enabled = false;
		}
		SetJetpackEmitting(HasJetpack && jumping && !succeeding);
		SetAnimatorFloat(AnimParam.VERTICALDIRFLOAT, num2);
		SecondaryAnimState value = SecondaryAnimState.NONE;
		if (onGround)
		{
			if (crouchingDown)
			{
				value = SecondaryAnimState.CROUCH;
			}
			if (Mathf.Abs(num) > 0f && (left > 0f || right > 0f) && !dancing)
			{
				if (num16 > 0f)
				{
					SetAnimatorInt(AnimParam.STATE, 1);
				}
				else
				{
					SetAnimatorInt(AnimParam.STATE, 2);
				}
			}
			else if (dancing || (suggestDance && !crouchingDown && !lookingUp))
			{
				SetAnimatorInt(AnimParam.STATE, 8);
			}
			else if (lookingUp)
			{
				value = SecondaryAnimState.LOOKUP;
				SetAnimatorInt(AnimParam.STATE, 0);
			}
			else
			{
				SetAnimatorInt(AnimParam.STATE, 0);
			}
		}
		else if (onWall)
		{
			SetAnimatorInt(AnimParam.STATE, 6);
			if (Left.Colliding && !Right.Colliding)
			{
				if (facing != 1)
				{
					AnimatorTrigger(AnimParam.FLIP);
				}
				facing = 1;
			}
			else if (Right.Colliding && !Left.Colliding)
			{
				if (facing != -1)
				{
					AnimatorTrigger(AnimParam.FLIP);
				}
				facing = -1;
			}
			else if (Right.Colliding && Left.Colliding)
			{
				if (left > 0.1f)
				{
					if (facing != 1)
					{
						AnimatorTrigger(AnimParam.FLIP);
					}
					facing = 1;
				}
				else if (right > 0.1f)
				{
					if (facing != -1)
					{
						AnimatorTrigger(AnimParam.FLIP);
					}
					facing = -1;
				}
			}
		}
		else if (crouchingDown)
		{
			value = SecondaryAnimState.CROUCH;
		}
		else if (dancing)
		{
			SetAnimatorInt(AnimParam.STATE, 8);
		}
		else
		{
			SetAnimatorInt(AnimParam.STATE, 4);
		}
		if (forceCrouch)
		{
			value = SecondaryAnimState.CROUCH;
		}
		SetAnimatorInt(AnimParam.SECONDARYSTATE, (int)value);
		int num26 = flipSpriteX;
		if (!onWall)
		{
			if (facing == -1)
			{
				NetworkflipSpriteX = -1;
			}
			if (facing == 1)
			{
				NetworkflipSpriteX = 1;
			}
		}
		else
		{
			if (facing == -1)
			{
				NetworkflipSpriteX = 1;
			}
			if (facing == 1)
			{
				NetworkflipSpriteX = -1;
			}
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (!currentAnimatorStateInfo.IsName("SlideToGroundMirror") && !currentAnimatorStateInfo.IsName("SlideToGroundTwist"))
		{
			if (num26 != flipSpriteX)
			{
				CallCmdSetScaleX(flipSpriteX);
			}
			animator.transform.localScale = new Vector3(flipSpriteX, 1f, 1f);
		}
		if (!onWall && !jumpDown)
		{
			num += baseHorizontalMotion;
		}
		if (instance2.PlayerPlayerCollisions)
		{
			num += characterInheritedMotion.x;
		}
		if (!dying && !deathFrozen)
		{
			if (BoostEffect)
			{
				float boostVMaxX = BoostVMaxX;
				num = Mathf.Clamp(num, 0f - boostVMaxX, boostVMaxX);
			}
			else
			{
				num = Mathf.Clamp(num, 0f - VMax.x, VMax.x);
			}
			float num27 = 0f;
			num27 = ((!BoostEffect) ? Mathf.Max(VMax.y, instance2.JumpSpeed * 1.05f * instance2.CharacterSizeSpeedMultiplier) : Mathf.Max(BoostVMaxY, instance2.JumpSpeed * 1.05f * instance2.CharacterSizeSpeedMultiplier));
			num2 = Mathf.Clamp(num2, 0f - num27, num27);
		}
		if (num2 >= 0f)
		{
			lastFallSpeed = 0f;
		}
		float num28 = 0f;
		num28 = (onGround ? ((num5 >= 0f) ? 0f : (1f - num5)) : 1f);
		num += windX * num28;
		num2 += windY * num28;
		Vector3 position = base.transform.position;
		if (Feet.Colliding && !Feet.CollidingSpecialButton)
		{
			if (num2 < baseVerticalMotion)
			{
				num2 = baseVerticalMotion;
				if (instance2.PlayerPlayerCollisions)
				{
					num2 += characterInheritedMotion.y;
				}
			}
			float num29 = Feet.ScaledMargin - Feet.lastCollisionDistance;
			position.y += num29 + Feet.baseVerticalMotionOfObject * Time.fixedDeltaTime;
		}
		if (!Feet.Colliding && flag2)
		{
			if (num2 < 0f)
			{
				num2 = baseVerticalMotion;
				if (instance2.PlayerPlayerCollisions)
				{
					num2 += characterInheritedMotion.y;
				}
			}
			float num30 = gapLifter.margin - gapLifter.lastCollisionDistance;
			position.y += num30;
		}
		if (Left.CollidingWall && (timeSinceJump > 0.01f || timeSinceJump > 0.1f))
		{
			if (left >= right)
			{
				if (!jumpDown && !wallJumpThisFrame)
				{
					num = 0f;
					float num31 = Left.ScaledMargin - Left.lastCollisionDistance + Left.baseHorizontalMotionOfObject * Time.fixedDeltaTime;
					position.x += num31;
				}
				if (instance2.PlayerPlayerCollisions)
				{
					num += characterInheritedMotion.x;
				}
			}
		}
		else if (!OnGround && Left.approachDistance + num * Time.fixedDeltaTime < Left.ScaledMargin)
		{
			num = (0f - (Left.approachDistance - Left.ScaledMargin)) / Time.fixedDeltaTime + baseHorizontalMotion;
		}
		if (Right.CollidingWall && (timeSinceJump > 0.01f || timeSinceJump > 0.1f))
		{
			if (right >= left)
			{
				if (!jumpDown && !wallJumpThisFrame)
				{
					num = 0f;
					float num32 = Right.ScaledMargin - Right.lastCollisionDistance - Right.baseHorizontalMotionOfObject * Time.fixedDeltaTime;
					position.x -= num32;
				}
				if (instance2.PlayerPlayerCollisions)
				{
					num += characterInheritedMotion.x;
				}
			}
		}
		else if (!OnGround && Right.approachDistance - num * Time.fixedDeltaTime < Right.ScaledMargin)
		{
			num = (Right.approachDistance - Right.ScaledMargin) / Time.fixedDeltaTime + baseHorizontalMotion;
		}
		num += blackHoleX;
		num2 += blackholeY;
		base.transform.position = position;
		previousPreviousVX = previousVX;
		previousPreviousVY = previousVY;
		previousVX = num;
		previousVY = num2;
		if (onGround)
		{
			SaveFileData saveFileDataForLocalPlayer2 = StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true);
			if (Mathf.Abs(num) > 0.01f)
			{
				saveFileDataForLocalPlayer2?.IncrementStat("DistanceRun", Mathf.Abs(num) * Time.fixedDeltaTime);
			}
		}
		else if (onWall)
		{
			StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true)?.IncrementStat("DistanceSlid", Mathf.Abs(num) * Time.fixedDeltaTime);
		}
		chrRigidBody.velocity = new Vector2(num, num2);
		lastFallSpeed = Mathf.Min(lastFallSpeed, num2);
		if (boostEffectTimer > 0f && !Waiting)
		{
			AnimState animState = currentAnim;
			if ((uint)animState <= 2u || (uint)(animState - 5) <= 1u)
			{
				boostEffectTimer = 0f;
			}
		}
		ResetVariables();
	}

	private void replayUpdate()
	{
		if (replayData != null && isReplaying && !replayPaused)
		{
			float num = Time.realtimeSinceStartup - replayStartTime;
			currentReplayFrame = replayData.GetDataForTime(num);
			if (num != currentReplayFrame.frameTimestamp)
			{
				Vector3 vector = (currentReplayFrame.position - currentReplayFrame.framePosition) / (num - currentReplayFrame.frameTimestamp);
				previousVX = vector.x;
				previousVY = vector.y;
			}
			else
			{
				previousVX = 0f;
				previousVY = 0f;
			}
			base.transform.position = currentReplayFrame.position;
			foreach (KeyValuePair<GhostData.GhostEvent, object> item in currentReplayFrame)
			{
				switch (item.Key)
				{
				case GhostData.GhostEvent.Invalid:
					Debug.LogError("Playing back an invalid keyframe!");
					break;
				case GhostData.GhostEvent.AnimState:
					NetworkcurrentAnim = (AnimState)item.Value;
					break;
				case GhostData.GhostEvent.SecondaryAnim:
					NetworksecondaryAnim = (SecondaryAnimState)item.Value;
					break;
				case GhostData.GhostEvent.Flipped:
					NetworkflipSpriteX = (int)item.Value;
					break;
				case GhostData.GhostEvent.Zombie:
					isZombie = (bool)item.Value;
					break;
				case GhostData.GhostEvent.Jetpack:
					if ((bool)item.Value)
					{
						JetpackParticles.Play();
					}
					else
					{
						JetpackParticles.Stop();
					}
					break;
				case GhostData.GhostEvent.Coin:
				{
					int num2 = (int)item.Value;
					if (num2 == 0)
					{
						CoinNumberText.text = "0";
						CoinCanvas.SetActive(value: false);
					}
					else
					{
						CoinCanvas.SetActive(value: true);
						CoinNumberText.text = num2.ToString();
					}
					break;
				}
				case GhostData.GhostEvent.Stopwatch:
					SlowedIcon.SetActive((bool)item.Value);
					break;
				}
			}
		}
		clientUpdate(doPhysics: false);
	}

	public void OnRunTimerHit()
	{
		if (!dying && !dead && !succeeding && !success)
		{
			KillCharacter("Run Timer", deathFreezeOn: false, 0);
			SetLocalDeadCmdDeadAvoidRespawn();
		}
	}

	public void SetupReplay(GhostData data)
	{
		if (data == null)
		{
			Debug.LogError("No ghost data set for replay character");
		}
		replayData = data;
		isReplay = true;
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		base.transform.position = data.GetDataForTime(0f, interpolate: false).position;
		SetOutfitsFromArray(data.Outfits);
		nameTag.setNameBoxText(data.PlayerName, this);
	}

	public void StartReplay()
	{
		isReplaying = true;
		UpperBodyEnable(onOff: false);
		LowerBodyEnable(onOff: false);
		DeadCollider.enabled = false;
		replayStartTime = Time.realtimeSinceStartup;
	}

	public void PauseReplay()
	{
		replayPaused = true;
		replayPauseTime = Time.realtimeSinceStartup;
	}

	public void ResumeReplay()
	{
		replayStartTime += Time.realtimeSinceStartup - replayPauseTime;
		replayPaused = false;
	}

	public void StopReplay()
	{
		isReplaying = false;
		replayPauseTime = Time.realtimeSinceStartup;
		replayStartTime = Time.realtimeSinceStartup;
	}

	private void clientUpdate(bool doPhysics = true)
	{
		animator.transform.localScale = new Vector3(flipSpriteX, 1f, 1f);
		if (doPhysics)
		{
			SetAnimatorFloat(AnimParam.VERTICALDIRFLOAT, chrRigidBody.velocity.y);
		}
		else
		{
			SetAnimatorFloat(AnimParam.VERTICALDIRFLOAT, previousVX);
		}
		if (doPhysics)
		{
			previousVX = chrRigidBody.velocity.x;
			previousVY = chrRigidBody.velocity.y;
		}
		if (animator.GetInteger(AnimToString(AnimParam.STATE)) != (int)currentAnim)
		{
			animator.SetInteger(AnimToString(AnimParam.STATE), (int)currentAnim);
		}
		if (animator.GetInteger(AnimToString(AnimParam.SECONDARYSTATE)) != (int)secondaryAnim)
		{
			animator.SetInteger(AnimToString(AnimParam.SECONDARYSTATE), (int)secondaryAnim);
		}
		if (doPhysics)
		{
			if (animator.GetInteger(AnimToString(AnimParam.SECONDARYSTATE)) == 2)
			{
				UpperBodyEnable(onOff: false);
			}
			else
			{
				DeadColliderSwitch(dying || dead);
			}
			NetworkonGround = Feet.Colliding && !inBlackHole;
		}
		AnimateIceStreaks();
		if (agonyTimer > 0f)
		{
			agonyTimer -= Time.deltaTime;
			if (agonyTimer < 0f)
			{
				agonyTimer = 0f;
			}
		}
		ResetVariables();
	}

	public void KillCharacter(string cause, bool deathFreezeOn, int causedByPlayerNumber, bool force = false)
	{
		if (((!dead && !dying) || (isZombie && !dying)) && !success && !succeeding && (force || !Invincible) && base.hasAuthority)
		{
			setupDeath(cause, deathFreezeOn, causedByPlayerNumber);
		}
	}

	public bool IsStandingOn(GameObject targetObject)
	{
		if (!onGround || targetObject == null)
		{
			return false;
		}
		return physicsModifiers.ContainsKey(targetObject);
	}

	private void RemoveAllPhysicsModifers()
	{
		foreach (PhysicsModifier value in physicsModifiers.Values)
		{
			RemovePhysicsModifier(value, removedFromDict: false);
		}
		physicsModifiers.Clear();
	}

	private void ResetVariables()
	{
		ResetInput();
		wallJumpThisFrame = false;
		Head.Colliding = false;
		Head.CollidingWall = false;
		Head.CollidingHazard = false;
		HazardHead.Colliding = false;
		HazardHead.CollidingWall = false;
		HazardHead.CollidingHazard = false;
		impulseAdded -= Time.fixedDeltaTime;
		if (impulseAdded < 0f)
		{
			impulseAdded = 0f;
		}
		if (nonLocalColliderMode == NonLocalColliderMode.PickedNonLocal)
		{
			lastInheritedMotion = (base.transform.position - previousPosition) / Time.deltaTime;
			previousPosition = base.transform.position;
		}
		else
		{
			lastInheritedMotion.x = baseHorizontalMotion;
			lastInheritedMotion.y = baseVerticalMotion;
		}
		RemoveAllPhysicsModifers();
		if (LobbyManager.instance.IsInOnlineGame && lastJetpackState != Jetpacking)
		{
			if (base.hasAuthority)
			{
				CallCmdSetRemoteJetpackState(Jetpacking);
			}
			else
			{
				SetJetpackEmitting(lastJetpackState);
			}
			lastJetpackState = Jetpacking;
		}
	}

	private float Fall(float vy)
	{
		Modifiers instance = Modifiers.GetInstance();
		if (Sitting || Ready)
		{
			return 0f;
		}
		if (loGravity)
		{
			gravityTransitionTime = 0f;
		}
		else
		{
			gravityTransitionTime = Mathf.MoveTowards(gravityTransitionTime, JumpCapTransitionTime.Evaluate(timeSinceJump), Time.fixedDeltaTime);
		}
		float num = ((!onGround || jumping) ? Mathf.Lerp(loGravityMod, GravityCurve.Evaluate((jumping || !CanJump || onWall) ? vy : 0f), gravityTransitionTime / JumpCapTransitionTime.Evaluate(timeSinceJump)) : ((physicsModifiers.Count == 0) ? GravityModOnGround : ((!(vy < 0f)) ? 1f : 0f)));
		if ((dying && !HasJetpack) || (HasJetpack && succeeding))
		{
			num = loGravityModDead;
		}
		float num2 = ((HasJetpack && jumping && !dying && !succeeding) ? 0f : GetGravityModifier());
		vy -= num2 * num * (1f + gravityModifier) * Time.fixedDeltaTime;
		if (onWall)
		{
			float num3 = Mathf.Clamp(wallFrictionModifier, -1f, 1f);
			float num4 = 0f - SlideTerminalVelocity;
			if (up > 0f && down == 0f)
			{
				num4 = 0f - Mathf.Lerp(SlideTerminalVelocity, SlowSlideTerminalVelocity, up);
			}
			else if (down > 0f && up == 0f)
			{
				num4 = 0f - Mathf.Lerp(SlideTerminalVelocity, FastSlideTerminalVelocity, down);
			}
			if (vy < num4 * (1f - num3))
			{
				vy = Mathf.MoveTowards(vy, num4 * (1f - num3), WallFrictionForce * Time.fixedDeltaTime);
			}
		}
		else if (vy < 0f - FallTerminalVelocity)
		{
			vy = 0f - FallTerminalVelocity;
		}
		vy += baseVerticalMotion;
		if (instance.PlayerPlayerCollisions)
		{
			vy += characterInheritedMotion.y;
		}
		return vy;
	}

	public void ForceGravity(Modifiers.GravityType gravityMode)
	{
		forcedGravity = true;
		forcedGravityMode = gravityMode;
	}

	public void RemoveForcedGravity()
	{
		forcedGravity = false;
		forcedGravityMode = Modifiers.GravityType.NORMAL;
	}

	public void SetForcedGravityMultiplier(float multiplier)
	{
		forcedGravityMultiplier = multiplier;
	}

	private float GetGravityModifier()
	{
		if (forcedGravity)
		{
			return Modifiers.GetInstance().GravityValues[(int)forcedGravityMode] * forcedGravityMultiplier;
		}
		return Modifiers.GetInstance().Gravity;
	}

	private bool IsGoal(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Goal);
		}
		return obj.tag == "Goal";
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if ((!isGhost || isZombie) && IsGoal(c.gameObject, component) && !succeeding)
		{
			if (isExtraCorpse)
			{
				soul.OnGoalTouched(c);
			}
			else
			{
				OnGoalTouched(c);
			}
		}
	}

	private void OnGoalTouched(Collider2D c)
	{
		if (suicide)
		{
			return;
		}
		GameControl currentGameController = LobbyManager.instance.CurrentGameController;
		if (currentGameController != null && currentGameController.LevelLayout != null)
		{
			float sqrMagnitude = (base.transform.position - currentGameController.LevelLayout.StartPoint.position).sqrMagnitude;
			float sqrMagnitude2 = (base.transform.position - c.transform.position).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				return;
			}
		}
		if (c.attachedRigidbody != null)
		{
			GoalBlock component = c.attachedRigidbody.GetComponent<GoalBlock>();
			NetworkLastFlagID = ((component != null) ? component.ID : (-1));
		}
		audioEvent("_CatchFlag", base.gameObject);
		if (base.hasAuthority)
		{
			MsgCharacterSuccess msgCharacterSuccess = new MsgCharacterSuccess();
			msgCharacterSuccess.NetworkPlayerNumber = networkNumber;
			NetworkManager.singleton.client.Send(NetMsgTypes.CharacterSuccess, msgCharacterSuccess);
			CallCmdSetLastFlagID(LastFlagID);
		}
		succeeding = true;
	}

	public void DeadColliderSwitch(bool enabled, bool resetPosition = false)
	{
		if (enabled)
		{
			UpperBodyEnable(onOff: false);
			LowerBodyEnable(onOff: false);
			feedColliderDeadPosition = true;
			FeetPhysicsCollider.GetComponent<Collider2D>().enabled = false;
		}
		else
		{
			UpperBodyEnable(onOff: true);
			LowerBodyEnable(onOff: true);
			feedColliderDeadPosition = false;
			Feet.transform.localPosition = feetPositionAlive;
			FeetPhysicsCollider.GetComponent<Collider2D>().enabled = true;
		}
		DeadCollider.enabled = enabled;
		if (enabled && resetPosition)
		{
			DeadCollider.transform.localPosition = DeadColliderInitialPosition;
		}
	}

	public void PositionCharacter(Vector3 position, bool groundScaleOffset = false)
	{
		if (groundScaleOffset)
		{
			position += Level.GetSpawnFeetOffset();
		}
		previousPosition = position;
		smoothSync.teleport(-1, position, Quaternion.identity);
		if (base.hasAuthority)
		{
			CallCmdPositionCharacter(position);
		}
	}

	public void Enable(bool playSound = true)
	{
		disabled = false;
		scoreboard = false;
		frozen = false;
		onWall = false;
		jumping = false;
		heightJumped = 0f;
		CanJump = false;
		deathFrozen = false;
		inBlackHole = false;
		inCannon = false;
		succeeding = false;
		firstFrame = true;
		justLanded = false;
		Ready = false;
		Sitting = false;
		dance = false;
		dancing = false;
		suggestDance = false;
		forcedJump = false;
		LastDeath = "";
		diedInPit = false;
		LocallyDead = false;
		afkTimer = 0f;
		HideAFKWarning();
		wantsToRetryUsed = false;
		if (base.hasAuthority && !isExtraCorpse)
		{
			NetworkonGround = false;
			SetDying(d: false);
			SetDead(d: false);
			SetSuccess(s: false);
			SetWantsToRetry(value: false);
		}
		ResetInput();
		SetJetpackEmitting(onOff: false);
		lastJetpackState = false;
		left = 0f;
		right = 0f;
		up = 0f;
		down = 0f;
		if (suicide)
		{
			backDownOnEnable = true;
		}
		suicide = false;
		deathTimer = 0f;
		deathSettleTimer = 0f;
		boringDeathTimer = 0f;
		PreJumpGraceTimer = 0f;
		DeadColliderSwitch(enabled: false);
		if (forceCrouch)
		{
			UpperBodyEnable(onOff: false);
		}
		idleTimer = 0f;
		CoinsCollected = 0;
		Visible = true;
		if (base.hasAuthority && !isExtraCorpse)
		{
			SetAnimatorInt(AnimParam.STATE, 0);
		}
		else
		{
			SetAnimatorInt(AnimParam.STATE, (int)currentAnim);
			if (!forceCrouch)
			{
				SetAnimatorInt(AnimParam.SECONDARYSTATE, (int)secondaryAnim);
			}
		}
		base.transform.rotation = Quaternion.identity;
		RefreshScale();
		FeetPhysicsCollider.GetComponent<Collider2D>().enabled = true;
		Feet.raycastEnabled = true;
		Left.raycastEnabled = true;
		Right.raycastEnabled = true;
		Head.GetComponent<Collider2D>().enabled = true;
		Head.Colliding = false;
		Head.CollidingWall = false;
		Head.CollidingHazard = false;
		HazardHead.GetComponent<Collider2D>().enabled = true;
		HazardHead.Colliding = false;
		HazardHead.CollidingWall = false;
		HazardHead.CollidingHazard = false;
		Feet.Colliding = false;
		Left.Colliding = false;
		Right.Colliding = false;
		Left.CollidingWall = false;
		Right.CollidingWall = false;
		Feet.CollidingHazard = false;
		Left.CollidingHazard = false;
		Right.CollidingHazard = false;
		Feet.CollidingSpecialButton = false;
		Left.CollidingSpecialButton = false;
		Right.CollidingSpecialButton = false;
		CheckCollidingPlayer component = DeadCollider.GetComponent<CheckCollidingPlayer>();
		component.Hazard = false;
		component.HazardType = "";
		boostEffectTimer = 0f;
		pauseVel = Vector2.zero;
		ClearTemporaryIgnoreCollisions();
		chrRigidBody.velocity = Vector2.zero;
		RemoveAllPhysicsModifers();
		baseHorizontalMotion = 0f;
		baseVerticalMotion = 0f;
		characterInheritedMotion = Vector2.zero;
		isGhost = false;
		DestroyFlies();
		isZombie = false;
		zombieLocallyDead = false;
		waitingToTurnUndead = false;
		EnableZombieOutfit(enable: false);
		agonyStarted = false;
		agonyTimer = 0f;
		animator.speed = AnimatorSpeed;
		holdBToGiveUpInstance.InstantHide();
		if (Reverb)
		{
			Reverb = false;
		}
		else
		{
			AkSoundEngine.PostEvent("Out_Enclosed_Area", base.gameObject);
		}
		if (playSound)
		{
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_GameStart_Drop", base.gameObject);
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Dance_Stop", base.gameObject);
		}
		HoveredCursors.Clear();
		if (base.hasAuthority && !isExtraCorpse)
		{
			CallCmdEnable(playSound);
			if (extraCorpse != null)
			{
				CallCmdClearExtraCorpse();
			}
		}
	}

	public void CreateFlies()
	{
		for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
		{
			Fly fly = UnityEngine.Object.Instantiate(zombieFliePrefab, base.transform.position, Quaternion.identity);
			fly.Initialize(this);
			spawnedFlies.Add(fly);
		}
	}

	public void DestroyFlies()
	{
		foreach (Fly spawnedFly in spawnedFlies)
		{
			UnityEngine.Object.Destroy(spawnedFly.gameObject);
		}
		spawnedFlies.Clear();
	}

	[Command]
	private void CmdEnable(bool playsound)
	{
		CallRpcEnable(playsound);
	}

	[ClientRpc]
	private void RpcEnable(bool playsound)
	{
		if (!base.hasAuthority)
		{
			Enable(playsound);
		}
	}

	protected void ResetInput()
	{
		jumpDown = false;
		sprintDown = false;
		suicideDown = false;
		danceDown = false;
		jumpUp = false;
		sprintUp = false;
		suicideUp = false;
		danceUp = false;
		rotateLeftDown = false;
		rotateRightDown = false;
	}

	public void Disable(bool moveAway = true)
	{
		disabled = true;
		frozen = true;
		chrRigidBody.velocity = Vector2.zero;
		Visible = false;
		FeetPhysicsCollider.GetComponent<Collider2D>().enabled = false;
		Feet.raycastEnabled = false;
		Left.raycastEnabled = false;
		Right.raycastEnabled = false;
		Head.GetComponent<Collider2D>().enabled = false;
		HazardHead.GetComponent<Collider2D>().enabled = false;
		LowerBodyCollider.Disable();
		UpperBodyCollider.Disable();
		SetLobbyCollider(enable: false);
		CoinCanvas.SetActive(value: false);
		CoinNumberText.text = "0";
		SlowedIcon.SetActive(value: false);
		pickedUpJetpack = false;
		jetpackTouched = false;
		if (moveAway)
		{
			base.transform.position = new Vector2(-1000f, -1000f);
		}
		ArrowHitDebris[] componentsInChildren = GetComponentsInChildren<ArrowHitDebris>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		RemoveAllPhysicsModifers();
		SetJetpackEmitting(onOff: false);
		if (!CharacterSFXName.NullOrEmpty())
		{
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Dance_Stop", base.gameObject);
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Move_Stop", base.gameObject);
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Sprint_Stop", base.gameObject);
			AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_WallSlideStop", base.gameObject);
		}
		if (base.hasAuthority)
		{
			CallCmdDisable(moveAway);
			if (!isExtraCorpse && extraCorpse != null)
			{
				CallCmdClearExtraCorpse();
			}
		}
	}

	public void RemoveSuccess()
	{
		SetSuccess(s: false);
	}

	[Command]
	private void CmdDisable(bool moveAway)
	{
		CallRpcDisable(moveAway);
	}

	[ClientRpc]
	private void RpcDisable(bool moveAway)
	{
		if (!base.hasAuthority)
		{
			Disable(moveAway);
		}
	}

	[Command]
	private void CmdClearExtraCorpse()
	{
		if (extraCorpse != null)
		{
			UnityEngine.Object.Destroy(extraCorpse.gameObject);
			RpcClearExtraCorpse(extraCorpse.transform.position);
		}
	}

	private void RpcClearExtraCorpse(Vector3 smokePosition)
	{
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, smokePosition);
		extraCorpse = null;
	}

	public void Freeze(bool hide, bool freezeAnimator, bool disableColliders)
	{
		if (!disabled)
		{
			frozen = true;
			if (freezeAnimator)
			{
				animator.speed = 0f;
			}
			pauseVel = chrRigidBody.velocity;
			chrRigidBody.velocity = Vector2.zero;
			CallCmdFreeze();
			firstFrame = true;
			sprinting = false;
			walking = false;
			audioEvent("_Move_Stop", base.gameObject);
			audioEvent("_Sprint_Stop", base.gameObject);
			audioEvent("_Dance_Stop", base.gameObject);
			audioEvent("_WallSlideStop", base.gameObject);
			if (hide)
			{
				Visible = false;
				CallCmdShowSprite(show: false);
			}
			if (disableColliders)
			{
				Feet.raycastEnabled = false;
				FeetPhysicsCollider.GetComponent<Collider2D>().enabled = false;
				Left.raycastEnabled = false;
				Right.raycastEnabled = false;
				Head.GetComponent<Collider2D>().enabled = false;
				HazardHead.GetComponent<Collider2D>().enabled = false;
				LowerBodyCollider.Disable();
				UpperBodyCollider.Disable();
			}
		}
	}

	public void Unfreeze()
	{
		if (!disabled)
		{
			frozen = false;
			animator.speed = AnimatorSpeed;
			chrRigidBody.velocity = pauseVel;
			Visible = true;
			CallCmdShowSprite(show: true);
			if (!Dead || isGhost)
			{
				FeetPhysicsCollider.GetComponent<Collider2D>().enabled = true;
				Feet.raycastEnabled = true;
				Left.raycastEnabled = true;
				Right.raycastEnabled = true;
				Head.GetComponent<Collider2D>().enabled = true;
				HazardHead.GetComponent<Collider2D>().enabled = true;
				LowerBodyCollider.Enable();
				UpperBodyCollider.Enable();
			}
			else if (Dead && agonyStarted)
			{
				Feet.raycastEnabled = true;
			}
			if (dying)
			{
				DeadColliderSwitch(enabled: true);
			}
		}
	}

	public void UpperBodyEnable(bool onOff)
	{
		if (onOff)
		{
			UpperBodyCollider.Enable();
		}
		else
		{
			UpperBodyCollider.Disable();
		}
		UpperBodyTrigger.enabled = onOff;
	}

	public void LowerBodyEnable(bool onOff)
	{
		if (onOff)
		{
			LowerBodyCollider.Enable();
		}
		else
		{
			LowerBodyCollider.Disable();
		}
		LowerBodyTrigger.enabled = onOff;
	}

	[Command]
	private void CmdShowSprite(bool show)
	{
		CallRpcShowprite(show);
	}

	[ClientRpc]
	private void RpcShowprite(bool show)
	{
		if (!base.hasAuthority)
		{
			Visible = show;
		}
	}

	[Command]
	private void CmdFreeze()
	{
		CallRpcFreeze();
	}

	[ClientRpc]
	private void RpcFreeze()
	{
		if (!base.hasAuthority)
		{
			pauseVel = chrRigidBody.velocity;
			chrRigidBody.velocity = Vector2.zero;
		}
	}

	public void Pause(bool soft = false)
	{
		if (soft)
		{
			softPause = true;
			ResetInput();
		}
		else if (!disabled)
		{
			animator.speed = 0f;
			pauseVel = chrRigidBody.velocity;
			chrRigidBody.isKinematic = true;
			chrRigidBody.velocity = Vector2.zero;
			firstFrame = true;
		}
	}

	public void Unpause(bool soft = false)
	{
		if (soft)
		{
			softPause = false;
			ResetInput();
		}
		else if (!disabled && !paused && !scoreboard)
		{
			if (animator != null)
			{
				animator.speed = AnimatorSpeed;
			}
			chrRigidBody.isKinematic = false;
			chrRigidBody.velocity = pauseVel * 0.9f;
		}
	}

	public bool AddImpulse(Vector2 force, float cooldown = 0f, bool resetAirJumps = false)
	{
		if (impulseAdded > 0f)
		{
			return false;
		}
		chrRigidBody.AddForce(force, ForceMode2D.Impulse);
		impulseAdded = cooldown;
		jumping = true;
		forcedJump = true;
		timeSinceJump = 0f;
		PreJumpGraceTimer = 0f;
		if (resetAirJumps)
		{
			AirJumps = 0;
		}
		return true;
	}

	public void ResetPhysicsVariablesForCannon()
	{
		chrRigidBody.velocity = Vector2.zero;
		jumping = false;
		canJump = false;
		AirJumps = 0;
		PreJumpGraceTimer = 0f;
		gravityTransitionTime = 0f;
		timeSinceJump = 0f;
		onWall = false;
		NetworkonGround = false;
		wallStickTimer = 0f;
		loGravity = false;
		heightJumped = 0f;
		jumping = true;
		forcedJump = true;
		right = 0f;
		left = 0f;
		rightResponseTime = 0f;
		leftResponseTime = 0f;
		previousVX = 0f;
		previousVY = 0f;
		sprint = false;
	}

	public void ForceDeathVelocity(Vector2 vel)
	{
		deathPauseVel = vel;
	}

	public void ApplyPhysicsModifier(PhysicsModifier pm)
	{
		if (pm == null || paused || scoreboard || pm.Source == null || physicsModifiers.ContainsKey(pm.Source))
		{
			return;
		}
		PhysicsModifier.ModType modType = pm.ModifierType;
		switch (modType)
		{
		case PhysicsModifier.ModType.Friction:
		{
			bool flag = false;
			bool flag2 = false;
			foreach (PhysicsModifier value in physicsModifiers.Values)
			{
				if (value.ModifierType == PhysicsModifier.ModType.Friction)
				{
					return;
				}
				if (!flag && value.ModifierType == PhysicsModifier.ModType.WallFriction)
				{
					flag = true;
					switch (modType)
					{
					case PhysicsModifier.ModType.Friction:
						modType = PhysicsModifier.ModType.GroundFriction;
						break;
					case PhysicsModifier.ModType.WallFriction:
						return;
					}
				}
				if (!flag2 && value.ModifierType == PhysicsModifier.ModType.GroundFriction)
				{
					flag2 = true;
					switch (modType)
					{
					case PhysicsModifier.ModType.Friction:
						modType = PhysicsModifier.ModType.WallFriction;
						break;
					case PhysicsModifier.ModType.GroundFriction:
						return;
					}
				}
			}
			break;
		}
		case PhysicsModifier.ModType.WallFriction:
			foreach (PhysicsModifier value2 in physicsModifiers.Values)
			{
				if (value2.ModifierType == PhysicsModifier.ModType.Friction || value2.ModifierType == PhysicsModifier.ModType.WallFriction)
				{
					return;
				}
			}
			break;
		case PhysicsModifier.ModType.GroundFriction:
			foreach (PhysicsModifier value3 in physicsModifiers.Values)
			{
				if (value3.ModifierType == PhysicsModifier.ModType.Friction || value3.ModifierType == PhysicsModifier.ModType.GroundFriction)
				{
					return;
				}
			}
			break;
		case PhysicsModifier.ModType.BaseMotion:
		case PhysicsModifier.ModType.JumpForce:
			foreach (PhysicsModifier value4 in physicsModifiers.Values)
			{
				if (value4.ModifierType == modType)
				{
					return;
				}
			}
			break;
		case PhysicsModifier.ModType.Treadmill:
			foreach (PhysicsModifier value5 in physicsModifiers.Values)
			{
				if (value5.ModifierType != modType)
				{
					continue;
				}
				if (Mathf.Sign(pm.Direction.x) != Mathf.Sign(value5.Direction.x))
				{
					float num = Mathf.Abs(pm.Source.transform.position.x - base.transform.position.x);
					if (Mathf.Abs(value5.Source.transform.position.x - base.transform.position.x) > num)
					{
						RemovePhysicsModifier(value5);
						break;
					}
				}
				return;
			}
			break;
		}
		switch (modType)
		{
		case PhysicsModifier.ModType.AirInertia:
			airInertiaModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.AirInertiaLeft:
			airInertiaModifierL += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.AirInertiaRight:
			airInertiaModifierR += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.BaseMotion:
		{
			Vector2 vector5 = pm.Magnitude * pm.Direction;
			baseHorizontalMotion += vector5.x;
			baseVerticalMotion += vector5.y;
			break;
		}
		case PhysicsModifier.ModType.Friction:
			groundFrictionModifier += pm.Magnitude;
			wallFrictionModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.GroundFriction:
			groundFrictionModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.JumpForce:
			jumpForceModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.WallFriction:
			wallFrictionModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.Gravity:
			gravityModifier += pm.Magnitude;
			break;
		case PhysicsModifier.ModType.Rotation:
		{
			Vector3 lhs2 = new Vector3(0f, 0f, pm.Magnitude * (MathF.PI / 180f));
			Vector3 rhs2 = base.transform.position - (Vector3)pm.Direction;
			Vector3 vector4 = Vector3.Cross(lhs2, rhs2);
			pm = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, vector4.magnitude, vector4.normalized, pm.Source);
			modType = PhysicsModifier.ModType.BaseMotion;
			baseHorizontalMotion += vector4.x;
			baseVerticalMotion += vector4.y;
			break;
		}
		case PhysicsModifier.ModType.Blackhole:
		{
			Vector2 vector3 = pm.Magnitude * pm.Direction;
			blackHoleX += vector3.x;
			blackholeY += vector3.y;
			break;
		}
		case PhysicsModifier.ModType.Wind:
		{
			Vector2 vector2 = pm.Magnitude * pm.Direction;
			windX += vector2.x;
			windY += vector2.y;
			break;
		}
		case PhysicsModifier.ModType.Treadmill:
			if (pm.Mode == 0)
			{
				baseHorizontalMotion += pm.Direction.x * pm.Magnitude;
			}
			else if (pm.Mode == 1)
			{
				Vector3 lhs = new Vector3(0f, 0f, pm.Magnitude * (MathF.PI / 180f));
				Vector3 rhs = base.transform.position - (Vector3)pm.Direction;
				Vector3 vector = Vector3.Cross(lhs, rhs);
				pm = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, vector.magnitude, vector.normalized, pm.Source);
				pm.Mode = 1;
				modType = PhysicsModifier.ModType.BaseMotion;
				baseHorizontalMotion += vector.x;
				baseVerticalMotion += vector.y;
			}
			break;
		}
		PhysicsModifier physicsModifier = new PhysicsModifier(pm);
		physicsModifier.ModifierType = modType;
		physicsModifiers.Add(pm.Source, physicsModifier);
	}

	public void RemovePhysicsModifier(PhysicsModifier pm, bool removedFromDict = true)
	{
		if (pm == null)
		{
			return;
		}
		switch (pm.ModifierType)
		{
		case PhysicsModifier.ModType.AirInertia:
			airInertiaModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.AirInertiaLeft:
			airInertiaModifierL -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.AirInertiaRight:
			airInertiaModifierR -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.BaseMotion:
		{
			Vector2 vector5 = pm.Magnitude * pm.Direction;
			baseHorizontalMotion -= vector5.x;
			baseVerticalMotion -= vector5.y;
			break;
		}
		case PhysicsModifier.ModType.Friction:
			groundFrictionModifier -= pm.Magnitude;
			wallFrictionModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.GroundFriction:
			groundFrictionModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.JumpForce:
			jumpForceModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.WallFriction:
			wallFrictionModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.Gravity:
			gravityModifier -= pm.Magnitude;
			break;
		case PhysicsModifier.ModType.Rotation:
		{
			Vector3 lhs2 = new Vector3(0f, 0f, pm.Magnitude);
			Vector3 rhs2 = (Vector3)pm.Direction - base.transform.position;
			Vector3 vector4 = Vector3.Cross(lhs2, rhs2);
			baseHorizontalMotion -= vector4.x;
			baseVerticalMotion -= vector4.y;
			break;
		}
		case PhysicsModifier.ModType.Blackhole:
		{
			Vector2 vector3 = pm.Magnitude * pm.Direction;
			blackHoleX -= vector3.x;
			blackholeY -= vector3.y;
			break;
		}
		case PhysicsModifier.ModType.Wind:
		{
			Vector2 vector2 = pm.Magnitude * pm.Direction;
			windX -= vector2.x;
			windY -= vector2.y;
			break;
		}
		case PhysicsModifier.ModType.Treadmill:
			if (pm.Mode == 0)
			{
				baseHorizontalMotion -= pm.Direction.x * pm.Magnitude;
			}
			else if (pm.Mode == 1)
			{
				Vector3 lhs = new Vector3(0f, 0f, pm.Magnitude);
				Vector3 rhs = (Vector3)pm.Direction - base.transform.position;
				Vector3 vector = Vector3.Cross(lhs, rhs);
				baseHorizontalMotion -= vector.x;
				baseVerticalMotion -= vector.y;
			}
			break;
		}
		if (removedFromDict)
		{
			physicsModifiers.Remove(pm.Source);
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (softPause || tempLockInput || Controller.FullScreenComputerIsActive)
		{
			return;
		}
		Modifiers instance = Modifiers.GetInstance();
		bool flag = instance.MirrorControls ^ (instance.CameraFlipping == Modifiers.CameraFlipModes.FlipX || instance.CameraFlipping == Modifiers.CameraFlipModes.FlipXY);
		bool flag2 = instance.CameraFlipping == Modifiers.CameraFlipModes.FlipY || instance.CameraFlipping == Modifiers.CameraFlipModes.FlipXY;
		bool flag3 = true;
		switch (e.Key)
		{
		case InputEvent.InputKey.Up:
			if (flag2)
			{
				down = e.Valuef;
			}
			else
			{
				up = e.Valuef;
			}
			break;
		case InputEvent.InputKey.Down:
			if (flag2)
			{
				up = e.Valuef;
			}
			else
			{
				down = e.Valuef;
			}
			break;
		case InputEvent.InputKey.Left:
			if (flag)
			{
				if (e.Valuef > 0.1f && rightInput < 0.1f)
				{
					rightResponseTime = 0f;
				}
				rightInput = e.Valuef;
				right = rightInput;
			}
			else
			{
				if (e.Valuef > 0.1f && leftInput < 0.1f)
				{
					leftResponseTime = 0f;
				}
				leftInput = e.Valuef;
				left = leftInput;
			}
			break;
		case InputEvent.InputKey.Right:
			if (flag)
			{
				if (e.Valuef > 0.1f && leftInput < 0.1f)
				{
					leftResponseTime = 0f;
				}
				leftInput = e.Valuef;
				left = e.Valuef;
			}
			else
			{
				if (e.Valuef > 0.1f && rightInput < 0.1f)
				{
					rightResponseTime = 0f;
				}
				rightInput = e.Valuef;
				right = e.Valuef;
			}
			break;
		case InputEvent.InputKey.Up2:
			up2 = e.Valuef;
			break;
		case InputEvent.InputKey.Down2:
			down2 = e.Valuef;
			break;
		case InputEvent.InputKey.Left2:
			left2 = e.Valuef;
			break;
		case InputEvent.InputKey.Right2:
			right2 = e.Valuef;
			break;
		case InputEvent.InputKey.Accept:
			if (e.Changed && e.Valueb && (bool)standingAtProp)
			{
				standingAtProp.Use(AssociatedLobbyPlayer, e.Key);
			}
			break;
		case InputEvent.InputKey.Jump:
			jump = e.Valueb;
			if (e.Changed)
			{
				if (jump)
				{
					if (lastJumpDownTimer > 0.1f)
					{
						lastJumpDownTimer = 0f;
						jumpDown = true;
					}
				}
				else
				{
					jumpUp = true;
				}
			}
			else
			{
				jumpUp = false;
			}
			break;
		case InputEvent.InputKey.Sprint:
			sprint = e.Valueb;
			if (e.Changed)
			{
				if (sprint)
				{
					sprintDown = true;
				}
				else
				{
					sprintUp = true;
				}
			}
			break;
		case InputEvent.InputKey.RightTrigger:
			sprint = e.Valueb;
			if (e.Changed)
			{
				if (sprint)
				{
					sprintDown = true;
				}
				else
				{
					sprintUp = true;
				}
			}
			break;
		case InputEvent.InputKey.LeftTrigger:
			leftTrigger = e.Valueb;
			break;
		case InputEvent.InputKey.Suicide:
			if (InMenu)
			{
				suicide = false;
				break;
			}
			suicide = e.Valueb;
			if (e.Changed)
			{
				if (suicide)
				{
					suicideDown = true;
					break;
				}
				suicideUp = true;
				backDownOnEnable = false;
			}
			break;
		case InputEvent.InputKey.Inventory:
			dance = e.Valueb;
			if (e.Changed)
			{
				if (dance)
				{
					danceDown = true;
				}
				else
				{
					danceUp = true;
				}
			}
			break;
		case InputEvent.InputKey.RotateLeft:
			rotateLeft = e.Valueb;
			if (e.Valueb && e.Changed)
			{
				rotateLeftDown = true;
				checkCameraToggle();
				cameraChangeTimeout = 0.1f;
			}
			break;
		case InputEvent.InputKey.RotateRight:
			rotateRight = e.Valueb;
			if (e.Valueb && e.Changed)
			{
				rotateRightDown = true;
				checkCameraToggle();
				cameraChangeTimeout = 0.1f;
			}
			break;
		default:
			flag3 = false;
			break;
		}
		if (flag3)
		{
			idleTimer = 0f;
		}
	}

	private void checkCameraToggle()
	{
		if (Enabled && !Frozen && LobbyManager.instance != null && !LobbyManager.instance.AllLocal && ((rotateLeftDown && rotateRightDown) || (rotateLeft && rotateRight && cameraChangeTimeout > 0f)) && ZoomCamera.CurrentZoomCamera != null)
		{
			ZoomCamera.CurrentZoomCamera.GetComponent<ZoomCamera>().ToggleLocalOnly();
		}
	}

	public void Tint()
	{
		if (GameSettings.GetInstance() == null)
		{
			return;
		}
		if (!Picked)
		{
			if (HoveredCursors.Count == 1)
			{
				sprite.color = GameSettings.GetInstance().CharacterHighlightColor;
			}
			else
			{
				sprite.color = GameSettings.GetInstance().neutralColor;
			}
		}
		else if (HoveredCursors.Count > 0)
		{
			sprite.color = GameSettings.GetInstance().CharacterNegativeColor;
		}
		else
		{
			sprite.color = GameSettings.GetInstance().neutralColor;
		}
		if (isZombie)
		{
			sprite.color = CharacterSpriteManager.GetInstance().GetZombieColour(CharacterSprite);
		}
		sprite.SetAlpha(opacityController.currentOpacity);
		if (HasJetpack)
		{
			JetPackSR.SetAlpha(opacityController.currentOpacity);
		}
		nameTag.maxOpacity = opacityController.currentOpacity;
		holdBToGiveUpInstance.maxOpacity = opacityController.currentOpacity;
	}

	public bool SkipDeath()
	{
		if (dying && deathTimer > minSkipDeathTime)
		{
			if (base.hasAuthority)
			{
				CallCmdSetDead(d: true);
			}
			return true;
		}
		return false;
	}

	public void StartInvincibleTimer(float time)
	{
		if (time > invincibleTimer)
		{
			invincibleTimer = time;
		}
	}

	public void SetUseableProp(UsableProp prop)
	{
		if (standingAtProp != null && standingAtProp.hoverCharacters.Contains(this))
		{
			standingAtProp.hoverCharacters.Remove(this);
		}
		standingAtProp = prop;
	}

	public void SetOutfitsFromArray(SyncListInt outfitsSyncList)
	{
		int[] array = new int[Outfit.NumOutfitTypes];
		for (int i = 0; i < Outfit.NumOutfitTypes; i++)
		{
			if (outfitsSyncList.Count > i)
			{
				array[i] = outfitsSyncList[i];
			}
			else
			{
				array[i] = -1;
			}
		}
		SetOutfitsFromArray(array);
	}

	public void SetOutfitsFromArray(int[] outfitsArray)
	{
		Outfit[] componentsInChildren = GetComponentsInChildren<Outfit>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			Outfit outfit = componentsInChildren[i];
			if (outfit.outfitType != Outfit.OutfitType.FollowOutfit && outfit.outfitType != Outfit.OutfitType.Zombie)
			{
				int outfitType = (int)outfit.outfitType;
				outfit.on = outfitsArray[outfitType] != -1 && i == outfitsArray[outfitType];
				if (outfit.on && !outfit.Unlocked)
				{
					outfit.TempUnlocked = true;
				}
			}
		}
		OnOutfitsUpdated(componentsInChildren);
		if (base.hasAuthority)
		{
			TellEverybodyAboutOutfits();
		}
	}

	public int GetOutfits()
	{
		int num = 0;
		Outfit[] componentsInChildren = GetComponentsInChildren<Outfit>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			Outfit outfit = componentsInChildren[i];
			if (outfit.on)
			{
				num |= outfit.OutfitMaskNumber;
			}
		}
		return num;
	}

	public int[] GetOutfitsAsArray()
	{
		int[] array = new int[6] { -1, -1, -1, -1, -1, -1 };
		Outfit[] componentsInChildren = GetComponentsInChildren<Outfit>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			Outfit outfit = componentsInChildren[i];
			if (!(outfit.followThisOutfit != null) && outfit.on)
			{
				array[(int)outfit.outfitType] = i;
			}
		}
		return array;
	}

	public void OnOutfitsUpdated(Outfit[] allOutfits = null)
	{
		if (allOutfits == null)
		{
			allOutfits = GetComponentsInChildren<Outfit>();
		}
		bodyHidden = false;
		bool flag = false;
		CustomItemSoundFX = false;
		for (int i = 0; i != allOutfits.Length; i++)
		{
			if (allOutfits[i].on)
			{
				if (allOutfits[i].hidesAnimalBody)
				{
					bodyHidden = true;
				}
				if (allOutfits[i].outfitType == Outfit.OutfitType.Skin)
				{
					flag = true;
				}
				if (allOutfits[i].specialCharacterSoundFx)
				{
					CustomItemSoundFX = true;
				}
			}
		}
		if (!Picked || !(LobbyManager.instance.CurrentLevelSelectController != null))
		{
			return;
		}
		int num = networkNumber;
		if (AssociatedLobbyPlayer != null)
		{
			num = AssociatedLobbyPlayer.networkNumber;
		}
		if (num > 0)
		{
			playerJoinIndicator playerJoinIndicator2 = LobbyManager.instance.CurrentLevelSelectController.PlayerJoinIndicators[num - 1];
			if (playerJoinIndicator2.altSkin != flag)
			{
				playerJoinIndicator2.setAnimalName(CharacterSprite, flag);
			}
		}
		else
		{
			Debug.LogError("Invalid network number");
		}
	}

	public void EnableZombieOutfit(bool enable)
	{
		if (OutfitArt == null)
		{
			return;
		}
		Outfit[] outfits = OutfitArt.outfits;
		bool flag = false;
		for (int i = 0; i != outfits.Length; i++)
		{
			Outfit outfit = outfits[i];
			if (outfit.outfitType == Outfit.OutfitType.Skin && outfit.on)
			{
				flag = true;
			}
		}
		bool flag2 = false;
		if (flag)
		{
			for (int j = 0; j != outfits.Length; j++)
			{
				Outfit outfit2 = outfits[j];
				if (outfit2.outfitType == Outfit.OutfitType.Zombie && outfit2.ZombieEyesForSkinX)
				{
					outfit2.TempUnlocked = true;
					outfit2.on = enable;
					flag2 = true;
				}
			}
		}
		if (!flag2)
		{
			for (int k = 0; k != outfits.Length; k++)
			{
				Outfit outfit3 = outfits[k];
				if (outfit3.outfitType == Outfit.OutfitType.Zombie && !outfit3.ZombieEyesForSkinX)
				{
					outfit3.TempUnlocked = true;
					outfit3.on = enable;
				}
			}
		}
		if (base.hasAuthority)
		{
			TellEverybodyAboutOutfits();
		}
	}

	public void TellEverybodyAboutOutfits()
	{
		CallCmdCommunicateOutfitsArray(GetOutfitsAsArray());
	}

	[Command]
	private void CmdCommunicateOutfitsArray(int[] outfitsArray)
	{
		CallRpcCommunicateOutfitsArray(outfitsArray);
	}

	[ClientRpc]
	private void RpcCommunicateOutfitsArray(int[] outfitsArray)
	{
		if (!base.hasAuthority)
		{
			SetOutfitsFromArray(outfitsArray);
		}
	}

	public UsableProp GetUseableProp()
	{
		return standingAtProp;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.END)
			{
				HideAFKWarning();
				ignoreAFK = true;
			}
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				opacityController.OnStartPlayPhase();
			}
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
			}
			else
			{
				scoreboard = false;
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				paused = true;
				Pause();
			}
			else
			{
				paused = false;
				Unpause();
			}
		}
		if (type == typeof(SoftPauseEvent))
		{
			SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
			if (softPauseEvent.PlayerNumber == networkNumber)
			{
				if (softPauseEvent.SoftPaused)
				{
					Pause(soft: true);
				}
				else
				{
					Unpause(soft: true);
				}
			}
		}
		if (type == typeof(PlayerInGameRuleEvent))
		{
			PlayerInGameRuleEvent playerInGameRuleEvent = e as PlayerInGameRuleEvent;
			if (playerInGameRuleEvent.PlayerNumber == networkNumber)
			{
				if (playerInGameRuleEvent.Entered)
				{
					if (!InMenu)
					{
						InMenu = true;
						Freeze(hide: false, freezeAnimator: false, disableColliders: false);
					}
				}
				else if (InMenu)
				{
					InMenu = false;
					Unfreeze();
				}
			}
		}
		if (type == typeof(NetworkMessageReceivedEvent) && (e as NetworkMessageReceivedEvent).Message.msgType == NetMsgTypes.NetworkClientConnected && base.hasAuthority)
		{
			StartCoroutine(WaitToBroadcastOutfits());
		}
		if (type == typeof(HoldRespawnEvent))
		{
			HoldRespawnEvent holdRespawnEvent = e as HoldRespawnEvent;
			holdingRespawns = holdRespawnEvent.Hold;
			UpdateHoldBIndicator();
		}
	}

	private IEnumerator WaitToBroadcastOutfits()
	{
		bool proceed = false;
		int framesWaited = 0;
		while (!proceed)
		{
			proceed = true;
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (lobbyPlayer != null && lobbyPlayer.PlayerStatus == LobbyPlayer.Status.INACTIVE)
				{
					proceed = false;
					framesWaited++;
					break;
				}
			}
			yield return null;
		}
		TellEverybodyAboutOutfits();
	}

	private void setupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)
	{
		Debug.Log("Setup player death caused by : " + cause);
		SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true);
		if (saveFileDataForLocalPlayer != null)
		{
			if (cause.Equals("World") || cause.Equals("Falling") || cause.Equals("Drowning") || cause.Equals("Drowning_In_Lava"))
			{
				saveFileDataForLocalPlayer.IncrementStat("DeathsByHazard");
			}
			else
			{
				saveFileDataForLocalPlayer.IncrementStat("DeathsByTrap");
				if (cause.Equals("SpikeBall"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsBySpikeBall");
				}
				if (cause.Equals("BarbedWire"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByBarbedWire");
				}
				if (cause.Equals("Crossbow"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByArrow");
				}
				if (cause.Equals("TennisBallLauncher"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByTennisBall");
				}
				if (cause.Equals("SpinningDeath"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsBySpinningSaw");
				}
				if (cause.Equals("LinearSaw"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByLinearSaw");
				}
				if (cause.Equals("FloatingPlatform"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByPropeller");
				}
				if (cause.Equals("FlippingBlock"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByFlippingBlock");
				}
				if (cause.Equals("Blackhole"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByBlackHole");
				}
				if (cause.Equals("HockeyShooter"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByHockeyPuck");
				}
				if (cause.Equals("PunchingPlant"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByPunchingPlant");
				}
				if (cause.Equals("PressureTriggerSpikes"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByPressureTriggerSpikes");
				}
				if (cause.Equals("WreckingBall"))
				{
					saveFileDataForLocalPlayer.IncrementStat("DeathsByWreckingBall");
				}
			}
		}
		if (causedByPlayerNumber != networkNumber && base.hasAuthority)
		{
			ScoreKeeper.Instance.AwardPoint(new PointBlock(PointBlock.pointBlockType.trap, causedByPlayerNumber));
		}
		switch (cause)
		{
		case "Falling":
			saveFileDataForLocalPlayer.IncrementStat("DeathsByFalling");
			audioEvent("_Falling_To_Death", base.gameObject);
			break;
		case "Drowning":
			saveFileDataForLocalPlayer.IncrementStat("DeathsByFalling");
			audioEvent("_Drown_To_Death", base.gameObject);
			break;
		case "Drowning_In_Lava":
			saveFileDataForLocalPlayer.IncrementStat("DeathsByFalling");
			audioEvent("_Drown_In_Lava", base.gameObject, ignoreGhostZombie: true);
			break;
		default:
			if (impulseAdded == 0f)
			{
				Vector3 vector = (LowerBodyCollider.Hazard ? LowerBodyCollider.HazardPoint : UpperBodyCollider.HazardPoint);
				Vector2 direction = base.transform.position - vector;
				direction.y = Mathf.Max(direction.y, 0f);
				direction.Normalize();
				ApplyPhysicsModifier(new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, DEATHFORCE, direction, base.gameObject));
			}
			if (cause != "UFO")
			{
				audioEvent("_Hurt", base.gameObject);
			}
			break;
		}
		LastDeath = cause;
		audioEvent("_Move_Stop", base.gameObject);
		audioEvent("_Sprint_Stop", base.gameObject);
		audioEvent("_Dance_Stop", base.gameObject);
		audioEvent("_WallSlideStop", base.gameObject);
		AnimatorTrigger(AnimParam.DEATHTRIGGER);
		switch (cause)
		{
		case "Falling":
		case "Drowning":
		case "Drowning_In_Lava":
			deathTimer = fallingDeathSpeedUpAmount;
			diedInPit = true;
			break;
		default:
			deathTimer = 0f;
			diedInPit = false;
			break;
		}
		if (suicide && GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE)
		{
			backDownOnEnable = true;
		}
		DeadColliderSwitch(enabled: true, resetPosition: true);
		loGravity = true;
		if (deathFreezeOn && impulseAdded < 0.01f)
		{
			deathFrozen = true;
			deathPauseVel = new Vector2(chrRigidBody.velocity.x, chrRigidBody.velocity.y);
			chrRigidBody.velocity = Vector2.zero;
		}
		if (saveFileDataForLocalPlayer != null)
		{
			saveFileDataForLocalPlayer.IncrementStat("CharacterDeaths", (int)CharacterSprite);
			saveFileDataForLocalPlayer.IncrementStat("TotalDeaths");
			AchievementChecker.Instance.Death_AchievementChecks(saveFileDataForLocalPlayer);
		}
		if (base.hasAuthority)
		{
			Networkdying = true;
			CallCmdSetDying(d: true);
			CallCmdSetupDeath(cause, deathFreezeOn, causedByPlayerNumber);
		}
		if (base.hasAuthority)
		{
			GameEventManager.SendEvent(new PlayerKilledEvent(associatedGamePlayer.LocalPlayer, cause));
		}
		HideAFKWarning();
	}

	[Command]
	private void CmdSetupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)
	{
		CallRpcSetupDeath(cause, deathFreezeOn, causedByPlayerNumber);
	}

	[ClientRpc]
	private void RpcSetupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)
	{
		if (!base.hasAuthority)
		{
			setupDeath(cause, deathFreezeOn, causedByPlayerNumber);
		}
	}

	private void OnDestroy()
	{
		AllCharacters.Remove(this);
		ChangeListener(adding: false);
		AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Move_Stop", base.gameObject);
		AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Sprint_Stop", base.gameObject);
		AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_Dance_Stop", base.gameObject);
		AkSoundEngine.PostEvent("SFX_" + CharacterSFXName + "_WallSlideStop", base.gameObject);
		SetJetpackEmitting(onOff: false);
	}

	public void SetSprites(Animals newAnimal)
	{
		NetworkCharacterSprite = newAnimal;
		CharacterSpriteLibrary characterSprites = CharacterSpriteManager.GetInstance().GetCharacterSprites(newAnimal);
		animator.runtimeAnimatorController = characterSprites.CharacterSpriteOverride;
		spectatorAnimator.runtimeAnimatorController = characterSprites.SpectatorSpriteOverride;
		sprite.sortingOrder = (int)newAnimal * 10;
		JetPackSR.sortingOrder = (int)newAnimal * 10 - 5;
		if (nameTag != null)
		{
			nameTag.MatchLayerOrder(sprite);
		}
		JetpackParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = sprite.sortingOrder;
		GameObject[] allArtmatchers = AllArtmatchers;
		foreach (GameObject gameObject in allArtmatchers)
		{
			if (gameObject.GetComponent<ArtMatcher>().CharacterSprite == newAnimal)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, base.transform.position, Quaternion.identity);
				OutfitArt = gameObject2.GetComponent<ArtMatcher>();
				OutfitArt.AttachArtMatcher(this);
				break;
			}
		}
	}

	[Command]
	public void CmdSetLocalPlayerID(int localNumber)
	{
		NetworklocalNumber = localNumber;
		CallRpcFindLocalPlayer(localNumber);
	}

	[ClientRpc]
	public void RpcFindLocalPlayer(int localNumber)
	{
		if (base.hasAuthority)
		{
			LocalPlayer = ((localNumber != 0) ? PlayerManager.GetInstance().GetPlayer(localNumber) : null);
			if (LocalPlayer != null)
			{
				LocalPlayer.PlayerCharacter = this;
			}
			if (localNumber > 0)
			{
				sprite.sortingOrder = (int)(CharacterSprite + Enum.GetValues(typeof(Animals)).Length) * 10;
				JetPackSR.sortingOrder = (int)(CharacterSprite + Enum.GetValues(typeof(Animals)).Length) * 10 - 5;
			}
			else
			{
				sprite.sortingOrder = (int)CharacterSprite * 10;
				JetPackSR.sortingOrder = (int)CharacterSprite * 10 - 5;
			}
			JetpackParticles.GetComponent<ParticleSystemRenderer>().sortingOrder = sprite.sortingOrder;
		}
	}

	[Command]
	public void CmdGetLocalController(int localNumber)
	{
		Debug.Log("Finding local controller " + localNumber);
		CallRpcGetLocalController(localNumber);
	}

	[ClientRpc]
	public void RpcGetLocalController(int localNumber)
	{
		if (base.hasAuthority && (localNumber != 0 || LocalPlayer != null))
		{
			Player player = ((LocalPlayer != null) ? LocalPlayer : PlayerManager.GetInstance().GetPlayer(localNumber));
			if (player != null)
			{
				Debug.Log(base.name + " Assigned controller: " + player.UseController);
				player.UseController.AddReceiver(this);
				SetLocalController(player.UseController);
			}
		}
	}

	[Command]
	private void CmdSetPlayerColor(Color c)
	{
		NetworkplayerColor = c;
	}

	private void SetSuccess(bool s)
	{
		Networksuccess = s;
		if (base.hasAuthority)
		{
			CallCmdSetSuccess(s);
		}
	}

	[Command]
	private void CmdSetSuccess(bool s)
	{
		Networksuccess = s;
		if (LastDeath == "" && s)
		{
			LastDeath = "Won";
		}
		if (s)
		{
			CallRpcSendPlayerSuccessEvent();
		}
	}

	[ClientRpc]
	private void RpcSendPlayerSuccessEvent()
	{
		GameEventManager.SendEvent(new PlayerSucceedEvent(this));
	}

	private void SetDying(bool d)
	{
		Networkdying = d;
		if (base.hasAuthority)
		{
			CallCmdSetDying(d);
		}
	}

	[Command]
	private void CmdSetDying(bool d)
	{
		Networkdying = d;
	}

	private void SetDead(bool d)
	{
		Networkdead = d;
		if (base.hasAuthority)
		{
			CallCmdSetDead(d);
		}
	}

	[Command]
	private void CmdSetDead(bool d)
	{
		Networkdead = d;
	}

	private bool AttemptRespawn()
	{
		if (AssociatedGamePlayer.lives > 0)
		{
			SetupClientRespawn();
			CallCmdRespawn();
			return true;
		}
		return false;
	}

	private void SetLocalDeadCmdDeadAvoidRespawn()
	{
		CallCmdSetDead(d: true);
		LocallyDead = true;
	}

	private void SetLocalDeadCmdDead()
	{
		switch (Modifiers.GetInstance().PostDeathBehavior)
		{
		case Modifiers.PostDeathBehaviors.None:
		case Modifiers.PostDeathBehaviors.Agony:
			if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE || GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY || GameSettings.GetInstance().respawnMode != RespawnMode.LivesPerRound || !AttemptRespawn())
			{
				CallCmdSetDead(d: true);
				LocallyDead = true;
			}
			break;
		case Modifiers.PostDeathBehaviors.Zombie:
			if (!isZombie && !succeeding && !diedInPit && !WantsToRetry)
			{
				TriggerZombification();
			}
			else if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE || GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY || GameSettings.GetInstance().respawnMode != RespawnMode.LivesPerRound || !AttemptRespawn())
			{
				CallCmdSetDead(d: true);
				LocallyDead = true;
				zombieLocallyDead = true;
				isGhost = false;
			}
			break;
		case Modifiers.PostDeathBehaviors.Ghost:
			if (!isGhost)
			{
				if (diedInPit)
				{
					if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE || GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY || GameSettings.GetInstance().respawnMode != RespawnMode.LivesPerRound || !AttemptRespawn())
					{
						CallCmdSetDead(d: true);
						LocallyDead = true;
						isGhost = false;
					}
				}
				else if (!LocallyDead && (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE || GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY || GameSettings.GetInstance().respawnMode != RespawnMode.LivesPerRound || !AttemptRespawn()))
				{
					TriggerSoulExtraction();
				}
			}
			else if (!WantsToRetry)
			{
				CallCmdSetDead(d: true);
				LocallyDead = true;
				isGhost = false;
			}
			break;
		}
	}

	public void SetLobbyCollider(bool enable)
	{
		LobbyCollider.enabled = enable;
		if (base.hasAuthority)
		{
			CallCmdSetLobbyCollider(enable);
		}
	}

	[Command]
	private void CmdSetLobbyCollider(bool enable)
	{
		CallRpcSetLobbyCollider(enable);
	}

	[ClientRpc]
	private void RpcSetLobbyCollider(bool enable)
	{
		if (!base.hasAuthority)
		{
			SetLobbyCollider(enable);
		}
	}

	[Command]
	public void CmdSetPicked(bool picked)
	{
		Networkpicked = picked;
		CallRpcSetRaycastsEnabled(picked);
	}

	public void SetPickedImmediate(bool picked)
	{
		Networkpicked = picked;
		audioEvent("_Move_Stop", base.gameObject);
		audioEvent("_Sprint_Stop", base.gameObject);
		audioEvent("_Dance_Stop", base.gameObject);
		audioEvent("_WallSlideStop", base.gameObject);
	}

	[ClientRpc]
	private void RpcSetRaycastsEnabled(bool enabled)
	{
		setRaycastsEnabled(enabled);
	}

	private void setRaycastsEnabled(bool enabled)
	{
		Left.raycastEnabled = enabled;
		Right.raycastEnabled = enabled;
		Feet.raycastEnabled = enabled;
	}

	[ClientRpc]
	public void RpcSetReady(bool ready)
	{
		Ready = ready;
	}

	[Command]
	private void CmdSwitchFreeMode()
	{
		CallRpcSwitchFreeMode();
	}

	[ClientRpc]
	private void RpcSwitchFreeMode()
	{
		if (!base.hasAuthority)
		{
			GameEventManager.SendEvent(new FreePlayPlayerSwitchEvent(networkNumber, GameControl.GamePhase.PLACE));
		}
	}

	private void AnimatorTrigger(AnimParam triggerName)
	{
		animator.SetTrigger(AnimToString(triggerName));
		if (base.hasAuthority)
		{
			CallCmdAnimatorTrigger(triggerName);
		}
	}

	[Command(channel = 1)]
	private void CmdAnimatorTrigger(AnimParam triggerName)
	{
		CallRpcAnimatorTrigger(triggerName);
	}

	[ClientRpc(channel = 1)]
	private void RpcAnimatorTrigger(AnimParam triggerName)
	{
		if (!base.hasAuthority)
		{
			if (triggerName == AnimParam.DEATHTRIGGER)
			{
				AnimatorTrigger(AnimParam.DEATHTRIGGERFORCE);
			}
			else
			{
				AnimatorTrigger(triggerName);
			}
		}
	}

	private void SetAnimatorInt(AnimParam paramName, int value)
	{
		if (animator.GetInteger(AnimToString(paramName)) != value)
		{
			animator.SetInteger(AnimToString(paramName), value);
			if (base.hasAuthority)
			{
				setAnimatorIntLocal(paramName, value);
				CallCmdSetAnimatorInt(paramName, value);
			}
		}
	}

	private void setAnimatorIntLocal(AnimParam paramName, int value)
	{
		switch (paramName)
		{
		case AnimParam.STATE:
			NetworkcurrentAnim = (AnimState)value;
			break;
		case AnimParam.SECONDARYSTATE:
			NetworksecondaryAnim = (SecondaryAnimState)value;
			break;
		}
	}

	[Command(channel = 1)]
	private void CmdSetAnimatorInt(AnimParam paramName, int value)
	{
		setAnimatorIntLocal(paramName, value);
	}

	[ClientRpc(channel = 1)]
	private void RpcSetAnimatorInt(AnimParam paramName, int value)
	{
		if (!base.hasAuthority)
		{
			SetAnimatorInt(paramName, value);
		}
	}

	private void SetBubble(int animId, bool value, Color color)
	{
		if (InvincibilityAnimator.GetBool(animId) != value)
		{
			InvincibilityAnimator.SetBool(animId, value);
			InvincibilityBubbleSR.color = new Color(color.r, color.g, color.b, InvincibilityBubbleSR.color.a);
			if (base.hasAuthority)
			{
				CallCmdSetBubble(animId, value, color);
			}
		}
	}

	[Command(channel = 1)]
	private void CmdSetBubble(int animId, bool value, Color color)
	{
		CallRpcSetBubble(animId, value, color);
	}

	[ClientRpc(channel = 1)]
	private void RpcSetBubble(int animId, bool value, Color color)
	{
		if (!base.hasAuthority)
		{
			SetBubble(animId, value, color);
		}
	}

	private string GetCachedAudioEventName(string sfxName, string audioEventName, bool isZomb, bool isGhst, bool ignoreState)
	{
		(string, string, bool, bool, bool) key = (sfxName, audioEventName, isZomb, isGhst, ignoreState);
		if (!cachedAudioStrings.TryGetValue(key, out var value))
		{
			string text = "";
			if (isZomb && !ignoreState)
			{
				text = "_Zombie";
			}
			else if (isGhst && !ignoreState)
			{
				text = "_Ghost";
			}
			value = "SFX_" + sfxName + text + audioEventName;
			cachedAudioStrings[key] = value;
		}
		return value;
	}

	public void audioEvent(string audioEventName, GameObject go, bool ignoreGhostZombie = false)
	{
		if (!CharacterSFXName.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(GetCachedAudioEventName(CharacterSFXName, audioEventName, isZombie, isGhost, ignoreGhostZombie), go);
			if (base.hasAuthority)
			{
				CallCmdAudioEvent(audioEventName, ignoreGhostZombie);
			}
		}
	}

	[Command(channel = 1)]
	private void CmdAudioEvent(string audioEventName, bool ignoreGhostZombie)
	{
		CallRpcAudioEvent(audioEventName, ignoreGhostZombie);
	}

	[ClientRpc(channel = 1)]
	private void RpcAudioEvent(string audioEventName, bool ignoreGhostZombie)
	{
		if (!CharacterSFXName.NullOrEmpty() && !base.hasAuthority)
		{
			AkSoundEngine.PostEvent(GetCachedAudioEventName(CharacterSFXName, audioEventName, isZombie, isGhost, ignoreGhostZombie), base.gameObject);
		}
	}

	public void AudioEventExact(string audioEventName)
	{
		AkSoundEngine.PostEvent(audioEventName, base.gameObject);
		if (base.hasAuthority)
		{
			CallCmdAudioEventExact(audioEventName);
		}
	}

	[Command(channel = 1)]
	private void CmdAudioEventExact(string audioEventName)
	{
		CallRpcAudioEventExact(audioEventName);
	}

	[ClientRpc(channel = 1)]
	private void RpcAudioEventExact(string audioEventName)
	{
		if (!base.hasAuthority)
		{
			AkSoundEngine.PostEvent(audioEventName, base.gameObject);
		}
	}

	private void SetAnimatorFloat(AnimParam paramName, float value)
	{
		animator.SetFloat(AnimToString(paramName), value);
	}

	[Command(channel = 1)]
	private void CmdSetAnimatorFloat(AnimParam paramName, float value)
	{
		CallRpcSetAnimatorFloat(paramName, value);
	}

	[ClientRpc(channel = 1)]
	private void RpcSetAnimatorFloat(AnimParam paramName, float value)
	{
		if (!base.hasAuthority)
		{
			SetAnimatorFloat(paramName, value);
		}
	}

	[Command(channel = 1)]
	private void CmdSetScaleX(int scaleX)
	{
		NetworkflipSpriteX = scaleX;
	}

	private void SpawnDustCloudAt(Vector3 position)
	{
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.LAND, position, Modifiers.GetInstance().CharacterRelativeScale, new Color(1f, 1f, 1f, 0.5f));
	}

	private void SpawnDustCloud()
	{
		SpawnDustCloudAt(base.transform.position);
		if (base.hasAuthority)
		{
			CallCmdSpawnDustCloud(base.transform.position);
		}
	}

	[Command(channel = 1)]
	private void CmdSpawnDustCloud(Vector3 position)
	{
		CallRpcSpawnDustCloud(position);
	}

	[ClientRpc(channel = 1)]
	private void RpcSpawnDustCloud(Vector3 position)
	{
		if (!base.hasAuthority)
		{
			SpawnDustCloudAt(position);
		}
	}

	private void SpawnJumpCloudAt(Vector3 position, bool multiJump)
	{
		if (currentAnim == AnimState.DIE)
		{
			position.y += sprite.transform.localPosition.y + DeadCollider.transform.localPosition.y;
		}
		if (multiJump)
		{
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.MULTIJUMP, position, Modifiers.GetInstance().CharacterRelativeScale, new Color(1f, 1f, 1f, 0.5f));
		}
		else
		{
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.JUMP, position, Modifiers.GetInstance().CharacterRelativeScale, new Color(1f, 1f, 1f, 0.5f));
		}
	}

	private void SpawnJumpCloud(bool multiJump = false)
	{
		SpawnJumpCloudAt(base.transform.position, multiJump);
		if (base.hasAuthority)
		{
			CallCmdSpawnJumpCloud(base.transform.position, multiJump);
		}
	}

	[Command(channel = 1)]
	private void CmdSpawnJumpCloud(Vector3 position, bool multiJump)
	{
		CallRpcSpawnJumpCloud(position, multiJump);
	}

	[ClientRpc(channel = 1)]
	private void RpcSpawnJumpCloud(Vector3 position, bool multiJump)
	{
		if (!base.hasAuthority)
		{
			SpawnJumpCloudAt(position, multiJump);
		}
	}

	private void SpawnWallJumpCloudAt(bool right, Vector3 position)
	{
		if (right)
		{
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.WALL_R, position, Modifiers.GetInstance().CharacterRelativeScale, new Color(1f, 1f, 1f, 0.5f));
		}
		else
		{
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.WALL_L, position, Modifiers.GetInstance().CharacterRelativeScale, new Color(1f, 1f, 1f, 0.5f));
		}
	}

	private void SpawnWallJumpCloud(bool right)
	{
		SpawnWallJumpCloudAt(right, base.transform.position);
		if (base.hasAuthority)
		{
			CallCmdSpawnWallJumpCloud(right, base.transform.position);
		}
	}

	[Command(channel = 1)]
	private void CmdSpawnWallJumpCloud(bool right, Vector3 position)
	{
		CallRpcSpawnWallJumpCloud(right, position);
	}

	[ClientRpc(channel = 1)]
	private void RpcSpawnWallJumpCloud(bool right, Vector3 position)
	{
		if (!base.hasAuthority)
		{
			SpawnWallJumpCloudAt(right, position);
		}
	}

	private void SpawnHoneyStickers()
	{
		foreach (PhysicsModifier value in physicsModifiers.Values)
		{
			HoneyPiece component = value.Source.GetComponent<HoneyPiece>();
			if (component != null)
			{
				component.triggerStickness(base.transform);
			}
		}
		if (base.hasAuthority)
		{
			CallCmdSpawnHoneyStickers();
		}
	}

	[Command(channel = 1)]
	private void CmdSpawnHoneyStickers()
	{
		CallRpcSpawnHoneyStickers();
	}

	[ClientRpc(channel = 1)]
	private void RpcSpawnHoneyStickers()
	{
		if (!base.hasAuthority)
		{
			SpawnHoneyStickers();
		}
	}

	[Command(channel = 0)]
	private void CmdPositionCharacter(Vector3 position)
	{
		CallRpcPositionCharacter(position);
	}

	[ClientRpc(channel = 0)]
	private void RpcPositionCharacter(Vector3 position)
	{
		if (!base.hasAuthority)
		{
			PositionCharacter(position);
		}
	}

	private void AnimateIceStreaks()
	{
		float num = 0f;
		foreach (KeyValuePair<GameObject, PhysicsModifier> physicsModifier in physicsModifiers)
		{
			if (physicsModifier.Value.ModifierType == PhysicsModifier.ModType.BaseMotion)
			{
				num += (physicsModifier.Value.Direction * physicsModifier.Value.Magnitude).x;
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (KeyValuePair<GameObject, PhysicsModifier> physicsModifier2 in physicsModifiers)
		{
			if (physicsModifier2.Value.ModifierType != PhysicsModifier.ModType.Friction)
			{
				continue;
			}
			Ice component = physicsModifier2.Key.GetComponent<Ice>();
			if (!(component != null))
			{
				continue;
			}
			if (OnGround && Mathf.Abs(component.transform.rotation.z) < 45f && Mathf.Abs(previousVX - num) > 1f)
			{
				flag = true;
				break;
			}
			if (onWall)
			{
				if (facing == 1)
				{
					flag2 = true;
					break;
				}
				if (facing == -1)
				{
					flag3 = true;
					break;
				}
			}
		}
		ParticleSystem.EmissionModule emission = footIceParticleSystem.emission;
		emission.enabled = flag;
		ParticleSystem.EmissionModule emission2 = leftIceParticleSystem.emission;
		emission2.enabled = flag2;
		ParticleSystem.EmissionModule emission3 = rightIceParticleSystem.emission;
		emission3.enabled = flag3;
	}

	public override void OnStartAuthority()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(delayStartAuthority());
		}
	}

	private IEnumerator delayStartAuthority()
	{
		yield return new WaitForSeconds(1f);
		base.OnStartAuthority();
		PlayerColor = playerColor;
		if (!disabled && netID.isClient && SceneManager.GetActiveScene().name == "TreeHouseLobby")
		{
			SetLobbyCollider(enable: true);
			ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
			if (currentZoomCamera != null)
			{
				currentZoomCamera.RecheckLocalOnly();
			}
		}
	}

	public void ForceJump()
	{
		jump = true;
		jumpDown = true;
		forceNextJump = true;
		CanJump = true;
	}

	public void SitDown()
	{
		SetAnimatorInt(AnimParam.STATE, 3);
	}

	public void LockInputTemporarily(float duration)
	{
		tempLockInput = true;
		StartCoroutine(LockInputCoroutine(duration));
	}

	private IEnumerator LockInputCoroutine(float duration)
	{
		tempLockInput = true;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			yield return null;
		}
		tempLockInput = false;
	}

	[Command]
	public void CmdRequestGrabCoin(NetworkInstanceId netSurrogateId)
	{
		Coin coinFromSurrogateID = Coin.GetCoinFromSurrogateID(netSurrogateId);
		if (coinFromSurrogateID != null)
		{
			if (coinFromSurrogateID.Carrier == null)
			{
				CallRpcGrantCoin(netSurrogateId);
			}
		}
		else
		{
			Debug.LogError("Could not find coin from provided net surrogate ID.");
		}
	}

	[ClientRpc]
	public void RpcGrantCoin(NetworkInstanceId netSurrogateId)
	{
		Coin coinFromSurrogateID = Coin.GetCoinFromSurrogateID(netSurrogateId);
		GameEventManager.SendEvent(new CoinPickupEvent(pickedUp: true));
		if (coinFromSurrogateID != null)
		{
			coinFromSurrogateID.SetCarrier(this);
		}
		else
		{
			Debug.LogError("Could not find coin from provided net surrogate ID.");
		}
	}

	[Command]
	public void CmdRequestBees(NetworkInstanceId netSurrogateId)
	{
		Beehive beehiveFromSurrogateID = Beehive.GetBeehiveFromSurrogateID(netSurrogateId);
		if (beehiveFromSurrogateID != null)
		{
			if (beehiveFromSurrogateID.followedCharacter == null)
			{
				CallRpcGrantBees(netSurrogateId);
			}
		}
		else
		{
			Debug.LogError("Could not find beehive from provided net surrogate ID.");
		}
	}

	[ClientRpc]
	public void RpcGrantBees(NetworkInstanceId netSurrogateId)
	{
		Beehive beehiveFromSurrogateID = Beehive.GetBeehiveFromSurrogateID(netSurrogateId);
		if (beehiveFromSurrogateID != null)
		{
			beehiveFromSurrogateID.SetFollowedCharacter(this);
			beehiveFromSurrogateID.bees.Release(this);
		}
		else
		{
			Debug.LogError("Could not find beehive from provided net surrogate ID.");
		}
	}

	[Command]
	public void CmdFinishedWithCoin(NetworkInstanceId netSurrogateId)
	{
		if (Coin.GetCoinFromSurrogateID(netSurrogateId) != null)
		{
			ScoreKeeper.Instance.AwardPoint(new PointBlock(PointBlock.pointBlockType.coin, networkNumber));
			CallRpcFinishedWithCoin(netSurrogateId);
		}
		else
		{
			Debug.LogError("Could not find coin from provided net surrogate ID.");
		}
	}

	[ClientRpc]
	public void RpcFinishedWithCoin(NetworkInstanceId netSurrogateId)
	{
		if (!base.hasAuthority)
		{
			Coin coinFromSurrogateID = Coin.GetCoinFromSurrogateID(netSurrogateId);
			if (coinFromSurrogateID != null)
			{
				coinFromSurrogateID.DoAwardAnimation();
			}
			else
			{
				Debug.LogError("Could not find coin from provided net surrogate ID.");
			}
		}
	}

	[Command]
	public void CmdDroppedCoin(NetworkInstanceId netSurrogateId, Vector2 coinPosition, bool returnToCoinSpawn)
	{
		if (Coin.GetCoinFromSurrogateID(netSurrogateId) != null)
		{
			CallRpcDroppedCoin(netSurrogateId, coinPosition, returnToCoinSpawn);
		}
		else
		{
			Debug.LogError("Could not find coin from provided net surrogate ID.");
		}
	}

	[ClientRpc]
	public void RpcDroppedCoin(NetworkInstanceId netSurrogateId, Vector2 coinPosition, bool returnToCoinSpawn)
	{
		Coin coinFromSurrogateID = Coin.GetCoinFromSurrogateID(netSurrogateId);
		GameEventManager.SendEvent(new CoinPickupEvent(pickedUp: false));
		if (coinFromSurrogateID != null)
		{
			coinFromSurrogateID.Drop(coinPosition, returnToCoinSpawn);
		}
		else
		{
			Debug.LogError("Could not find coin from provided net surrogate ID.");
		}
	}

	public void TryPickUpJetpack(NetworkInstanceId netSurrogateId)
	{
		if (!jetpackTouched)
		{
			jetpackTouched = true;
			CallCmdRequestGrabJetpack(netSurrogateId);
		}
	}

	[Command]
	public void CmdRequestGrabJetpack(NetworkInstanceId netSurrogateId)
	{
		Jetpack jetpackFromSurrogateID = Jetpack.GetJetpackFromSurrogateID(netSurrogateId);
		if (jetpackFromSurrogateID != null)
		{
			if (jetpackFromSurrogateID.Carrier == null)
			{
				CallRpcGrantJetpack(netSurrogateId);
			}
			else
			{
				CallRpcSetJetPackTouched(value: false);
			}
		}
		else
		{
			CallRpcSetJetPackTouched(value: false);
			Debug.LogError("Could not find jetpack from provided net surrogate ID.");
		}
	}

	[ClientRpc]
	public void RpcSetJetPackTouched(bool value)
	{
		jetpackTouched = value;
	}

	[ClientRpc]
	public void RpcGrantJetpack(NetworkInstanceId netSurrogateId)
	{
		Jetpack jetpackFromSurrogateID = Jetpack.GetJetpackFromSurrogateID(netSurrogateId);
		if (jetpackFromSurrogateID != null && !pickedUpJetpack)
		{
			jetpackFromSurrogateID.SetCarrier(this);
		}
		else if (jetpackFromSurrogateID == null)
		{
			Debug.LogError("Could not find jetpack from provided net surrogate ID.");
		}
		else
		{
			Debug.LogWarning("Character already has a jetpack");
		}
		jetpackTouched = false;
	}

	[Command]
	public void CmdSetLastFlagID(int id)
	{
		NetworkLastFlagID = id;
	}

	private void EnableAFKWarning(float timeLeft)
	{
		SetAFKWarningTime(timeLeft, GameSettings.GetInstance().AFKWarningTime);
	}

	private void ShowAFKWarning()
	{
		if (!AFKWarningVisible)
		{
			AFKWarningCanvas.gameObject.SetActive(value: true);
			AFKWarningVisible = true;
		}
	}

	private void HideAFKWarning(bool initializing = false)
	{
		if (initializing || AFKWarningVisible)
		{
			AFKWarningCanvas.gameObject.SetActive(value: false);
			AFKWarningVisible = false;
		}
	}

	private void DisableAFKWarning(bool initializing = false)
	{
		if (initializing || AFKWarningEnabled)
		{
			HideAFKWarning(initializing);
			AFKWarningEnabled = false;
			afkTimer = 0f;
			if (base.hasAuthority && !initializing)
			{
				CallCmdDisableAFKWarning();
			}
		}
	}

	public void SetAFKWarningTime(float timeLeft, float showWarningTime)
	{
		if (timeLeft > 0f)
		{
			if (!AFKWarningEnabled)
			{
				AFKWarningEnabled = true;
				afkTimer = (float)GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime - timeLeft;
				if (base.hasAuthority)
				{
					CallCmdEnableAFKWarning(timeLeft - LobbyManager.instance.GetAveragePingToServer());
				}
			}
			ShowAFKWarning();
		}
		if (timeLeft < showWarningTime)
		{
			int num = Mathf.CeilToInt(timeLeft);
			if (num >= 1)
			{
				AFKWarningText.text = LocalizationManager.GetTranslation("Network/AFK Warning") + " " + num;
			}
			else
			{
				HideAFKWarning();
			}
		}
		else
		{
			HideAFKWarning();
		}
	}

	[Command]
	private void CmdEnableAFKWarning(float timeLeft)
	{
		CallRpcEnableAFKWarning(timeLeft);
	}

	[ClientRpc]
	private void RpcEnableAFKWarning(float timeLeft)
	{
		if (!base.hasAuthority)
		{
			float averagePingToServer = LobbyManager.instance.GetAveragePingToServer();
			EnableAFKWarning(timeLeft - averagePingToServer);
		}
	}

	[Command]
	public void CmdDisableAFKWarning()
	{
		CallRpcDisableAFKWarning();
	}

	[ClientRpc]
	private void RpcDisableAFKWarning()
	{
		if (!base.hasAuthority)
		{
			DisableAFKWarning();
		}
	}

	private void SetWantsToRetry(bool value)
	{
		NetworkWantsToRetry = value;
		if (base.hasAuthority)
		{
			CallCmdSetWantsToRetry(value);
		}
	}

	[Command]
	public void CmdSetWantsToRetry(bool value)
	{
		NetworkWantsToRetry = value;
	}

	public void SetLocalController(Controller controller)
	{
		holdBToGiveUpInstance.SetLocalController(controller);
		GameControl currentGameController = LobbyManager.instance.CurrentGameController;
		if (currentGameController != null && AssociatedGamePlayer != null && currentGameController.livesDisplayController != null)
		{
			currentGameController.livesDisplayController.SetLocalController(AssociatedGamePlayer.networkNumber, controller);
		}
	}

	private void LateUpdate()
	{
		bool flag = Visible && !bodyHidden;
		if (flag != sprite.enabled)
		{
			sprite.enabled = flag;
		}
		if (HasJetpack && Visible != JetPackSR.enabled)
		{
			JetPackSR.enabled = Visible;
		}
		if (waitingForExtraCorpse)
		{
			TryLinkExtraCorpse();
		}
		Tint();
	}

	private void TriggerSoulExtraction()
	{
		CallCmdSpawnExtraCorpse();
		CallCmdComeBackAsGhost();
		isGhost = true;
		LocallyDead = true;
		waitingToTurnUndead = true;
	}

	[Command]
	public void CmdComeBackAsGhost()
	{
		CallRpcComeBackAsGhost();
	}

	[ClientRpc]
	public void RpcComeBackAsGhost()
	{
		ComeBackAsGhost();
	}

	[Command]
	public void CmdSpawnExtraCorpse()
	{
		GameControl currentGameController = LobbyManager.instance.CurrentGameController;
		GamePlayer gamePlayer = AssociatedGamePlayer;
		Character character = UnityEngine.Object.Instantiate(currentGameController.CharacterPrefab);
		character.associatedGamePlayer = gamePlayer;
		character.gameObject.name = gamePlayer.PickedAnimal.ToString();
		character.NetworkCharacterSprite = gamePlayer.PickedAnimal;
		character.SetOutfitsFromArray(gamePlayer.characterOutfitsList);
		character.NetworknetworkNumber = gamePlayer.networkNumber;
		character.NetworklocalNumber = gamePlayer.localNumber;
		character.NetworkFindPlayerOnSpawn = false;
		character.Networkpicked = true;
		character.Enable(playSound: false);
		character.PositionCharacter(base.transform.position);
		character.NetworkisExtraCorpse = true;
		character.Networkdying = true;
		character.Networkdead = true;
		NetworkServer.SpawnWithClientAuthority(character.gameObject, gamePlayer.gameObject);
		CallRpcOnExtraCorpseSpawned();
	}

	[ClientRpc]
	private void RpcOnExtraCorpseSpawned()
	{
		waitingForExtraCorpse = true;
	}

	private void TryLinkExtraCorpse()
	{
		if (!waitingForExtraCorpse)
		{
			return;
		}
		Character character = null;
		foreach (Character allCharacter in AllCharacters)
		{
			if (allCharacter != this && allCharacter.isExtraCorpse && allCharacter.networkNumber == networkNumber)
			{
				character = allCharacter;
				break;
			}
		}
		if (character != null)
		{
			GamePlayer gamePlayer = (character.associatedGamePlayer = AssociatedGamePlayer);
			character.gameObject.name = gamePlayer.PickedAnimal.ToString();
			character.NetworkCharacterSprite = gamePlayer.PickedAnimal;
			character.SetOutfitsFromArray(gamePlayer.characterOutfitsList);
			character.NetworknetworkNumber = gamePlayer.networkNumber;
			character.NetworklocalNumber = gamePlayer.localNumber;
			character.NetworkFindPlayerOnSpawn = false;
			character.Networkpicked = true;
			character.PositionCharacter(base.transform.position);
			character.Enable(playSound: false);
			character.NetworkisExtraCorpse = true;
			character.Networkdying = true;
			character.Networkdead = true;
			character.DeadColliderSwitch(enabled: true, resetPosition: true);
			character.soul = this;
			character.pickedUpJetpack = pickedUpJetpack;
			pickedUpJetpack = false;
			extraCorpse = character;
			waitingForExtraCorpse = false;
		}
	}

	private void ComeBackAsGhost()
	{
		waitingToTurnUndead = false;
		audioEvent("_Respawn_Ghost", base.gameObject, ignoreGhostZombie: true);
		Networkdying = false;
		Networkdead = true;
		LocallyDead = true;
		isGhost = true;
		DeadColliderSwitch(enabled: false);
		deathFrozen = false;
		deathPauseVel = Vector2.zero;
		LobbyManager.instance.GetCurrentZoomCamera().AddTarget(this);
	}

	private void TriggerZombification()
	{
		CallCmdComeBackAsZombie();
		isZombie = true;
		zombieLocallyDead = false;
		waitingToTurnUndead = true;
		suicideTimer = 0f;
	}

	[Command]
	public void CmdComeBackAsZombie()
	{
		CallRpcComeBackAsZombie();
	}

	[ClientRpc]
	public void RpcComeBackAsZombie()
	{
		ComeBackAsZombie();
	}

	private void ComeBackAsZombie()
	{
		waitingToTurnUndead = false;
		audioEvent("_Respawn_Zombie", base.gameObject, ignoreGhostZombie: true);
		Networkdying = false;
		Networkdead = false;
		LocallyDead = true;
		isGhost = true;
		isZombie = true;
		CreateFlies();
		StartInvincibleTimer(1f);
		DeadColliderSwitch(enabled: false);
		deathFrozen = false;
		deathPauseVel = Vector2.zero;
		if (!HasJetpack)
		{
			ForceJump();
		}
		animator.speed = AnimatorSpeed;
		LobbyManager.instance.GetCurrentZoomCamera().AddTarget(this);
		EnableZombieOutfit(enable: true);
	}

	[Command]
	public void CmdLoseLife()
	{
		CallRpcLoseLife();
	}

	[ClientRpc]
	public void RpcLoseLife()
	{
		LoseLife();
	}

	private void LoseLife()
	{
		AssociatedGamePlayer.lives--;
		GameControl currentGameController = LobbyManager.instance.CurrentGameController;
		if (currentGameController.livesDisplayController != null)
		{
			currentGameController.livesDisplayController.SetPlayerLives(AssociatedGamePlayer.networkNumber, AssociatedGamePlayer.lives);
			currentGameController.livesDisplayController.SetCanRespawn(AssociatedGamePlayer.networkNumber, canRespawn: false);
		}
	}

	private void SetupClientRespawn()
	{
		Networkdying = false;
		Networkdead = false;
		LocallyDead = true;
		Teleporter.OnCharacterRespawned(this);
		AnimalCannon.OnCharacterRespawned(this);
	}

	[Command]
	public void CmdRespawn()
	{
		CallRpcRespawn();
	}

	[ClientRpc]
	public void RpcRespawn()
	{
		Respawn();
	}

	private void Respawn()
	{
		SetupClientRespawn();
		LoseLife();
		Disable();
		LobbyManager.instance.CurrentGameController.LevelLayout.SpawnCharacter(this, 0f);
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, base.transform.position, 0.5f);
		Enable();
		StartInvincibleTimer(1f);
		animator.speed = AnimatorSpeed;
		ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		currentZoomCamera.AddTarget(this);
		if (base.hasAuthority)
		{
			currentZoomCamera.ForceShowAllPlayer(showAll: false);
			currentZoomCamera.RecheckLocalOnly();
		}
		audioEvent("_Respawn_Normal", base.gameObject, ignoreGhostZombie: true);
		StartCoroutine(WaitAndUpdateHoldBIndicator());
	}

	public void TemporaryIgnoreCollision(Collider2D[] colliders)
	{
		CharacterRaycaster[] array = cachedRaycasters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		Collider2D[] array2 = cachedPlayerColliders;
		foreach (Collider2D collider in array2)
		{
			foreach (Collider2D collider2 in colliders)
			{
				Physics2D.IgnoreCollision(collider, collider2, ignore: true);
			}
		}
		ignoredCollisions.AddRange(colliders);
	}

	public void ClearTemporaryIgnoreCollisions()
	{
		CharacterRaycaster[] array = cachedRaycasters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		Collider2D[] array2 = cachedPlayerColliders;
		foreach (Collider2D collider in array2)
		{
			foreach (Collider2D ignoredCollision in ignoredCollisions)
			{
				Physics2D.IgnoreCollision(collider, ignoredCollision, ignore: false);
			}
		}
		ignoredCollisions.Clear();
	}

	private IEnumerator WaitAndUpdateHoldBIndicator()
	{
		yield return new WaitForSeconds(0.5f);
		UpdateHoldBIndicator();
	}

	public void RefreshScale()
	{
		base.transform.localScale = Modifiers.GetInstance().CharacterScale.MakeVector3();
		AkSoundEngine.PostEvent("RULES_Set_Char_Size_" + Modifiers.GetInstance().CharacterScaleAudioStateString, base.gameObject);
	}

	private void ReproducePhysicsModifiers(Character otherChar)
	{
		int num = otherChar.FeetPhysicsCollider.GetComponent<Collider2D>().OverlapCollider(footPhysicsContactFilter, playerContactResultCache);
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				GameObject go = playerContactResultCache[i].gameObject;
				FeetPhysicsCollider.PassPhysicsModifier(go, null);
			}
		}
	}

	private void DuplicateCharacterMotion()
	{
		Character standingOnCharacterNested = StandingOnCharacterNested;
		if (standingOnCharacterNested != null && standingOnCharacterNested != this && !standingOnCharacterNested.CrouchingDown)
		{
			if (standingOnCharacterNested.nonLocalColliderMode == NonLocalColliderMode.PickedNonLocal)
			{
				characterInheritedMotion = Vector2.zero;
				ReproducePhysicsModifiers(standingOnCharacterNested);
			}
			else
			{
				characterInheritedMotion = standingOnCharacterNested.lastInheritedMotion;
			}
		}
		else
		{
			characterInheritedMotion = Vector2.zero;
		}
	}

	public void OnHitLevelBottom()
	{
		if (!Dead && !Dying)
		{
			KillCharacter("Falling", deathFreezeOn: false, 0);
		}
		else if (isGhost)
		{
			SetLocalDeadCmdDead();
		}
		diedInPit = true;
	}

	private void SetupJetPackArt(bool enabled)
	{
		if (enabled)
		{
			GameSettings.animalColors animalColors = default(GameSettings.animalColors);
			GameSettings.animalColors[] characterColors = GameSettings.GetInstance().characterColors;
			for (int i = 0; i < characterColors.Length; i++)
			{
				GameSettings.animalColors animalColors2 = characterColors[i];
				if (animalColors2.type == CharacterSprite)
				{
					animalColors = animalColors2;
				}
			}
			JetPackSR.GetComponent<Renderer>().material = DefaultJetPackMaterial;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			JetPackSR.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_HueShiftAmount", animalColors.JetpackHue);
			materialPropertyBlock.SetFloat("_SatShiftAmount", animalColors.JetpackSat);
			materialPropertyBlock.SetFloat("_ValShiftAmount", animalColors.JetpackVal);
			materialPropertyBlock.SetFloat("_ContrastShiftAmount", 1f);
			materialPropertyBlock.SetFloat("_Colorize", 1f);
			JetPackSR.SetPropertyBlock(materialPropertyBlock);
		}
		else
		{
			JetPackSR.enabled = false;
		}
	}

	[Command]
	public void CmdAgonize()
	{
		CallRpcAgonize();
	}

	[ClientRpc]
	public void RpcAgonize()
	{
		LobbyManager.instance.GetCurrentZoomCamera().AddTarget(this);
		agonyTimer = maxDeathDelay;
	}

	private void SetJetpackEmitting(bool onOff)
	{
		if (onOff && !disabled)
		{
			if (!JetpackParticles.isPlaying || !JetpackParticles.isEmitting)
			{
				JetpackParticles.Play();
				if (!Jetpacking)
				{
					AkSoundEngine.PostEvent("SFX_Char_Jetpack_Start", base.gameObject);
				}
				Jetpacking = true;
			}
		}
		else
		{
			JetpackParticles.Stop();
			if (Jetpacking)
			{
				AkSoundEngine.PostEvent("SFX_Char_Jetpack_Stop", base.gameObject);
			}
			Jetpacking = false;
		}
		if (Jetpacking)
		{
			AkSoundEngine.SetRTPCValue("Jetpack_Vertical_Speed", previousVY, base.gameObject);
		}
	}

	public static IEnumerable<Character> GetCharactersInCollider(Collider2D c)
	{
		bool wasEnabled = c.enabled;
		c.enabled = true;
		int numResults = c.OverlapCollider(playerContactFilter, playerContactResultCache);
		HashSet<Character> seenCharacters = new HashSet<Character>();
		int i = 0;
		while (i < numResults)
		{
			Character componentInParent = playerContactResultCache[i].GetComponentInParent<Character>();
			if (componentInParent != null && !seenCharacters.Contains(componentInParent))
			{
				seenCharacters.Add(componentInParent);
				yield return componentInParent;
			}
			int num = i + 1;
			i = num;
		}
		c.enabled = wasEnabled;
	}

	public void SetJetpackPickedUp(bool pickedUp)
	{
		pickedUpJetpack = pickedUp;
	}

	[Command]
	public void CmdIShouldBeKicked(LobbyManager.KickReasons kickReason)
	{
		LobbyManager.instance.IssueKickMessage(networkNumber, kickReason);
	}

	[Command]
	public void CmdSetRemoteJetpackState(bool onOff)
	{
		CallRpcSetRemoteJetpackState(onOff);
	}

	[ClientRpc]
	public void RpcSetRemoteJetpackState(bool onOff)
	{
		if (!base.hasAuthority)
		{
			lastJetpackState = onOff;
		}
	}

	public void forceCrouchCommand()
	{
		forceCrouchTimer = 0.75f;
		UpperBodyEnable(onOff: false);
		SetAnimatorInt(AnimParam.SECONDARYSTATE, 2);
	}

	public void EnableBoostEffect(float time)
	{
		boostEffectTimer = time;
	}

	public void ClearVelocity()
	{
		chrRigidBody.position = smoothSync.getPosition();
		chrRigidBody.velocity = Vector2.zero;
	}

	private float GetBoostLateralSpeedLimit(float normalMaxSpeed)
	{
		GameSettings instance = GameSettings.GetInstance();
		float time = boostEffectTimer / instance.boostEffectDuration;
		return Mathf.Lerp(normalMaxSpeed, boostVMax.x, instance.boostEffectCurve.Evaluate(time));
	}

	public void ForceAllowSuicideFor(float time)
	{
		forceAllowSuicideTimer = time;
	}

	public void ForceAllowSuicidePermanently(bool on)
	{
		forceAllowSuicideTimer = (on ? (-1f) : 0f);
	}

	public void AddDeathTimerTimer(float time)
	{
		if (dying)
		{
			deathTimer += time;
		}
	}

	private void UpdateSuicidalState()
	{
		GameSettings instance = GameSettings.GetInstance();
		Modifiers instance2 = Modifiers.GetInstance();
		bool flag = instance2.PostDeathBehavior == Modifiers.PostDeathBehaviors.Zombie && isZombie && !dead && !dying;
		bool runTimerHit = RunTimerHit;
		if (runTimerHit)
		{
			OnRunTimerHit();
		}
		bool flag2 = (instance.respawnMode == RespawnMode.RespawnsPerMatch || instance.respawnMode == RespawnMode.RespawnsPerRound) && (instance.GameMode == GameState.GameMode.CREATIVE || instance.GameMode == GameState.GameMode.PARTY) && AssociatedGamePlayer != null && AssociatedGamePlayer.lives > 0 && (dead || dying) && !runTimerHit && !holdingRespawns;
		bool flag3 = false;
		switch (instance2.PostDeathBehavior)
		{
		case Modifiers.PostDeathBehaviors.Zombie:
			flag3 = !isGhost && instance.GameMode != GameState.GameMode.FREEPLAY && instance.GameMode != GameState.GameMode.CHALLENGE && !zombieLocallyDead;
			break;
		case Modifiers.PostDeathBehaviors.Ghost:
			flag3 = !isGhost && instance.GameMode != GameState.GameMode.FREEPLAY && instance.GameMode != GameState.GameMode.CHALLENGE && !diedInPit;
			break;
		}
		bool flag4 = !flag3 && (instance.GameMode == GameState.GameMode.FREEPLAY || instance.GameMode == GameState.GameMode.CHALLENGE || flag || flag2);
		bool flag5 = instance.GameMode == GameState.GameMode.FREEPLAY;
		if (((dying || dead) && (!flag4 || wantsToRetryUsed)) || Waiting)
		{
			return;
		}
		if (suicideDown || (flag4 && !wantsToRetryUsed))
		{
			UpdateHoldBIndicator();
		}
		if (suicide && AssociatedGamePlayer != null && !backDownOnEnable && !lockSuicide && (flag5 || !succeeding))
		{
			suicideTimer += Time.fixedUnscaledDeltaTime;
			if (holdBToGiveUpInstance != null)
			{
				if (!holdBToGiveUpInstance.Visible)
				{
					holdBToGiveUpInstance.Show();
				}
				setHoldBFillAmount(suicideTimer / SuicideTime);
			}
			if (suicideTimer > SuicideTime)
			{
				if (holdBToGiveUpInstance != null)
				{
					hideHoldBIndicator();
				}
				if (instance.GameMode == GameState.GameMode.FREEPLAY)
				{
					SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, base.transform.position, 0.5f * instance2.CharacterRelativeScale);
					Disable();
					GameEventManager.SendEvent(new FreePlayPlayerSwitchEvent(networkNumber, GameControl.GamePhase.PLACE));
					CallCmdSwitchFreeMode();
					suicideTimer = 0f;
				}
				else if (instance.GameMode == GameState.GameMode.CHALLENGE && !wantsToRetryUsed)
				{
					wantsToRetryUsed = true;
					MsgPlayerWantsToRetry msgPlayerWantsToRetry = new MsgPlayerWantsToRetry();
					msgPlayerWantsToRetry.networkNumber = networkNumber;
					NetworkManager.singleton.client.Send(NetMsgTypes.PlayerWantsToRetry, msgPlayerWantsToRetry);
				}
				else if ((!dying || isZombie) && !flag2)
				{
					StatTracker.Instance.GetSaveFileDataForLocalPlayer(localNumber, fallback: true)?.IncrementStat("DeathsBySuicide");
					AnimatorTrigger(AnimParam.DEATHTRIGGER);
					KillCharacter("Suicide", deathFreezeOn: false, 0);
					suicideTimer = 0f;
					hideHoldBIndicator();
					backDownOnEnable = true;
				}
				else if (flag2)
				{
					SetupClientRespawn();
					CallCmdRespawn();
					suicideTimer = 0f;
					hideHoldBIndicator();
					backDownOnEnable = true;
				}
			}
		}
		else
		{
			suicideTimer = 0f;
			setHoldBFillAmount(0f);
			hideHoldBIndicator();
		}
	}

	static Character()
	{
		animHashSetup = false;
		playerContactResultCache = new Collider2D[50];
		playerContactFilter = GetHeadCheckContactFilter();
		footPhysicsContactFilter = GetFootPhysicsContactFilter();
		AllCharacters = new List<Character>(32);
		kCmdCmdEnable = 241366572;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdEnable, InvokeCmdCmdEnable);
		kCmdCmdDisable = -2121636001;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdDisable, InvokeCmdCmdDisable);
		kCmdCmdClearExtraCorpse = -2110033784;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdClearExtraCorpse, InvokeCmdCmdClearExtraCorpse);
		kCmdCmdShowSprite = 1479027787;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdShowSprite, InvokeCmdCmdShowSprite);
		kCmdCmdFreeze = 273812288;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdFreeze, InvokeCmdCmdFreeze);
		kCmdCmdCommunicateOutfitsArray = 933011251;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdCommunicateOutfitsArray, InvokeCmdCmdCommunicateOutfitsArray);
		kCmdCmdSetupDeath = 1744749088;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetupDeath, InvokeCmdCmdSetupDeath);
		kCmdCmdSetLocalPlayerID = 1179192430;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetLocalPlayerID, InvokeCmdCmdSetLocalPlayerID);
		kCmdCmdGetLocalController = 1249444442;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdGetLocalController, InvokeCmdCmdGetLocalController);
		kCmdCmdSetPlayerColor = 1242503913;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetPlayerColor, InvokeCmdCmdSetPlayerColor);
		kCmdCmdSetSuccess = 1806114154;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetSuccess, InvokeCmdCmdSetSuccess);
		kCmdCmdSetDying = -226373036;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetDying, InvokeCmdCmdSetDying);
		kCmdCmdSetDead = -1808437155;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetDead, InvokeCmdCmdSetDead);
		kCmdCmdSetLobbyCollider = -1753573871;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetLobbyCollider, InvokeCmdCmdSetLobbyCollider);
		kCmdCmdSetPicked = 1900962361;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetPicked, InvokeCmdCmdSetPicked);
		kCmdCmdSwitchFreeMode = 193752364;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSwitchFreeMode, InvokeCmdCmdSwitchFreeMode);
		kCmdCmdAnimatorTrigger = -1963598264;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdAnimatorTrigger, InvokeCmdCmdAnimatorTrigger);
		kCmdCmdSetAnimatorInt = 1863223663;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetAnimatorInt, InvokeCmdCmdSetAnimatorInt);
		kCmdCmdSetBubble = 1511198277;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetBubble, InvokeCmdCmdSetBubble);
		kCmdCmdAudioEvent = -2069942131;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdAudioEvent, InvokeCmdCmdAudioEvent);
		kCmdCmdAudioEventExact = 1677621874;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdAudioEventExact, InvokeCmdCmdAudioEventExact);
		kCmdCmdSetAnimatorFloat = -446254116;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetAnimatorFloat, InvokeCmdCmdSetAnimatorFloat);
		kCmdCmdSetScaleX = 1981250055;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetScaleX, InvokeCmdCmdSetScaleX);
		kCmdCmdSpawnDustCloud = 195399953;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSpawnDustCloud, InvokeCmdCmdSpawnDustCloud);
		kCmdCmdSpawnJumpCloud = 1091003669;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSpawnJumpCloud, InvokeCmdCmdSpawnJumpCloud);
		kCmdCmdSpawnWallJumpCloud = 1401384811;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSpawnWallJumpCloud, InvokeCmdCmdSpawnWallJumpCloud);
		kCmdCmdSpawnHoneyStickers = 370989727;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSpawnHoneyStickers, InvokeCmdCmdSpawnHoneyStickers);
		kCmdCmdPositionCharacter = 1299241911;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdPositionCharacter, InvokeCmdCmdPositionCharacter);
		kCmdCmdRequestGrabCoin = 716430371;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdRequestGrabCoin, InvokeCmdCmdRequestGrabCoin);
		kCmdCmdRequestBees = 1362873431;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdRequestBees, InvokeCmdCmdRequestBees);
		kCmdCmdFinishedWithCoin = -2129238926;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdFinishedWithCoin, InvokeCmdCmdFinishedWithCoin);
		kCmdCmdDroppedCoin = -1851401368;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdDroppedCoin, InvokeCmdCmdDroppedCoin);
		kCmdCmdRequestGrabJetpack = -1168696992;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdRequestGrabJetpack, InvokeCmdCmdRequestGrabJetpack);
		kCmdCmdSetLastFlagID = 889874678;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetLastFlagID, InvokeCmdCmdSetLastFlagID);
		kCmdCmdEnableAFKWarning = -41373918;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdEnableAFKWarning, InvokeCmdCmdEnableAFKWarning);
		kCmdCmdDisableAFKWarning = -2006549355;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdDisableAFKWarning, InvokeCmdCmdDisableAFKWarning);
		kCmdCmdSetWantsToRetry = 252683779;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetWantsToRetry, InvokeCmdCmdSetWantsToRetry);
		kCmdCmdComeBackAsGhost = -1136207255;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdComeBackAsGhost, InvokeCmdCmdComeBackAsGhost);
		kCmdCmdSpawnExtraCorpse = -607401318;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSpawnExtraCorpse, InvokeCmdCmdSpawnExtraCorpse);
		kCmdCmdComeBackAsZombie = -312344180;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdComeBackAsZombie, InvokeCmdCmdComeBackAsZombie);
		kCmdCmdLoseLife = 744464922;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdLoseLife, InvokeCmdCmdLoseLife);
		kCmdCmdRespawn = 1599410591;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdRespawn, InvokeCmdCmdRespawn);
		kCmdCmdAgonize = -549737690;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdAgonize, InvokeCmdCmdAgonize);
		kCmdCmdIShouldBeKicked = 259103899;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdIShouldBeKicked, InvokeCmdCmdIShouldBeKicked);
		kCmdCmdSetRemoteJetpackState = -342115554;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Character), kCmdCmdSetRemoteJetpackState, InvokeCmdCmdSetRemoteJetpackState);
		kRpcRpcEnable = 1023435138;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcEnable, InvokeRpcRpcEnable);
		kRpcRpcDisable = 647653065;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcDisable, InvokeRpcRpcDisable);
		kRpcRpcShowprite = -2072576484;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcShowprite, InvokeRpcRpcShowprite);
		kRpcRpcFreeze = 1055880854;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcFreeze, InvokeRpcRpcFreeze);
		kRpcRpcCommunicateOutfitsArray = 2032437917;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcCommunicateOutfitsArray, InvokeRpcRpcCommunicateOutfitsArray);
		kRpcRpcSetupDeath = -391474570;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetupDeath, InvokeRpcRpcSetupDeath);
		kRpcRpcFindLocalPlayer = -510733932;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcFindLocalPlayer, InvokeRpcRpcFindLocalPlayer);
		kRpcRpcGetLocalController = 1498553520;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcGetLocalController, InvokeRpcRpcGetLocalController);
		kRpcRpcSendPlayerSuccessEvent = -1293842049;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSendPlayerSuccessEvent, InvokeRpcRpcSendPlayerSuccessEvent);
		kRpcRpcSetLobbyCollider = -2124263961;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetLobbyCollider, InvokeRpcRpcSetLobbyCollider);
		kRpcRpcSetRaycastsEnabled = 1990033780;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetRaycastsEnabled, InvokeRpcRpcSetRaycastsEnabled);
		kRpcRpcSetReady = -265432416;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetReady, InvokeRpcRpcSetReady);
		kRpcRpcSwitchFreeMode = -1232330110;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSwitchFreeMode, InvokeRpcRpcSwitchFreeMode);
		kRpcRpcAnimatorTrigger = 1072485298;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcAnimatorTrigger, InvokeRpcRpcAnimatorTrigger);
		kRpcRpcSetAnimatorInt = 437141189;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetAnimatorInt, InvokeRpcRpcSetAnimatorInt);
		kRpcRpcSetBubble = -81732817;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetBubble, InvokeRpcRpcSetBubble);
		kRpcRpcAudioEvent = 88801507;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcAudioEvent, InvokeRpcRpcAudioEvent);
		kRpcRpcAudioEventExact = 418738140;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcAudioEventExact, InvokeRpcRpcAudioEventExact);
		kRpcRpcSetAnimatorFloat = -816944206;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetAnimatorFloat, InvokeRpcRpcSetAnimatorFloat);
		kRpcRpcSpawnDustCloud = -1230682521;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSpawnDustCloud, InvokeRpcRpcSpawnDustCloud);
		kRpcRpcSpawnJumpCloud = -335078805;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSpawnJumpCloud, InvokeRpcRpcSpawnJumpCloud);
		kRpcRpcSpawnWallJumpCloud = 1650493889;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSpawnWallJumpCloud, InvokeRpcRpcSpawnWallJumpCloud);
		kRpcRpcSpawnHoneyStickers = 620098805;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSpawnHoneyStickers, InvokeRpcRpcSpawnHoneyStickers);
		kRpcRpcPositionCharacter = -1602216287;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcPositionCharacter, InvokeRpcRpcPositionCharacter);
		kRpcRpcGrantCoin = -1405237906;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcGrantCoin, InvokeRpcRpcGrantCoin);
		kRpcRpcGrantBees = -1405277426;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcGrantBees, InvokeRpcRpcGrantBees);
		kRpcRpcFinishedWithCoin = 1795038280;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcFinishedWithCoin, InvokeRpcRpcFinishedWithCoin);
		kRpcRpcDroppedCoin = 645141970;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcDroppedCoin, InvokeRpcRpcDroppedCoin);
		kRpcRpcSetJetPackTouched = -1160845553;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetJetPackTouched, InvokeRpcRpcSetJetPackTouched);
		kRpcRpcGrantJetpack = 1245358133;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcGrantJetpack, InvokeRpcRpcGrantJetpack);
		kRpcRpcEnableAFKWarning = -412064008;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcEnableAFKWarning, InvokeRpcRpcEnableAFKWarning);
		kRpcRpcDisableAFKWarning = -613040257;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcDisableAFKWarning, InvokeRpcRpcDisableAFKWarning);
		kRpcRpcComeBackAsGhost = 1899876307;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcComeBackAsGhost, InvokeRpcRpcComeBackAsGhost);
		kRpcRpcOnExtraCorpseSpawned = 1620295180;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcOnExtraCorpseSpawned, InvokeRpcRpcOnExtraCorpseSpawned);
		kRpcRpcComeBackAsZombie = -683034270;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcComeBackAsZombie, InvokeRpcRpcComeBackAsZombie);
		kRpcRpcLoseLife = 693080048;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcLoseLife, InvokeRpcRpcLoseLife);
		kRpcRpcRespawn = 73732361;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcRespawn, InvokeRpcRpcRespawn);
		kRpcRpcAgonize = -2075415920;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcAgonize, InvokeRpcRpcAgonize);
		kRpcRpcSetRemoteJetpackState = -837060344;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Character), kRpcRpcSetRemoteJetpackState, InvokeRpcRpcSetRemoteJetpackState);
		NetworkCRC.RegisterBehaviour("Character", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEnable called on client.");
		}
		else
		{
			((Character)obj).CmdEnable(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdDisable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDisable called on client.");
		}
		else
		{
			((Character)obj).CmdDisable(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdClearExtraCorpse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearExtraCorpse called on client.");
		}
		else
		{
			((Character)obj).CmdClearExtraCorpse();
		}
	}

	protected static void InvokeCmdCmdShowSprite(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShowSprite called on client.");
		}
		else
		{
			((Character)obj).CmdShowSprite(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdFreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFreeze called on client.");
		}
		else
		{
			((Character)obj).CmdFreeze();
		}
	}

	protected static void InvokeCmdCmdCommunicateOutfitsArray(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCommunicateOutfitsArray called on client.");
		}
		else
		{
			((Character)obj).CmdCommunicateOutfitsArray(GeneratedNetworkCode._ReadArrayInt32_None(reader));
		}
	}

	protected static void InvokeCmdCmdSetupDeath(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetupDeath called on client.");
		}
		else
		{
			((Character)obj).CmdSetupDeath(reader.ReadString(), reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetLocalPlayerID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLocalPlayerID called on client.");
		}
		else
		{
			((Character)obj).CmdSetLocalPlayerID((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdGetLocalController(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetLocalController called on client.");
		}
		else
		{
			((Character)obj).CmdGetLocalController((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetPlayerColor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerColor called on client.");
		}
		else
		{
			((Character)obj).CmdSetPlayerColor(reader.ReadColor());
		}
	}

	protected static void InvokeCmdCmdSetSuccess(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSuccess called on client.");
		}
		else
		{
			((Character)obj).CmdSetSuccess(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetDying(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDying called on client.");
		}
		else
		{
			((Character)obj).CmdSetDying(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetDead(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDead called on client.");
		}
		else
		{
			((Character)obj).CmdSetDead(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetLobbyCollider(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLobbyCollider called on client.");
		}
		else
		{
			((Character)obj).CmdSetLobbyCollider(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetPicked(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPicked called on client.");
		}
		else
		{
			((Character)obj).CmdSetPicked(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSwitchFreeMode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwitchFreeMode called on client.");
		}
		else
		{
			((Character)obj).CmdSwitchFreeMode();
		}
	}

	protected static void InvokeCmdCmdAnimatorTrigger(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAnimatorTrigger called on client.");
		}
		else
		{
			((Character)obj).CmdAnimatorTrigger((AnimParam)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSetAnimatorInt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAnimatorInt called on client.");
		}
		else
		{
			((Character)obj).CmdSetAnimatorInt((AnimParam)reader.ReadInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetBubble(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetBubble called on client.");
		}
		else
		{
			((Character)obj).CmdSetBubble((int)reader.ReadPackedUInt32(), reader.ReadBoolean(), reader.ReadColor());
		}
	}

	protected static void InvokeCmdCmdAudioEvent(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAudioEvent called on client.");
		}
		else
		{
			((Character)obj).CmdAudioEvent(reader.ReadString(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdAudioEventExact(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAudioEventExact called on client.");
		}
		else
		{
			((Character)obj).CmdAudioEventExact(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetAnimatorFloat(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAnimatorFloat called on client.");
		}
		else
		{
			((Character)obj).CmdSetAnimatorFloat((AnimParam)reader.ReadInt32(), reader.ReadSingle());
		}
	}

	protected static void InvokeCmdCmdSetScaleX(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetScaleX called on client.");
		}
		else
		{
			((Character)obj).CmdSetScaleX((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSpawnDustCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnDustCloud called on client.");
		}
		else
		{
			((Character)obj).CmdSpawnDustCloud(reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdSpawnJumpCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnJumpCloud called on client.");
		}
		else
		{
			((Character)obj).CmdSpawnJumpCloud(reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnWallJumpCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnWallJumpCloud called on client.");
		}
		else
		{
			((Character)obj).CmdSpawnWallJumpCloud(reader.ReadBoolean(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdSpawnHoneyStickers(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnHoneyStickers called on client.");
		}
		else
		{
			((Character)obj).CmdSpawnHoneyStickers();
		}
	}

	protected static void InvokeCmdCmdPositionCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPositionCharacter called on client.");
		}
		else
		{
			((Character)obj).CmdPositionCharacter(reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdRequestGrabCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestGrabCoin called on client.");
		}
		else
		{
			((Character)obj).CmdRequestGrabCoin(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdRequestBees(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestBees called on client.");
		}
		else
		{
			((Character)obj).CmdRequestBees(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdFinishedWithCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFinishedWithCoin called on client.");
		}
		else
		{
			((Character)obj).CmdFinishedWithCoin(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdDroppedCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDroppedCoin called on client.");
		}
		else
		{
			((Character)obj).CmdDroppedCoin(reader.ReadNetworkId(), reader.ReadVector2(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdRequestGrabJetpack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestGrabJetpack called on client.");
		}
		else
		{
			((Character)obj).CmdRequestGrabJetpack(reader.ReadNetworkId());
		}
	}

	protected static void InvokeCmdCmdSetLastFlagID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLastFlagID called on client.");
		}
		else
		{
			((Character)obj).CmdSetLastFlagID((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdEnableAFKWarning(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEnableAFKWarning called on client.");
		}
		else
		{
			((Character)obj).CmdEnableAFKWarning(reader.ReadSingle());
		}
	}

	protected static void InvokeCmdCmdDisableAFKWarning(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDisableAFKWarning called on client.");
		}
		else
		{
			((Character)obj).CmdDisableAFKWarning();
		}
	}

	protected static void InvokeCmdCmdSetWantsToRetry(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetWantsToRetry called on client.");
		}
		else
		{
			((Character)obj).CmdSetWantsToRetry(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdComeBackAsGhost(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdComeBackAsGhost called on client.");
		}
		else
		{
			((Character)obj).CmdComeBackAsGhost();
		}
	}

	protected static void InvokeCmdCmdSpawnExtraCorpse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnExtraCorpse called on client.");
		}
		else
		{
			((Character)obj).CmdSpawnExtraCorpse();
		}
	}

	protected static void InvokeCmdCmdComeBackAsZombie(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdComeBackAsZombie called on client.");
		}
		else
		{
			((Character)obj).CmdComeBackAsZombie();
		}
	}

	protected static void InvokeCmdCmdLoseLife(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLoseLife called on client.");
		}
		else
		{
			((Character)obj).CmdLoseLife();
		}
	}

	protected static void InvokeCmdCmdRespawn(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRespawn called on client.");
		}
		else
		{
			((Character)obj).CmdRespawn();
		}
	}

	protected static void InvokeCmdCmdAgonize(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAgonize called on client.");
		}
		else
		{
			((Character)obj).CmdAgonize();
		}
	}

	protected static void InvokeCmdCmdIShouldBeKicked(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdIShouldBeKicked called on client.");
		}
		else
		{
			((Character)obj).CmdIShouldBeKicked((LobbyManager.KickReasons)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSetRemoteJetpackState(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetRemoteJetpackState called on client.");
		}
		else
		{
			((Character)obj).CmdSetRemoteJetpackState(reader.ReadBoolean());
		}
	}

	public void CallCmdEnable(bool playsound)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEnable called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEnable(playsound);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEnable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(playsound);
		SendCommandInternal(networkWriter, 0, "CmdEnable");
	}

	public void CallCmdDisable(bool moveAway)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDisable called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDisable(moveAway);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDisable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(moveAway);
		SendCommandInternal(networkWriter, 0, "CmdDisable");
	}

	public void CallCmdClearExtraCorpse()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdClearExtraCorpse called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdClearExtraCorpse();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdClearExtraCorpse);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdClearExtraCorpse");
	}

	public void CallCmdShowSprite(bool show)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdShowSprite called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdShowSprite(show);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdShowSprite);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(show);
		SendCommandInternal(networkWriter, 0, "CmdShowSprite");
	}

	public void CallCmdFreeze()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFreeze called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFreeze();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdFreeze");
	}

	public void CallCmdCommunicateOutfitsArray(int[] outfitsArray)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCommunicateOutfitsArray called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCommunicateOutfitsArray(outfitsArray);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCommunicateOutfitsArray);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, outfitsArray);
		SendCommandInternal(networkWriter, 0, "CmdCommunicateOutfitsArray");
	}

	public void CallCmdSetupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetupDeath called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetupDeath(cause, deathFreezeOn, causedByPlayerNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetupDeath);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(cause);
		networkWriter.Write(deathFreezeOn);
		networkWriter.WritePackedUInt32((uint)causedByPlayerNumber);
		SendCommandInternal(networkWriter, 0, "CmdSetupDeath");
	}

	public void CallCmdSetLocalPlayerID(int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLocalPlayerID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLocalPlayerID(localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLocalPlayerID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdSetLocalPlayerID");
	}

	public void CallCmdGetLocalController(int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGetLocalController called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGetLocalController(localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGetLocalController);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdGetLocalController");
	}

	public void CallCmdSetPlayerColor(Color c)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerColor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerColor(c);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerColor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(c);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerColor");
	}

	public void CallCmdSetSuccess(bool s)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSuccess called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetSuccess(s);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetSuccess);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(s);
		SendCommandInternal(networkWriter, 0, "CmdSetSuccess");
	}

	public void CallCmdSetDying(bool d)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetDying called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetDying(d);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetDying);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(d);
		SendCommandInternal(networkWriter, 0, "CmdSetDying");
	}

	public void CallCmdSetDead(bool d)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetDead called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetDead(d);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetDead);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(d);
		SendCommandInternal(networkWriter, 0, "CmdSetDead");
	}

	public void CallCmdSetLobbyCollider(bool enable)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLobbyCollider called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLobbyCollider(enable);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLobbyCollider);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(enable);
		SendCommandInternal(networkWriter, 0, "CmdSetLobbyCollider");
	}

	public void CallCmdSetPicked(bool picked)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPicked called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPicked(picked);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPicked);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(picked);
		SendCommandInternal(networkWriter, 0, "CmdSetPicked");
	}

	public void CallCmdSwitchFreeMode()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSwitchFreeMode called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSwitchFreeMode();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSwitchFreeMode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSwitchFreeMode");
	}

	public void CallCmdAnimatorTrigger(AnimParam triggerName)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAnimatorTrigger called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAnimatorTrigger(triggerName);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAnimatorTrigger);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)triggerName);
		SendCommandInternal(networkWriter, 1, "CmdAnimatorTrigger");
	}

	public void CallCmdSetAnimatorInt(AnimParam paramName, int value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetAnimatorInt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetAnimatorInt(paramName, value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetAnimatorInt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)paramName);
		networkWriter.WritePackedUInt32((uint)value);
		SendCommandInternal(networkWriter, 1, "CmdSetAnimatorInt");
	}

	public void CallCmdSetBubble(int animId, bool value, Color color)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetBubble called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetBubble(animId, value, color);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetBubble);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)animId);
		networkWriter.Write(value);
		networkWriter.Write(color);
		SendCommandInternal(networkWriter, 1, "CmdSetBubble");
	}

	public void CallCmdAudioEvent(string audioEventName, bool ignoreGhostZombie)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAudioEvent called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAudioEvent(audioEventName, ignoreGhostZombie);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAudioEvent);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(audioEventName);
		networkWriter.Write(ignoreGhostZombie);
		SendCommandInternal(networkWriter, 1, "CmdAudioEvent");
	}

	public void CallCmdAudioEventExact(string audioEventName)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAudioEventExact called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAudioEventExact(audioEventName);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAudioEventExact);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(audioEventName);
		SendCommandInternal(networkWriter, 1, "CmdAudioEventExact");
	}

	public void CallCmdSetAnimatorFloat(AnimParam paramName, float value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetAnimatorFloat called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetAnimatorFloat(paramName, value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetAnimatorFloat);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)paramName);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 1, "CmdSetAnimatorFloat");
	}

	public void CallCmdSetScaleX(int scaleX)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetScaleX called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetScaleX(scaleX);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetScaleX);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)scaleX);
		SendCommandInternal(networkWriter, 1, "CmdSetScaleX");
	}

	public void CallCmdSpawnDustCloud(Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnDustCloud called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnDustCloud(position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnDustCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 1, "CmdSpawnDustCloud");
	}

	public void CallCmdSpawnJumpCloud(Vector3 position, bool multiJump)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnJumpCloud called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnJumpCloud(position, multiJump);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnJumpCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(multiJump);
		SendCommandInternal(networkWriter, 1, "CmdSpawnJumpCloud");
	}

	public void CallCmdSpawnWallJumpCloud(bool right, Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnWallJumpCloud called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnWallJumpCloud(right, position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnWallJumpCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(right);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 1, "CmdSpawnWallJumpCloud");
	}

	public void CallCmdSpawnHoneyStickers()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnHoneyStickers called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnHoneyStickers();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnHoneyStickers);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 1, "CmdSpawnHoneyStickers");
	}

	public void CallCmdPositionCharacter(Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPositionCharacter called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPositionCharacter(position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPositionCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 0, "CmdPositionCharacter");
	}

	public void CallCmdRequestGrabCoin(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestGrabCoin called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestGrabCoin(netSurrogateId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestGrabCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendCommandInternal(networkWriter, 0, "CmdRequestGrabCoin");
	}

	public void CallCmdRequestBees(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestBees called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestBees(netSurrogateId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestBees);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendCommandInternal(networkWriter, 0, "CmdRequestBees");
	}

	public void CallCmdFinishedWithCoin(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFinishedWithCoin called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFinishedWithCoin(netSurrogateId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFinishedWithCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendCommandInternal(networkWriter, 0, "CmdFinishedWithCoin");
	}

	public void CallCmdDroppedCoin(NetworkInstanceId netSurrogateId, Vector2 coinPosition, bool returnToCoinSpawn)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDroppedCoin called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDroppedCoin(netSurrogateId, coinPosition, returnToCoinSpawn);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDroppedCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		networkWriter.Write(coinPosition);
		networkWriter.Write(returnToCoinSpawn);
		SendCommandInternal(networkWriter, 0, "CmdDroppedCoin");
	}

	public void CallCmdRequestGrabJetpack(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestGrabJetpack called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestGrabJetpack(netSurrogateId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestGrabJetpack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendCommandInternal(networkWriter, 0, "CmdRequestGrabJetpack");
	}

	public void CallCmdSetLastFlagID(int id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLastFlagID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLastFlagID(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLastFlagID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		SendCommandInternal(networkWriter, 0, "CmdSetLastFlagID");
	}

	public void CallCmdEnableAFKWarning(float timeLeft)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEnableAFKWarning called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEnableAFKWarning(timeLeft);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEnableAFKWarning);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(timeLeft);
		SendCommandInternal(networkWriter, 0, "CmdEnableAFKWarning");
	}

	public void CallCmdDisableAFKWarning()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDisableAFKWarning called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDisableAFKWarning();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDisableAFKWarning);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdDisableAFKWarning");
	}

	public void CallCmdSetWantsToRetry(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetWantsToRetry called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetWantsToRetry(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetWantsToRetry);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetWantsToRetry");
	}

	public void CallCmdComeBackAsGhost()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdComeBackAsGhost called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdComeBackAsGhost();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdComeBackAsGhost);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdComeBackAsGhost");
	}

	public void CallCmdSpawnExtraCorpse()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnExtraCorpse called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnExtraCorpse();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnExtraCorpse);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSpawnExtraCorpse");
	}

	public void CallCmdComeBackAsZombie()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdComeBackAsZombie called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdComeBackAsZombie();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdComeBackAsZombie);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdComeBackAsZombie");
	}

	public void CallCmdLoseLife()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdLoseLife called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdLoseLife();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdLoseLife);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdLoseLife");
	}

	public void CallCmdRespawn()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRespawn called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRespawn();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRespawn);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRespawn");
	}

	public void CallCmdAgonize()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAgonize called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAgonize();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAgonize);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdAgonize");
	}

	public void CallCmdIShouldBeKicked(LobbyManager.KickReasons kickReason)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdIShouldBeKicked called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdIShouldBeKicked(kickReason);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdIShouldBeKicked);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)kickReason);
		SendCommandInternal(networkWriter, 0, "CmdIShouldBeKicked");
	}

	public void CallCmdSetRemoteJetpackState(bool onOff)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetRemoteJetpackState called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetRemoteJetpackState(onOff);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetRemoteJetpackState);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(onOff);
		SendCommandInternal(networkWriter, 0, "CmdSetRemoteJetpackState");
	}

	protected static void InvokeRpcRpcEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnable called on server.");
		}
		else
		{
			((Character)obj).RpcEnable(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcDisable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisable called on server.");
		}
		else
		{
			((Character)obj).RpcDisable(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcShowprite(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowprite called on server.");
		}
		else
		{
			((Character)obj).RpcShowprite(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcFreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFreeze called on server.");
		}
		else
		{
			((Character)obj).RpcFreeze();
		}
	}

	protected static void InvokeRpcRpcCommunicateOutfitsArray(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCommunicateOutfitsArray called on server.");
		}
		else
		{
			((Character)obj).RpcCommunicateOutfitsArray(GeneratedNetworkCode._ReadArrayInt32_None(reader));
		}
	}

	protected static void InvokeRpcRpcSetupDeath(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetupDeath called on server.");
		}
		else
		{
			((Character)obj).RpcSetupDeath(reader.ReadString(), reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcFindLocalPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFindLocalPlayer called on server.");
		}
		else
		{
			((Character)obj).RpcFindLocalPlayer((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcGetLocalController(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGetLocalController called on server.");
		}
		else
		{
			((Character)obj).RpcGetLocalController((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcSendPlayerSuccessEvent(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendPlayerSuccessEvent called on server.");
		}
		else
		{
			((Character)obj).RpcSendPlayerSuccessEvent();
		}
	}

	protected static void InvokeRpcRpcSetLobbyCollider(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetLobbyCollider called on server.");
		}
		else
		{
			((Character)obj).RpcSetLobbyCollider(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSetRaycastsEnabled(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetRaycastsEnabled called on server.");
		}
		else
		{
			((Character)obj).RpcSetRaycastsEnabled(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSetReady(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetReady called on server.");
		}
		else
		{
			((Character)obj).RpcSetReady(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSwitchFreeMode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSwitchFreeMode called on server.");
		}
		else
		{
			((Character)obj).RpcSwitchFreeMode();
		}
	}

	protected static void InvokeRpcRpcAnimatorTrigger(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnimatorTrigger called on server.");
		}
		else
		{
			((Character)obj).RpcAnimatorTrigger((AnimParam)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcSetAnimatorInt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetAnimatorInt called on server.");
		}
		else
		{
			((Character)obj).RpcSetAnimatorInt((AnimParam)reader.ReadInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcSetBubble(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetBubble called on server.");
		}
		else
		{
			((Character)obj).RpcSetBubble((int)reader.ReadPackedUInt32(), reader.ReadBoolean(), reader.ReadColor());
		}
	}

	protected static void InvokeRpcRpcAudioEvent(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAudioEvent called on server.");
		}
		else
		{
			((Character)obj).RpcAudioEvent(reader.ReadString(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcAudioEventExact(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAudioEventExact called on server.");
		}
		else
		{
			((Character)obj).RpcAudioEventExact(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcSetAnimatorFloat(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetAnimatorFloat called on server.");
		}
		else
		{
			((Character)obj).RpcSetAnimatorFloat((AnimParam)reader.ReadInt32(), reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcSpawnDustCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnDustCloud called on server.");
		}
		else
		{
			((Character)obj).RpcSpawnDustCloud(reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcSpawnJumpCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnJumpCloud called on server.");
		}
		else
		{
			((Character)obj).RpcSpawnJumpCloud(reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSpawnWallJumpCloud(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnWallJumpCloud called on server.");
		}
		else
		{
			((Character)obj).RpcSpawnWallJumpCloud(reader.ReadBoolean(), reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcSpawnHoneyStickers(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnHoneyStickers called on server.");
		}
		else
		{
			((Character)obj).RpcSpawnHoneyStickers();
		}
	}

	protected static void InvokeRpcRpcPositionCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPositionCharacter called on server.");
		}
		else
		{
			((Character)obj).RpcPositionCharacter(reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcGrantCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGrantCoin called on server.");
		}
		else
		{
			((Character)obj).RpcGrantCoin(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcGrantBees(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGrantBees called on server.");
		}
		else
		{
			((Character)obj).RpcGrantBees(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcFinishedWithCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFinishedWithCoin called on server.");
		}
		else
		{
			((Character)obj).RpcFinishedWithCoin(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcDroppedCoin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDroppedCoin called on server.");
		}
		else
		{
			((Character)obj).RpcDroppedCoin(reader.ReadNetworkId(), reader.ReadVector2(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSetJetPackTouched(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetJetPackTouched called on server.");
		}
		else
		{
			((Character)obj).RpcSetJetPackTouched(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcGrantJetpack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGrantJetpack called on server.");
		}
		else
		{
			((Character)obj).RpcGrantJetpack(reader.ReadNetworkId());
		}
	}

	protected static void InvokeRpcRpcEnableAFKWarning(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnableAFKWarning called on server.");
		}
		else
		{
			((Character)obj).RpcEnableAFKWarning(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcDisableAFKWarning(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisableAFKWarning called on server.");
		}
		else
		{
			((Character)obj).RpcDisableAFKWarning();
		}
	}

	protected static void InvokeRpcRpcComeBackAsGhost(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcComeBackAsGhost called on server.");
		}
		else
		{
			((Character)obj).RpcComeBackAsGhost();
		}
	}

	protected static void InvokeRpcRpcOnExtraCorpseSpawned(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnExtraCorpseSpawned called on server.");
		}
		else
		{
			((Character)obj).RpcOnExtraCorpseSpawned();
		}
	}

	protected static void InvokeRpcRpcComeBackAsZombie(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcComeBackAsZombie called on server.");
		}
		else
		{
			((Character)obj).RpcComeBackAsZombie();
		}
	}

	protected static void InvokeRpcRpcLoseLife(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLoseLife called on server.");
		}
		else
		{
			((Character)obj).RpcLoseLife();
		}
	}

	protected static void InvokeRpcRpcRespawn(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRespawn called on server.");
		}
		else
		{
			((Character)obj).RpcRespawn();
		}
	}

	protected static void InvokeRpcRpcAgonize(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAgonize called on server.");
		}
		else
		{
			((Character)obj).RpcAgonize();
		}
	}

	protected static void InvokeRpcRpcSetRemoteJetpackState(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetRemoteJetpackState called on server.");
		}
		else
		{
			((Character)obj).RpcSetRemoteJetpackState(reader.ReadBoolean());
		}
	}

	public void CallRpcEnable(bool playsound)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcEnable called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcEnable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(playsound);
		SendRPCInternal(networkWriter, 0, "RpcEnable");
	}

	public void CallRpcDisable(bool moveAway)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDisable called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDisable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(moveAway);
		SendRPCInternal(networkWriter, 0, "RpcDisable");
	}

	public void CallRpcShowprite(bool show)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowprite called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowprite);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(show);
		SendRPCInternal(networkWriter, 0, "RpcShowprite");
	}

	public void CallRpcFreeze()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFreeze called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcFreeze");
	}

	public void CallRpcCommunicateOutfitsArray(int[] outfitsArray)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcCommunicateOutfitsArray called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcCommunicateOutfitsArray);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, outfitsArray);
		SendRPCInternal(networkWriter, 0, "RpcCommunicateOutfitsArray");
	}

	public void CallRpcSetupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetupDeath called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetupDeath);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(cause);
		networkWriter.Write(deathFreezeOn);
		networkWriter.WritePackedUInt32((uint)causedByPlayerNumber);
		SendRPCInternal(networkWriter, 0, "RpcSetupDeath");
	}

	public void CallRpcFindLocalPlayer(int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFindLocalPlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFindLocalPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcFindLocalPlayer");
	}

	public void CallRpcGetLocalController(int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcGetLocalController called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcGetLocalController);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcGetLocalController");
	}

	public void CallRpcSendPlayerSuccessEvent()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSendPlayerSuccessEvent called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSendPlayerSuccessEvent);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcSendPlayerSuccessEvent");
	}

	public void CallRpcSetLobbyCollider(bool enable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetLobbyCollider called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetLobbyCollider);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(enable);
		SendRPCInternal(networkWriter, 0, "RpcSetLobbyCollider");
	}

	public void CallRpcSetRaycastsEnabled(bool enabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetRaycastsEnabled called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetRaycastsEnabled);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(enabled);
		SendRPCInternal(networkWriter, 0, "RpcSetRaycastsEnabled");
	}

	public void CallRpcSetReady(bool ready)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetReady called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetReady);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ready);
		SendRPCInternal(networkWriter, 0, "RpcSetReady");
	}

	public void CallRpcSwitchFreeMode()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSwitchFreeMode called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSwitchFreeMode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcSwitchFreeMode");
	}

	public void CallRpcAnimatorTrigger(AnimParam triggerName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAnimatorTrigger called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAnimatorTrigger);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)triggerName);
		SendRPCInternal(networkWriter, 1, "RpcAnimatorTrigger");
	}

	public void CallRpcSetAnimatorInt(AnimParam paramName, int value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetAnimatorInt called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetAnimatorInt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)paramName);
		networkWriter.WritePackedUInt32((uint)value);
		SendRPCInternal(networkWriter, 1, "RpcSetAnimatorInt");
	}

	public void CallRpcSetBubble(int animId, bool value, Color color)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetBubble called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetBubble);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)animId);
		networkWriter.Write(value);
		networkWriter.Write(color);
		SendRPCInternal(networkWriter, 1, "RpcSetBubble");
	}

	public void CallRpcAudioEvent(string audioEventName, bool ignoreGhostZombie)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAudioEvent called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAudioEvent);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(audioEventName);
		networkWriter.Write(ignoreGhostZombie);
		SendRPCInternal(networkWriter, 1, "RpcAudioEvent");
	}

	public void CallRpcAudioEventExact(string audioEventName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAudioEventExact called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAudioEventExact);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(audioEventName);
		SendRPCInternal(networkWriter, 1, "RpcAudioEventExact");
	}

	public void CallRpcSetAnimatorFloat(AnimParam paramName, float value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetAnimatorFloat called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetAnimatorFloat);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)paramName);
		networkWriter.Write(value);
		SendRPCInternal(networkWriter, 1, "RpcSetAnimatorFloat");
	}

	public void CallRpcSpawnDustCloud(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnDustCloud called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnDustCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		SendRPCInternal(networkWriter, 1, "RpcSpawnDustCloud");
	}

	public void CallRpcSpawnJumpCloud(Vector3 position, bool multiJump)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnJumpCloud called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnJumpCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(multiJump);
		SendRPCInternal(networkWriter, 1, "RpcSpawnJumpCloud");
	}

	public void CallRpcSpawnWallJumpCloud(bool right, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnWallJumpCloud called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnWallJumpCloud);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(right);
		networkWriter.Write(position);
		SendRPCInternal(networkWriter, 1, "RpcSpawnWallJumpCloud");
	}

	public void CallRpcSpawnHoneyStickers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnHoneyStickers called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnHoneyStickers);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 1, "RpcSpawnHoneyStickers");
	}

	public void CallRpcPositionCharacter(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPositionCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPositionCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		SendRPCInternal(networkWriter, 0, "RpcPositionCharacter");
	}

	public void CallRpcGrantCoin(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcGrantCoin called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcGrantCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendRPCInternal(networkWriter, 0, "RpcGrantCoin");
	}

	public void CallRpcGrantBees(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcGrantBees called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcGrantBees);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendRPCInternal(networkWriter, 0, "RpcGrantBees");
	}

	public void CallRpcFinishedWithCoin(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFinishedWithCoin called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFinishedWithCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendRPCInternal(networkWriter, 0, "RpcFinishedWithCoin");
	}

	public void CallRpcDroppedCoin(NetworkInstanceId netSurrogateId, Vector2 coinPosition, bool returnToCoinSpawn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDroppedCoin called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDroppedCoin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		networkWriter.Write(coinPosition);
		networkWriter.Write(returnToCoinSpawn);
		SendRPCInternal(networkWriter, 0, "RpcDroppedCoin");
	}

	public void CallRpcSetJetPackTouched(bool value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetJetPackTouched called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetJetPackTouched);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendRPCInternal(networkWriter, 0, "RpcSetJetPackTouched");
	}

	public void CallRpcGrantJetpack(NetworkInstanceId netSurrogateId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcGrantJetpack called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcGrantJetpack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(netSurrogateId);
		SendRPCInternal(networkWriter, 0, "RpcGrantJetpack");
	}

	public void CallRpcEnableAFKWarning(float timeLeft)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcEnableAFKWarning called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcEnableAFKWarning);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(timeLeft);
		SendRPCInternal(networkWriter, 0, "RpcEnableAFKWarning");
	}

	public void CallRpcDisableAFKWarning()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDisableAFKWarning called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDisableAFKWarning);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcDisableAFKWarning");
	}

	public void CallRpcComeBackAsGhost()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcComeBackAsGhost called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcComeBackAsGhost);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcComeBackAsGhost");
	}

	public void CallRpcOnExtraCorpseSpawned()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcOnExtraCorpseSpawned called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcOnExtraCorpseSpawned);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcOnExtraCorpseSpawned");
	}

	public void CallRpcComeBackAsZombie()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcComeBackAsZombie called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcComeBackAsZombie);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcComeBackAsZombie");
	}

	public void CallRpcLoseLife()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLoseLife called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLoseLife);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcLoseLife");
	}

	public void CallRpcRespawn()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRespawn called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRespawn);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRespawn");
	}

	public void CallRpcAgonize()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAgonize called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAgonize);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcAgonize");
	}

	public void CallRpcSetRemoteJetpackState(bool onOff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetRemoteJetpackState called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetRemoteJetpackState);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(onOff);
		SendRPCInternal(networkWriter, 0, "RpcSetRemoteJetpackState");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)networkNumber);
			writer.WritePackedUInt32((uint)localNumber);
			writer.Write(picked);
			writer.Write(FindPlayerOnSpawn);
			writer.WritePackedUInt32((uint)flipSpriteX);
			writer.Write((int)currentAnim);
			writer.Write((int)secondaryAnim);
			writer.Write(isExtraCorpse);
			writer.Write((int)CharacterSprite);
			writer.Write(playerColor);
			writer.WritePackedUInt32((uint)LastFlagID);
			writer.Write(WantsToRetry);
			writer.Write(onGround);
			writer.Write(dying);
			writer.Write(dead);
			writer.Write(success);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)networkNumber);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)localNumber);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(picked);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(FindPlayerOnSpawn);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)flipSpriteX);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)currentAnim);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)secondaryAnim);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isExtraCorpse);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)CharacterSprite);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(playerColor);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)LastFlagID);
		}
		if ((base.syncVarDirtyBits & 0x800) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(WantsToRetry);
		}
		if ((base.syncVarDirtyBits & 0x1000) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(onGround);
		}
		if ((base.syncVarDirtyBits & 0x2000) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(dying);
		}
		if ((base.syncVarDirtyBits & 0x4000) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(dead);
		}
		if ((base.syncVarDirtyBits & 0x8000) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(success);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			networkNumber = (int)reader.ReadPackedUInt32();
			localNumber = (int)reader.ReadPackedUInt32();
			picked = reader.ReadBoolean();
			FindPlayerOnSpawn = reader.ReadBoolean();
			flipSpriteX = (int)reader.ReadPackedUInt32();
			currentAnim = (AnimState)reader.ReadInt32();
			secondaryAnim = (SecondaryAnimState)reader.ReadInt32();
			isExtraCorpse = reader.ReadBoolean();
			CharacterSprite = (Animals)reader.ReadInt32();
			playerColor = reader.ReadColor();
			LastFlagID = (int)reader.ReadPackedUInt32();
			WantsToRetry = reader.ReadBoolean();
			onGround = reader.ReadBoolean();
			dying = reader.ReadBoolean();
			dead = reader.ReadBoolean();
			success = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			networkNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			localNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			picked = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			FindPlayerOnSpawn = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			flipSpriteX = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x20) != 0)
		{
			currentAnim = (AnimState)reader.ReadInt32();
		}
		if ((num & 0x40) != 0)
		{
			secondaryAnim = (SecondaryAnimState)reader.ReadInt32();
		}
		if ((num & 0x80) != 0)
		{
			isExtraCorpse = reader.ReadBoolean();
		}
		if ((num & 0x100) != 0)
		{
			CharacterSprite = (Animals)reader.ReadInt32();
		}
		if ((num & 0x200) != 0)
		{
			playerColor = reader.ReadColor();
		}
		if ((num & 0x400) != 0)
		{
			LastFlagID = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x800) != 0)
		{
			WantsToRetry = reader.ReadBoolean();
		}
		if ((num & 0x1000) != 0)
		{
			onGround = reader.ReadBoolean();
		}
		if ((num & 0x2000) != 0)
		{
			dying = reader.ReadBoolean();
		}
		if ((num & 0x4000) != 0)
		{
			dead = reader.ReadBoolean();
		}
		if ((num & 0x8000) != 0)
		{
			success = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
