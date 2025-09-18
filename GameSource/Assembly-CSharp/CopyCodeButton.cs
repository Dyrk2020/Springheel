using I2.Loc;
using UnityEngine;

public class CopyCodeButton : PickableButton
{
	public string snapshotName;

	public string snapshotCode;

	public ChallengeScoreboard challengeScoreboard;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public void DisableCollidersOnConsole()
	{
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = false;
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (!buttonText.text.NullOrEmpty())
		{
			TextEditor textEditor = new TextEditor();
			textEditor.text = buttonText.text;
			textEditor.SelectAll();
			textEditor.Copy();
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
		}
	}

	public void SetSnapshot(string name, string code)
	{
		snapshotCode = code;
	}
}
