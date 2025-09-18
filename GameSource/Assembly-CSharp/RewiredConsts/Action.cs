using Rewired.Dev;

namespace RewiredConsts;

public static class Action
{
	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action0")]
	public const int UpButton = 0;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action1")]
	public const int DownButton = 1;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action2")]
	public const int LeftButton = 2;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action3")]
	public const int RightButton = 3;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action4")]
	public const int RotateLeft = 4;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action5")]
	public const int RotateRight = 5;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action6")]
	public const int Pause = 6;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action7")]
	public const int Scoreboard = 7;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action8")]
	public const int LT = 8;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action9")]
	public const int RT = 9;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action0")]
	public const int MoveHorizontal = 10;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action1")]
	public const int MoveVertical = 11;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action2")]
	public const int LookHorizontal = 12;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action3")]
	public const int LookVertical = 13;
}
