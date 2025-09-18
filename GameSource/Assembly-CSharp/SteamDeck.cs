using Steamworks;
using UnityEngine;

public class SteamDeck : MonoBehaviour
{
	public static void OpenVirtualKeyboard(Cursor cursor, int x = 0, int y = 0, int width = 10, int height = 10)
	{
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, x, y, width, height);
			if (cursor != null)
			{
				cursor.IgnoreNextBackDown = true;
			}
		}
	}
}
