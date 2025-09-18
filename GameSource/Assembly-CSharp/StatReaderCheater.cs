using UnityEngine.EventSystems;

public class StatReaderCheater : StatReader, IPointerClickHandler, IEventSystemHandler
{
	public bool Cheated;

	protected override string getValue()
	{
		if (!StatTracker.Instance.GetSaveFileDataForMainUser().IsCheater)
		{
			return "N";
		}
		return "Y";
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		StatBool stat = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBool>("Cheater");
		stat.Set(!stat.value);
		TextField.text = getValue();
	}
}
