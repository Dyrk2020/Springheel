using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs;

public class FactorList<TValue>
{
	private readonly List<Factor<TValue>> _List = new List<Factor<TValue>>();

	public double LogNormalization
	{
		get
		{
			_List.ForEach(delegate(Factor<TValue> f)
			{
				f.ResetMarginals();
			});
			double num = 0.0;
			for (int num2 = 0; num2 < _List.Count; num2++)
			{
				Factor<TValue> factor = _List[num2];
				for (int num3 = 0; num3 < factor.NumberOfMessages; num3++)
				{
					num += factor.SendMessage(num3);
				}
			}
			double num4 = 0.0;
			for (int num5 = 0; num5 != _List.Count; num5++)
			{
				num4 += _List[num5].LogNormalization;
			}
			return num + num4;
		}
	}

	public int Count => _List.Count;

	public Factor<TValue> AddFactor(Factor<TValue> factor)
	{
		_List.Add(factor);
		return factor;
	}
}
