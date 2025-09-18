using System;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Numerics;

internal class Matrix
{
	private const int FractionalDigitsToRoundTo = 10;

	private static readonly double ErrorTolerance = Math.Pow(0.1, 10.0);

	protected double[][] _MatrixRowValues;

	public int Rows { get; protected set; }

	public int Columns { get; protected set; }

	public double this[int row, int column] => _MatrixRowValues[row][column];

	public Matrix Transpose
	{
		get
		{
			double[][] array = new double[Columns][];
			for (int i = 0; i < Columns; i++)
			{
				double[] array2 = (array[i] = new double[Rows]);
				for (int j = 0; j < Rows; j++)
				{
					array2[j] = _MatrixRowValues[j][i];
				}
			}
			return new Matrix(Columns, Rows, array);
		}
	}

	private bool IsSquare
	{
		get
		{
			if (Rows == Columns)
			{
				return Rows > 0;
			}
			return false;
		}
	}

	public double Determinant
	{
		get
		{
			if (!IsSquare)
			{
				throw new NotSupportedException("Matrix must be square!");
			}
			if (Rows == 1)
			{
				return _MatrixRowValues[0][0];
			}
			if (Rows == 2)
			{
				double num = _MatrixRowValues[0][0];
				double num2 = _MatrixRowValues[0][1];
				double num3 = _MatrixRowValues[1][0];
				double num4 = _MatrixRowValues[1][1];
				return num * num4 - num2 * num3;
			}
			double num5 = 0.0;
			for (int i = 0; i < Columns; i++)
			{
				double num6 = _MatrixRowValues[0][i];
				double cofactor = GetCofactor(0, i);
				double num7 = num6 * cofactor;
				num5 += num7;
			}
			return num5;
		}
	}

	public Matrix Adjugate
	{
		get
		{
			if (!IsSquare)
			{
				throw new ArgumentException("Matrix must be square!");
			}
			if (Rows == 2)
			{
				double num = _MatrixRowValues[0][0];
				double num2 = _MatrixRowValues[0][1];
				double num3 = _MatrixRowValues[1][0];
				double num4 = _MatrixRowValues[1][1];
				return new SquareMatrix(num4, 0.0 - num2, 0.0 - num3, num);
			}
			double[][] array = new double[Columns][];
			for (int i = 0; i < Columns; i++)
			{
				array[i] = new double[Rows];
				for (int j = 0; j < Rows; j++)
				{
					array[i][j] = GetCofactor(j, i);
				}
			}
			return new Matrix(array);
		}
	}

	public Matrix Inverse
	{
		get
		{
			if (Rows == 1 && Columns == 1)
			{
				return new SquareMatrix(1.0 / _MatrixRowValues[0][0]);
			}
			return 1.0 / Determinant * Adjugate;
		}
	}

	protected Matrix()
	{
	}

	public Matrix(int rows, int columns, params double[] allRowValues)
	{
		Rows = rows;
		Columns = columns;
		_MatrixRowValues = new double[rows][];
		int num = 0;
		for (int i = 0; i < Rows; i++)
		{
			_MatrixRowValues[i] = new double[Columns];
			for (int j = 0; j < Columns; j++)
			{
				if (allRowValues != null && num < allRowValues.Length)
				{
					_MatrixRowValues[i][j] = allRowValues[num++];
				}
			}
		}
	}

	public Matrix(double[][] rowValues)
	{
		if (!rowValues.All((double[] row) => row.Length == rowValues[0].Length))
		{
			throw new ArgumentException("All rows must be the same length!");
		}
		Rows = rowValues.Length;
		Columns = rowValues[0].Length;
		_MatrixRowValues = rowValues;
	}

	protected Matrix(int rows, int columns, double[][] matrixRowValues)
	{
		Rows = rows;
		Columns = columns;
		_MatrixRowValues = matrixRowValues;
	}

	public Matrix(int rows, int columns, IEnumerable<IEnumerable<double>> columnValues)
		: this(rows, columns)
	{
		int num = 0;
		foreach (IEnumerable<double> columnValue in columnValues)
		{
			int num2 = 0;
			foreach (double item in columnValue)
			{
				_MatrixRowValues[num2++][num] = item;
			}
			num++;
		}
	}

	public static Matrix operator *(double scalarValue, Matrix matrix)
	{
		int rows = matrix.Rows;
		int columns = matrix.Columns;
		double[][] array = new double[rows][];
		for (int i = 0; i < rows; i++)
		{
			double[] array2 = (array[i] = new double[columns]);
			for (int j = 0; j < columns; j++)
			{
				array2[j] = scalarValue * matrix._MatrixRowValues[i][j];
			}
		}
		return new Matrix(rows, columns, array);
	}

	public static Matrix operator +(Matrix left, Matrix right)
	{
		if (left.Rows != right.Rows || left.Columns != right.Columns)
		{
			throw new ArgumentException("Matrices must be of the same size");
		}
		double[][] array = new double[left.Rows][];
		for (int i = 0; i < left.Rows; i++)
		{
			double[] array2 = (array[i] = new double[right.Columns]);
			for (int j = 0; j < right.Columns; j++)
			{
				array2[j] = left._MatrixRowValues[i][j] + right._MatrixRowValues[i][j];
			}
		}
		return new Matrix(left.Rows, right.Columns, array);
	}

	public static Matrix operator *(Matrix left, Matrix right)
	{
		if (left.Columns != right.Rows)
		{
			throw new ArgumentException("The width of the left matrix must match the height of the right matrix", "right");
		}
		int rows = left.Rows;
		int columns = right.Columns;
		double[][] array = new double[rows][];
		for (int i = 0; i < rows; i++)
		{
			array[i] = new double[columns];
			for (int j = 0; j < columns; j++)
			{
				double num = 0.0;
				for (int k = 0; k < left.Columns; k++)
				{
					double num2 = left._MatrixRowValues[i][k];
					double num3 = right._MatrixRowValues[k][j];
					double num4 = num2 * num3;
					num += num4;
				}
				array[i][j] = num;
			}
		}
		return new Matrix(rows, columns, array);
	}

	private Matrix GetMinorMatrix(int rowToRemove, int columnToRemove)
	{
		double[][] array = new double[Rows - 1][];
		int num = 0;
		for (int i = 0; i < Rows; i++)
		{
			if (i == rowToRemove)
			{
				continue;
			}
			array[num] = new double[Columns - 1];
			int num2 = 0;
			for (int j = 0; j < Columns; j++)
			{
				if (j != columnToRemove)
				{
					array[num][num2] = _MatrixRowValues[i][j];
					num2++;
				}
			}
			num++;
		}
		return new Matrix(Rows - 1, Columns - 1, array);
	}

	private double GetCofactor(int rowToRemove, int columnToRemove)
	{
		if ((rowToRemove + columnToRemove) % 2 == 0)
		{
			return GetMinorMatrix(rowToRemove, columnToRemove).Determinant;
		}
		return -1.0 * GetMinorMatrix(rowToRemove, columnToRemove).Determinant;
	}

	public static bool operator ==(Matrix a, Matrix b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.Rows != b.Rows || a.Columns != b.Columns)
		{
			return false;
		}
		for (int i = 0; i < a.Rows; i++)
		{
			for (int j = 0; j < a.Columns; j++)
			{
				if (Math.Abs(a._MatrixRowValues[i][j] - b._MatrixRowValues[i][j]) > ErrorTolerance)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool operator !=(Matrix a, Matrix b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		double num = Rows;
		num += (double)(2 * Columns);
		for (int i = 0; i < Rows; i++)
		{
			double num2 = ((i % 2 == 0) ? 1.0 : 2.0);
			for (int j = 0; j < Columns; j++)
			{
				double num3 = Math.Round(_MatrixRowValues[i][j], 10);
				num += num2 * num3;
			}
		}
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] array = new byte[4];
		for (int k = 0; k < 4; k++)
		{
			array[k] = (byte)(bytes[k] ^ bytes[k + 4]);
		}
		return BitConverter.ToInt32(array, 0);
	}

	public override bool Equals(object obj)
	{
		Matrix matrix = obj as Matrix;
		if (matrix == null)
		{
			return base.Equals(obj);
		}
		return this == matrix;
	}
}
