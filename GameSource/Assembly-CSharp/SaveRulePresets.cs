using UnityEngine;
using UnityEngine.UI;

public class SaveRulePresets : MonoBehaviour
{
	public InputField NameInput;

	public InputField DescriptionInput;

	public Image RuleCheck;

	public Image PointCheck;

	public Image BlockCheck;

	protected string presetName;

	protected string presetDescription;

	private bool saveRules = true;

	private bool savePoints = true;

	private bool saveBlocks = true;

	public void ActivateNameInputField(PickCursor cursor)
	{
		Controller.LockInputField(NameInput, delegate(string str)
		{
			PickableButton.ResetMasks();
			SetPresetName(str);
		});
	}

	public void ActivateDescriptionInputField(PickCursor cursor)
	{
		Controller.LockInputField(DescriptionInput, delegate(string str)
		{
			PickableButton.ResetMasks();
			SetPresetDescription(str);
		});
	}

	public void SetPresetName(string name)
	{
		presetName = name;
	}

	public void SetPresetDescription(string description)
	{
		presetDescription = description;
	}

	public void ToggleSaveRules()
	{
		saveRules = !saveRules;
		RuleCheck.enabled = saveRules;
	}

	public void ToggleSavePoints()
	{
		savePoints = !savePoints;
		PointCheck.enabled = savePoints;
	}

	public void ToggleSaveBlocks()
	{
		saveBlocks = !saveBlocks;
		BlockCheck.enabled = saveBlocks;
	}

	public void SaveRules()
	{
		GameRulePreset.SaveRules(NameInput.text, DescriptionInput.text, delegate(string str)
		{
			NameInput.text = str;
		}, delegate
		{
		});
	}
}
