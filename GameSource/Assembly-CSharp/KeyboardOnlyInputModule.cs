using UnityEngine.EventSystems;

internal class KeyboardOnlyInputModule : StandaloneInputModule
{
	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		if (Controller.InputFieldIsActive)
		{
			ProcessMouseEvent();
		}
	}
}
