public interface TabletClickable
{
	bool Disabled { get; }

	bool Interactable { get; }

	bool TracksCursors { get; }

	void OnAccept(PickCursor cursor);

	void OnCursorOver();

	void OnCursorOut();

	void AddTrackedCursor(PickCursor pickCursor);

	void RemoveTrackedCursor(PickCursor pickCursor);
}
