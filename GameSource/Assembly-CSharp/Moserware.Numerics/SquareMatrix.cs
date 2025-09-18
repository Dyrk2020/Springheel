using System;

namespace Moserware.Numerics;

internal class SquareMatrix : Matrix
{
	public SquareMatrix(params double[] allValues)
	{
		base.Rows = (int)Math.Sqrt(allValues.Length);
		base.Columns = base.Rows;
		int num = 0;
		_MatrixRowValues = new double[base.Rows][];
		for (int i = 0; i < base.Rows; i++)
		{
			double[] array = new double[base.Columns];
			_MatrixRowValues[i] = array;
			for (int j = 0; j < base.Columns; j++)
			{
				array[j] = allValues[num++];
			}
		}
	}
}
