using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class WinMessage : UIGraphic
{
	public Text characterNameText;

	public Text characterNameTextShadow;

	public Text ultimateText;

	public Text ultimateTextShadow;

	public Text youreTheText;

	public Text youreTheTexShadow;

	public Vector3 StandardPositionUltimate;

	public Vector3 OtherPositonUltimate;

	public Vector3 StandardPositionAnimal;

	public Vector3 OtherPositionAnimal;

	public void SetWinnerNameSprite(string characterName, Character.Animals animal, bool isWearingSkin)
	{
		characterNameText.text = characterName;
		characterNameTextShadow.text = characterName;
		string currentLanguageCode = LocalizationManager.CurrentLanguageCode;
		if (currentLanguageCode.StartsWith("es") || currentLanguageCode.StartsWith("fr") || currentLanguageCode.StartsWith("pt") || currentLanguageCode.StartsWith("it"))
		{
			SetWordPositions(OtherPositonUltimate, OtherPositionAnimal);
		}
		else
		{
			SetWordPositions(StandardPositionUltimate, StandardPositionAnimal);
		}
		string text = ScriptLocalization.InGameText.victory_message_Line_2;
		if (currentLanguageCode.StartsWith("es") && (animal == Character.Animals.SHEEP || animal == Character.Animals.SQUIRREL))
		{
			text = ScriptLocalization.InGameText.victory_message_Line_2_Other_Gender;
		}
		if (currentLanguageCode.StartsWith("cs") && animal == Character.Animals.FOX)
		{
			text = ScriptLocalization.InGameText.victory_message_Line_2_Other_Gender;
		}
		if (currentLanguageCode.StartsWith("pl") && (animal == Character.Animals.SHEEP || animal == Character.Animals.SQUIRREL))
		{
			text = ScriptLocalization.InGameText.victory_message_Line_2_Other_Gender;
		}
		string text2 = ScriptLocalization.InGameText.victory_message_Line_1;
		if (currentLanguageCode.StartsWith("de"))
		{
			if (animal == Character.Animals.ROBOT || animal == Character.Animals.RACCOON || animal == Character.Animals.MONKEY || animal == Character.Animals.PANDA || animal == Character.Animals.FOX)
			{
				text2 = ScriptLocalization.InGameText.victory_message_Line_1_Alt_Gender;
			}
			if (animal == Character.Animals.ELEPHANT)
			{
				text2 = ((!isWearingSkin) ? ScriptLocalization.InGameText.victory_message_Line_1_Alt_Gender : ScriptLocalization.InGameText.victory_message_Line_1);
			}
			if (animal == Character.Animals.PLATYPUS)
			{
				text2 = ((!isWearingSkin) ? ScriptLocalization.InGameText.victory_message_Line_1 : ScriptLocalization.InGameText.victory_message_Line_1_Alt_Gender);
			}
			if (animal == Character.Animals.SNAKE)
			{
				text2 = ScriptLocalization.InGameText.victory_message_Line_1_Neutral_Gender;
			}
		}
		if (currentLanguageCode.StartsWith("it") && animal == Character.Animals.FOX)
		{
			text2 = ScriptLocalization.InGameText.victory_message_Line_1_Alt_Gender;
		}
		if (currentLanguageCode.StartsWith("pt-BR") && (animal == Character.Animals.CHICKEN || animal == Character.Animals.SHEEP || animal == Character.Animals.SNAKE || animal == Character.Animals.FOX))
		{
			text2 = ScriptLocalization.InGameText.victory_message_Line_1_Alt_Gender;
		}
		ultimateTextShadow.text = text;
		ultimateText.text = text;
		youreTheText.text = text2;
		youreTheTexShadow.text = text2;
	}

	public void SetWordPositions(Vector3 Ultimate, Vector3 Animals)
	{
		characterNameText.rectTransform.localPosition = Animals;
		characterNameTextShadow.rectTransform.localPosition = Animals + new Vector3(0.6f, -0.6f, 0f);
		ultimateText.rectTransform.localPosition = Ultimate;
		ultimateTextShadow.rectTransform.localPosition = Ultimate + new Vector3(0.6f, -0.6f, 0f);
	}
}
