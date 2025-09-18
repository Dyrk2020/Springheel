using UnityEngine;

public class ControllerMonitor : MonoBehaviour
{
	public class JoinedControllerEntry
	{
		public int consolePadSlot;

		public Controller controller;
	}

	public JoinedControllerEntry mainController;

	private static ControllerMonitor instance;

	public static ControllerMonitor Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("ControllerMonitor").AddComponent<ControllerMonitor>();
			}
			return instance;
		}
	}

	public bool IsMainControllerSet => mainController != null;

	public int MainControllerPadIndex
	{
		get
		{
			if (IsMainControllerSet)
			{
				return mainController.consolePadSlot;
			}
			return -1;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		_ = instance == this;
	}

	public void SetMainMenuController(Controller controller)
	{
		mainController = new JoinedControllerEntry
		{
			controller = controller
		};
		OnMainControllerChanged();
	}

	public void ClearAllJoinedControllers()
	{
		if (mainController != null)
		{
			mainController = null;
			StatTracker.Instance.OnMainControllerRemoved();
			OnMainControllerChanged();
			if (RamFS.PlatformUsesRamFS)
			{
				RamFS.AddUnmountOperation(null);
			}
		}
	}

	public bool IsMainController(Controller controller)
	{
		if (mainController != null)
		{
			return mainController.controller == controller;
		}
		return false;
	}

	private void OnMainControllerChanged()
	{
		GameSparksManager.Instance.OnMainControllerChanged();
	}
}
