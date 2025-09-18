using UnityEngine;

public class FeaturedSpecialFilter : MonoBehaviour
{
	public enum SpecialFilterType
	{
		Local,
		Recent,
		Favourites
	}

	public SpecialFilterType type;
}
