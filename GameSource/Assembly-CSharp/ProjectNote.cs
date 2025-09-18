using UnityEngine;

[CreateAssetMenu(fileName = "New Project Note", menuName = "Development/Project Note", order = 100)]
public class ProjectNote : ScriptableObject
{
	public enum NoteType
	{
		Info,
		TODO,
		ArchitecturePlan,
		DesignIdea,
		BugContext
	}

	[Tooltip("Categorize this note for easier filtering in the Project window.")]
	[Header("Metadata")]
	public NoteType noteType;

	[Tooltip("Who left this note?")]
	public string author = "Kyler";

	[Space(10f)]
	[Header("Note Content")]
	[TextArea(15, 50)]
	public string content;
}
