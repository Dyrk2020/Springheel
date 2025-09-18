using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeaturedQuickFilter : MonoBehaviour
{
	public enum FilterTypes
	{
		Featured,
		Trending,
		Sorted,
		Local,
		Recent,
		Favourites
	}

	public enum LevelTypes
	{
		Versus,
		Challenge,
		Any
	}

	public enum InfoLineTypes
	{
		None,
		Author,
		TimeSinceCreation,
		LevelType,
		Difficulty,
		Points,
		PlayCount
	}

	[Serializable]
	public class SortingFilter
	{
		public FilterTypes filterType = FilterTypes.Sorted;

		public string sortBy = "";

		public bool descending;

		[HideInInspector]
		public LevelTypes levelType;

		public string restrictToUserId;

		public string restrictToGSID;

		[HideInInspector]
		public bool allowUnpublished;

		[HideInInspector]
		public int cutoffDays;

		public List<string> codeList = new List<string>();

		public float lowerDifficultyBound;

		public float upperDifficultyBound = 1f;

		public string searchTerms;

		public InfoLineTypes infoLine1 = InfoLineTypes.Author;

		public InfoLineTypes infoLine2 = InfoLineTypes.TimeSinceCreation;

		public int approvalStatusFilter = -1;

		public bool hideAcknowledged;

		public int showMods;

		public bool IsSpecialFilterType
		{
			get
			{
				FilterTypes filterTypes = filterType;
				if ((uint)(filterTypes - 3) <= 2u)
				{
					return true;
				}
				return false;
			}
		}

		public SortingFilter Clone()
		{
			return new SortingFilter
			{
				filterType = filterType,
				sortBy = sortBy,
				descending = descending,
				levelType = levelType,
				restrictToUserId = restrictToUserId,
				restrictToGSID = restrictToGSID,
				allowUnpublished = allowUnpublished,
				cutoffDays = cutoffDays,
				codeList = codeList.ToList(),
				lowerDifficultyBound = lowerDifficultyBound,
				upperDifficultyBound = upperDifficultyBound,
				searchTerms = searchTerms,
				infoLine1 = infoLine1,
				infoLine2 = infoLine2,
				approvalStatusFilter = approvalStatusFilter,
				hideAcknowledged = hideAcknowledged,
				showMods = showMods
			};
		}
	}

	public UndergroundComputer undergroundComputer;

	public SortingFilter sortingFilter;
}
