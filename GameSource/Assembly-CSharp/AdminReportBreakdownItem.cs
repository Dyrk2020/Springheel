using UnityEngine;
using UnityEngine.UI;

public class AdminReportBreakdownItem : MonoBehaviour
{
	public Text reportTypeText;

	public Text reportCountText;

	public void Initialize(string reason, int count)
	{
		reportTypeText.text = reason;
		reportCountText.text = count.ToString();
	}
}
