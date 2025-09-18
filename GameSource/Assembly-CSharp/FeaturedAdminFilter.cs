using UnityEngine;

public class FeaturedAdminFilter : MonoBehaviour
{
	public enum FilterType
	{
		Flagged
	}

	public FilterType filterType;

	public int minimumPermissionLevel = 10;

	public FeaturedQuickFilter.SortingFilter sortingFilter;
}
