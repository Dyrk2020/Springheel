using UnityEngine;
using UnityEngine.UI;

public class GetCodeToggleButton : PickableButton
{
	public GetCodeToggleGroup toggleGroup;

	public int toggleValue;

	public Image bgImage;

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		toggleGroup.OnClickToggleButton(this);
	}

	public void SetBGColor(Color color)
	{
		if (bgImage != null)
		{
			bgImage.color = color;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (tipContainer != null)
		{
			ShowTip(HoveredCursors.Count > 0);
		}
	}
}
