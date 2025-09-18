using I2.Loc;
using UnityEngine.UI;

public class LastTurnMessage : UIGraphic
{
	public bool ShowSuddenDeath;

	public Text LastTurn;

	public Text LastTurnShadow;

	public Text SuddenDeath;

	public Text SuddenDeathShadow;

	public override void Show()
	{
		base.Show();
		LastTurnShadow.enabled = true;
		LastTurn.enabled = true;
		if (ShowSuddenDeath)
		{
			SuddenDeathShadow.enabled = true;
			SuddenDeath.enabled = true;
		}
	}

	public void ShowTurns(int turnsRemaining)
	{
		base.Show();
		string text = null;
		switch (turnsRemaining)
		{
		case 3:
			text = ScriptLocalization.InGameText._3_Turns_Left;
			break;
		case 2:
			text = ScriptLocalization.InGameText._2_Turns_Left;
			break;
		case 1:
			text = ScriptLocalization.InGameText.Last_Turn;
			break;
		case 0:
			text = "";
			break;
		case -1:
			text = ScriptLocalization.InGameText.Tied;
			break;
		}
		LastTurn.text = text;
		LastTurnShadow.text = text;
		LastTurnShadow.enabled = true;
		LastTurn.enabled = true;
		if (ShowSuddenDeath)
		{
			SuddenDeathShadow.enabled = true;
			SuddenDeath.enabled = true;
		}
		else
		{
			SuddenDeathShadow.enabled = false;
			SuddenDeath.enabled = false;
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		LastTurnShadow.enabled = false;
		LastTurn.enabled = false;
		SuddenDeathShadow.enabled = false;
		SuddenDeath.enabled = false;
	}
}
