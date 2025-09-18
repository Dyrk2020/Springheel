public static class TabletRuleUtility
{
	public static bool IsRuleModifierChange(TabletRule rule)
	{
		if (rule >= TabletRule.ModifierGravity && rule != TabletRule.ModifierForceLobbyModifiers)
		{
			return true;
		}
		if ((uint)(rule - 1) <= 5u || (uint)(rule - 28) <= 1u || rule == TabletRule.CompetitiveRandomizer)
		{
			return true;
		}
		return false;
	}
}
