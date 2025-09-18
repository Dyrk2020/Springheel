using UnityEngine;
using UnityEngine.UI;

public class ReportReason : MonoBehaviour
{
	public enum Reason
	{
		InappropriateTitle,
		InappropriateContent,
		Spam,
		PersonalInformation,
		Copyright,
		BrokenLevel,
		Custom
	}

	public Reason reason;

	public Image checkMarkImage;

	public void SetChecked(bool value)
	{
		if (value)
		{
			checkMarkImage.enabled = true;
		}
		else
		{
			checkMarkImage.enabled = false;
		}
	}
}
