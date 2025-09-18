namespace Moserware.Numerics;

internal class IdentityMatrix : DiagonalMatrix
{
	public IdentityMatrix(int rows)
		: base(CreateDiagonal(rows))
	{
	}

	private static double[] CreateDiagonal(int rows)
	{
		double[] array = new double[rows];
		for (int i = 0; i < rows; i++)
		{
			array[i] = 1.0;
		}
		return array;
	}
}
