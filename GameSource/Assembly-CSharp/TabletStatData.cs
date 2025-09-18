using I2.Loc;
using UnityEngine;

public class TabletStatData : MonoBehaviour
{
	public Character.Animals animalType;

	public GameState.LevelName levelType;

	public TabletTextLabel DataName;

	public Localize NameLocalization;

	public LocalizationFontSizeSwitcher fontsizeSwitcher;

	public TabletTextLabel DataSlots1;

	public TabletTextLabel DataSlots2;

	public TabletTextLabel DataSlots3;

	public void SetStyle(TabletColorScheme colorScheme)
	{
		DataName.colorScheme = colorScheme;
		DataSlots1.colorScheme = colorScheme;
		DataSlots2.colorScheme = colorScheme;
		DataSlots3.colorScheme = colorScheme;
	}
}
