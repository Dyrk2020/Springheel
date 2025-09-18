using System.Collections.Generic;

public interface IPickable
{
	List<Cursor> IHoveredCursors { get; set; }

	bool Paused { get; }

	bool DeactivatedInBook { get; }

	int PageNumber { get; set; }

	string SFXEventName { get; }

	PickableBlock ThisPickableBlock { get; }

	SortOrder SpriteSortOrder { get; }

	string Name { get; }

	uint netIdValue { get; }

	InventoryBook InventoryBook { get; set; }

	void OnAccept(PickCursor pickCursor);

	void PlayHoverSound();

	void Enable();

	void Enable(bool enable);

	void Disable();

	void SetTextCanvasOrder(int num);
}
