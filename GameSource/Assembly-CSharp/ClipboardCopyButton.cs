using UnityEngine;
using UnityEngine.UI;

public class ClipboardCopyButton : PickableButton
{
	public Text textToCopy;

	public bool adminOnly;

	protected override void Start()
	{
		if (adminOnly && GameSparksManager.Instance.MainUserIsAdmin)
		{
			base.Start();
			Enable();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		QuickSaver.CopyStringToClipboard(textToCopy.text);
		UserMessageManager.Instance.UserMessage("Copied text to clipboard");
	}
}
