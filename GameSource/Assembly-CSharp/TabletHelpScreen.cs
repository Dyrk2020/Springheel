using GameEvent;
using UnityEngine;

public class TabletHelpScreen : TabletScreen, IGameEventListener
{
	public TabletSubdialogController subdialogController;

	public RectTransform optionsDialog;

	private RectTransform lastEnteredSubdialog;

	public TabletControllerDiagram helpDiagramController;

	private void Awake()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void JumpToHelpPage(int pageIndex)
	{
		helpDiagramController.SetCurrentPageIndex(pageIndex);
	}

	public override void OnTransitionOutBegin()
	{
		base.OnTransitionOutBegin();
		helpDiagramController.ResetToFirstPage();
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		_ = e is LanguageChangeEvent;
	}

	public override void OnModalOverlayClosed()
	{
		base.OnModalOverlayClosed();
		UpdateButtonValue(tablet.modalOverlay.currentOverlayType);
	}

	private void UpdateButtonValue(TabletRule overlayType)
	{
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (tablet.modalOverlay.IsOpen || tablet.modalOverlay.IsOpening)
		{
			tablet.modalOverlay.OnCancel();
			return true;
		}
		if (!subdialogController.IsOnMainSubdialog)
		{
			subdialogController.PopSubdialog();
			return true;
		}
		return base.OnPressBack(pickCursor);
	}

	public override bool OnRotateRight(PickCursor pickCursor)
	{
		helpDiagramController.OnClickNext(pickCursor);
		return true;
	}

	public override bool OnRotateLeft(PickCursor pickCursor)
	{
		helpDiagramController.OnClickPrev(pickCursor);
		return true;
	}

	public override void Update()
	{
		base.Update();
	}
}
