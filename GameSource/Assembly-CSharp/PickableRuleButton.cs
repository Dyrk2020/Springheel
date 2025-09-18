using UnityEngine;
using UnityEngine.UI;

public class PickableRuleButton : PickableButton
{
	public enum ButtonJobs
	{
		GAMEMODE = 0,
		ROUNDLIMITER = 1,
		ROUNDTYPETEXT = 2,
		ROUNDNUMBER = 3,
		ADDROUNDTIME = 4,
		SUBTRACTROUNDTIME = 5,
		ADDPOINTS = 6,
		SUBTRACTPOINTS = 7,
		POINTNUMBER = 8,
		RESETDEFAULT = 9,
		SPECIALPOINTSTOGGLE = 10,
		NONECHANGINGTEXT = 11,
		HOSTCONTROLSGAMERULES = 12,
		ADDPLACETIME = 13,
		SUBTRACTPLACETIME = 14,
		PLACETIMENUMBER = 15,
		LOCKGAMEMODE = 16,
		ADDCREATIVEPIECES = 17,
		SUBTRACTCREATIVEPIECES = 18,
		CREATIVEPIECESNUMBER = 19,
		NONEFREEPLAYRULETEXT = 20,
		CREATIVEPIECEPERROUNDTEXT = 21,
		FREEPLAYHASNOTRULES = 22,
		CHALLENGEMODETEXT = 23,
		DOUBLEPARTYBOXTEXT = 24,
		DOUBLEPARTYBOXTOGGLE = 25,
		ADDRUNTIMERLIMIT = 26,
		SUBTRACTRUNTIMERLIMIT = 27,
		RUNTIMERLIMITNUMBER = 28,
		POINTVALUEDECREASE = 29,
		POINTVALUEINCREASE = 30,
		POINTALWAYSAWARD = 31,
		POINTSRESETTODEFAULT = 32,
		CURRENTPRESET = 33,
		PREVPRESET = 34,
		NEXTPRESET = 35,
		DELETEPRESET = 36,
		NONE = 99999
	}

	public ButtonJobs job;

	public PointBlock.pointBlockType PointType;

	public Text TooltipText;

	public static LevelSelectController levelSelectController;

	protected void Show(bool show)
	{
		if ((bool)buttonText)
		{
			buttonText.enabled = show;
		}
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if ((bool)image)
		{
			image.enabled = show;
		}
		if ((bool)sprite)
		{
			sprite.enabled = show;
		}
	}
}
