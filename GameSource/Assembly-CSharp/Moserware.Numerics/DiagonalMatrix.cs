using System.Collections.Generic;

namespace Moserware.Numerics;

internal class DiagonalMatrix : Matrix
{
	public DiagonalMatrix(IList<double> diagonalValues)
		: base(diagonalValues.Count, diagonalValues.Count)
	{
		for (int i = 0; i < diagonalValues.Count; i++)
		{
			_MatrixRowValues[i][i] = diagonalValues[i];
		}
	}
}
